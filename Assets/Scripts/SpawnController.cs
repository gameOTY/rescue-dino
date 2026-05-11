using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SoldierTracker))]
public class SpawnController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TilemapSpawnArea spawnArea;
    [SerializeField] private PrefabPool soliderPool;
    [SerializeField] private GameManager gameManager;
  [SerializeField] private SoldierTracker soldierTracker;

  [Header("Solider Spawn Settings")]
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float spawnIntervalDecayPerSecond = 0.1f;
    [SerializeField] private float minimumSpawnInterval = 0.5f;

    [SerializeField] private int maxActiveSoldiers = 3;

    [SerializeField] private float rescueDuration = 3f;

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
        return Mathf.Max(minimumSpawnInterval, spawnInterval - spawnIntervalDecayPerSecond * elapsedTime);
    }

    private IEnumerator SpawnLoop()
    {
        if (soliderPool == null)
        {
            Debug.LogError("[SpawnController] SoliderPool was not assigned in the Inspector.");
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

            if (activeTargetCount >= maxActiveSoldiers)
            {
                continue;
            }

            SpawnSoldier();
        }
    }

    private void SpawnSoldier()
    {
        if (activeTargetCount >= maxActiveSoldiers)
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

        GameObject spawnedObject = soliderPool.Get();
        spawnedObject.transform.position = spawnPosition;
    var soliderController = spawnedObject.GetComponent<SoldierController>();
    soliderController.Initialize(rescueDuration);
    soliderController.Completed += OnTargetCompleted;



    if (spawnedObject.TryGetComponent<TargetIndicatorController>(out var indicator))
    {
      indicator.Initialize(rescueDuration, rescueDuration);
    }

    if (soldierTracker != null)
      soldierTracker.RegisterSoldier(spawnedObject.transform);
  }


  private void OnTargetCompleted(SoldierController target, SoldierController.RescueSoliderResult result)
  {
    target.Completed -= OnTargetCompleted;

    if (soldierTracker != null)
      soldierTracker.UnregisterSoldier(target.transform);

    Vector3Int targetCell = spawnArea.WorldToCell(target.transform.position);
        occupiedCells.Remove(targetCell);

        activeTargetCount--;

        switch (result)
        {
      case SoldierController.RescueSoliderResult.Rescued:
        gameManager.RegisterRescued();
                break;

      case SoldierController.RescueSoliderResult.Dead:
        gameManager.RegisterDead();
                break;
        }
    }
}
