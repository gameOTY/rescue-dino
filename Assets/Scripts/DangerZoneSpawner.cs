using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DangerZoneSpawner : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private TilemapSpawnArea spawnArea;
  [SerializeField] private PrefabPool dangerZonePool;
  [SerializeField] private GameManager gameManager;
  [SerializeField] private GameObject player;
  [SerializeField] private SoldierTracker soldierTracker;

  [Header("Danger Zone Rules")]
  [SerializeField] private GameConfig gameConfig;

  private Coroutine spawnCoroutine;
  private readonly HashSet<Transform> activeZones = new();

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

  private IEnumerator SpawnLoop()
  {
    if (gameConfig == null)
    {
      Debug.LogError("[DangerZoneSpawner] GameConfig was not assigned in the Inspector.");
      yield break;
    }

    while (!gameManager.IsTerminal)
    {
      yield return WaitForGameplaySeconds(gameConfig.DangerZoneSpawnInterval);

      if (gameManager.IsPaused)
        continue;

      if (!spawnArea.IsReady)
      {
        yield return null;
        continue;
      }

      if (dangerZonePool.CountActive >= gameConfig.MaxActiveDangerZones)
        continue;

      SpawnZone();
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

  private void SpawnZone()
  {
    if (soldierTracker == null)
    {
      Debug.LogWarning("[DangerZoneSpawner] SoldierTracker not assigned, spawning without rescue zone exclusion.");
      SpawnZoneNoExclusion();
      return;
    }

    float rescueZoneRadius = gameConfig.RescueZoneDiameter / 2f;
    HashSet<Vector3Int> excludedCells = soldierTracker.GetRescueZoneCells(rescueZoneRadius, spawnArea);

    if (!spawnArea.GetRandomWalkablePosition(out Vector3 position, excludedCells))
      return;

    var zone = dangerZonePool.Get();
    zone.transform.position = position;
    var dangerZoneController = zone.GetComponent<DangerZoneController>();
    dangerZoneController.Initialize(gameConfig);
    dangerZoneController.PlayerDamaged += OnDangerZonePlayerDamaged;
    dangerZoneController.Completed += OnDangerZoneCompleted;
    activeZones.Add(zone.transform);
  }

  private void SpawnZoneNoExclusion()
  {
    if (!spawnArea.GetRandomWalkablePosition(out Vector3 position))
      return;

    var zone = dangerZonePool.Get();
    zone.transform.position = position;
    var dangerZoneController = zone.GetComponent<DangerZoneController>();
    dangerZoneController.Initialize(gameConfig);
    dangerZoneController.PlayerDamaged += OnDangerZonePlayerDamaged;
    dangerZoneController.Completed += OnDangerZoneCompleted;
    activeZones.Add(zone.transform);
  }

  private void OnDangerZonePlayerDamaged(DangerZoneController dangerZone)
  {
    gameManager.RegisterDamage(1, "Player entered a danger zone");
    dangerZone.PlayerDamaged -= OnDangerZonePlayerDamaged;
    dangerZone.Completed -= OnDangerZoneCompleted;
    activeZones.Remove(dangerZone.transform);
  }

  private void OnDangerZoneCompleted(DangerZoneController dangerZone)
  {
    dangerZone.Completed -= OnDangerZoneCompleted;
    activeZones.Remove(dangerZone.transform);
  }

  public HashSet<Vector3Int> GetDangerZoneCells(TilemapSpawnArea spawnArea)
  {
    var cells = new HashSet<Vector3Int>();
    foreach (var zone in activeZones)
    {
      if (zone == null) continue;
      cells.Add(spawnArea.WorldToCell(zone.position));
    }
    return cells;
  }
}
