using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SoldierTracker))]
public class SpawnController : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private TilemapSpawnArea spawnArea;
  [SerializeField] private PrefabPool soldierPool;
  [SerializeField] private GameManager gameManager;
  [SerializeField] private SoldierTracker soldierTracker;
  [SerializeField] private DangerZoneSpawner dangerZoneSpawner;
  [SerializeField] private GameConfig gameConfig;

  public GameConfig Config => gameConfig;

  private int activeTargetCount;
  private readonly HashSet<Vector3Int> occupiedCells = new();

  private Coroutine spawnCoroutine;

  private void OnEnable()
  {
    spawnCoroutine = StartCoroutine(SpawnLoop());
  }

  private void OnDisable()
  {
    if (spawnCoroutine != null)
    {
      StopCoroutine(spawnCoroutine);
      spawnCoroutine = null;
    }
  }

  private float GetCurrentSpawnInterval()
  {
    // Spawn decay formula: interval = max(MinSpawnInterval, SoldierSpawnInterval - SoliderSpawnDecayRate * elapsedTime)
    // At 30s: soldier interval = 5 - 0.1*30 = 2s (equals danger zone's 2s interval)
    // At 60s: interval = 5 - 0.1*60 = 5 - 6 = -1, clamped to 0.5s floor (4x faster than danger zone)
    //
    // The 0.5s floor prevents impossible spawn rates.
    // Target: 1s floor would maintain 2:1 ratio against danger zone's 2s interval.
    float elapsedTime = gameManager != null ? gameManager.TotalTime - gameManager.TimeRemaining : Time.timeSinceLevelLoad;
    return Mathf.Max(
      Config.MinSpawnInterval,
      Config.SoldierSpawnInterval - Config.SoliderSpawnDecayRate * elapsedTime
    );
  }

  private IEnumerator SpawnLoop()
  {
    if (Config == null)
    {
      Debug.LogError("[SpawnController] GameConfig was not assigned in the Inspector.");
      yield break;
    }

    if (soldierPool == null)
    {
      Debug.LogError("[SpawnController] SoldierPool was not assigned in the Inspector.");
      yield break;
    }

    while (!gameManager.IsTerminal)
    {
      if (gameManager == null) yield break;

      yield return WaitForGameplaySeconds(GetCurrentSpawnInterval());

      if (gameManager.IsPaused)
        continue;

      if (!spawnArea.IsReady)
      {
        yield return null;
        continue;
      }

      if (activeTargetCount >= Config.MaxActiveSoldiers) continue;

      SpawnSoldier();
    }
  }

  private IEnumerator WaitForGameplaySeconds(float seconds)
  {
    float elapsed = 0f;
    while (elapsed < seconds)
    {
      if (gameManager == null || !gameManager.IsPaused)
        elapsed += Time.deltaTime;

      yield return null;
    }
  }

  private void SpawnSoldier()
  {
    // Exclusion policy: soldiers are NOT spawned inside active danger zone cells.
    // This protects newly-spawned soldiers from instant damage.
    //
    // Note: Danger zones do NOT exclude rescue zones when spawning — this is intentional asymmetry.
    // Reason: rescue zones are transient (player-activated) and blocking them would reduce spawn options
    // as gameplay progresses. The danger zone's short lifetime (3s) means exclusion is brief.
    if (activeTargetCount >= Config.MaxActiveSoldiers)
    {
      return;
    }

    bool hasPosition = spawnArea.GetRandomWalkablePosition(
      out Vector3 spawnPosition,
      occupiedCells
    );

    if (!hasPosition)
    {
      Debug.LogWarning("Cannot spawn target. No valid spawn position.");
      return;
    }

    Vector3Int spawnCell = spawnArea.WorldToCell(spawnPosition);

    if (dangerZoneSpawner != null)
    {
      HashSet<Vector3Int> dangerZoneCells = dangerZoneSpawner.GetDangerZoneCells(spawnArea);

      if (dangerZoneCells.Contains(spawnCell))
      {
        Debug.LogWarning("Cannot spawn soldier in a danger zone cell.");
        return;
      }
    }

    activeTargetCount++;
    occupiedCells.Add(spawnCell);

    GameObject spawnedObject = soldierPool.Get();
    spawnedObject.transform.position = spawnPosition;
    var soldierController = spawnedObject.GetComponent<SoldierController>();
    soldierController.Initialize(Config);
    soldierController.Completed += OnTargetCompleted;

    if (spawnedObject.TryGetComponent<TargetIndicatorController>(out var indicator))
    {
      indicator.Initialize(Config.RescueTime, Config.SoldierLifetime);
    }

    if (soldierTracker != null)
      soldierTracker.RegisterSoldier(spawnedObject.transform);
  }

  private void OnTargetCompleted(SoldierController target, SoldierController.RescueSoldierResult result)
  {
    target.Completed -= OnTargetCompleted;

    if (soldierTracker != null)
      soldierTracker.UnregisterSoldier(target.transform);

    Vector3Int targetCell = spawnArea.WorldToCell(target.transform.position);
    occupiedCells.Remove(targetCell);

    activeTargetCount--;

    switch (result)
    {
      case SoldierController.RescueSoldierResult.Rescued:
        gameManager?.RegisterRescued();
        break;

      case SoldierController.RescueSoldierResult.Dead:
        gameManager?.RegisterDead();
        break;
    }
  }
}
