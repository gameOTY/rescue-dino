using System.Collections;
using UnityEngine;

public class DangerZoneSpawner : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private TilemapSpawnArea spawnArea;
  [SerializeField] private PrefabPool dangerZonePool;
  [SerializeField] private GameManager gameManager;
  [SerializeField] private GameObject player;

  [Header("Danger Zone Rules")]
  [SerializeField] private GameConfig gameConfig;

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

  private IEnumerator SpawnLoop()
  {
    if (gameConfig == null)
    {
      Debug.LogError("[DangerZoneSpawner] GameConfig was not assigned in the Inspector.");
      yield break;
    }

    while (!gameManager.IsTerminal)
    {
      yield return new WaitForSeconds(gameConfig.DangerZoneSpawnInterval);

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

  private void SpawnZone()
  {
    if (!spawnArea.GetRandomWalkablePosition(out Vector3 position))
      return;

    var zone = dangerZonePool.Get();
    zone.transform.position = position;
    var dangerZoneController = zone.GetComponent<DangerZoneController>();
    dangerZoneController.Initialize(gameConfig);
    dangerZoneController.PlayerDamaged += OnDangerZonePlayerDamaged;
  }

  private void OnDangerZonePlayerDamaged(DangerZoneController dangerZone)
  {
    gameManager.RegisterDamage(1, "Player entered a danger zone");
    dangerZone.PlayerDamaged -= OnDangerZonePlayerDamaged;
  }
}
