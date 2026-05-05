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
    [SerializeField] private float dangerZoneSpawnInterval = 2f;
    [SerializeField] private float dangerZoneLifetime = 3f;
    [SerializeField] private int maxActiveDangerZones = 5;

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
        while (!gameManager.IsTerminal)
        {
            yield return new WaitForSeconds(dangerZoneSpawnInterval);

            if (!spawnArea.IsReady)
            {
                yield return null;
                continue;
            }

            if (dangerZonePool.CountActive >= maxActiveDangerZones)
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
        dangerZoneController.Initialize(dangerZoneLifetime);
        dangerZoneController.PlayerDamaged += OnDangerZonePlayerDamaged;
    }

    private void OnDangerZonePlayerDamaged(DangerZoneController dangerZone)
    {
        dangerZone.PlayerDamaged -= OnDangerZonePlayerDamaged;
        gameManager.RegisterDamage(1, "Player entered a danger zone");
    }
}
