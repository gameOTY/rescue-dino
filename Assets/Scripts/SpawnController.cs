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
    float elapsedTime = Time.timeSinceLevelLoad;
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
      yield return new WaitForSeconds(GetCurrentSpawnInterval());

      if (!spawnArea.IsReady)
      {
        yield return null;
        continue;
      }

      if (activeTargetCount >= Config.MaxActiveSoldiers) continue;

      SpawnSoldier();
    }
  }

  private void SpawnSoldier()
  {
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

    activeTargetCount++;
    Vector3Int spawnCell = spawnArea.WorldToCell(spawnPosition);
    occupiedCells.Add(spawnCell);

    GameObject spawnedObject = soldierPool.Get();
    spawnedObject.transform.position = spawnPosition;
    var soldierController = spawnedObject.GetComponent<SoldierController>();
    soldierController.Initialize(Config);
    soldierController.Completed += OnTargetCompleted;

    if (spawnedObject.TryGetComponent<TargetIndicatorController>(out var indicator))
    {
      if (indicator == null)
      {
        Debug.LogWarning($"[SpawnController] Spawned object '{spawnedObject.name}' does not have a TargetIndicatorController component.");
      }
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
        gameManager.RegisterRescued();
        break;

      case SoldierController.RescueSoldierResult.Dead:
        gameManager.RegisterDead();
        break;
    }
  }
}
