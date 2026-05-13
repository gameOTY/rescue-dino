using System.Collections.Generic;
using UnityEngine;

public class SoldierTracker : MonoBehaviour
{
    private readonly List<Transform> _activeSoldiers = new();

    public HashSet<Vector3Int> GetRescueZoneCells(float rescueZoneRadius, TilemapSpawnArea spawnArea)
    {
        var cells = new HashSet<Vector3Int>();

        foreach (var soldier in _activeSoldiers)
        {
            if (soldier == null) continue;

            Vector3 worldPos = soldier.position;
            Vector3Int centerCell = spawnArea.WorldToCell(worldPos);

            cells.Add(centerCell);

            float radiusInCells = rescueZoneRadius / spawnArea.CellSize;
            int radiusInt = Mathf.CeilToInt(radiusInCells);

            for (int dx = -radiusInt; dx <= radiusInt; dx++)
            {
                for (int dy = -radiusInt; dy <= radiusInt; dy++)
                {
                    cells.Add(new Vector3Int(centerCell.x + dx, centerCell.y + dy, 0));
                }
            }
        }

        return cells;
    }

    public bool TryGetNearestSoldier(Transform player, out Vector3 nearestPosition)
    {
        nearestPosition = Vector3.zero;

        if (_activeSoldiers.Count == 0)
            return false;

        Transform nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var soldier in _activeSoldiers)
        {
            if (soldier == null) continue;
            float dist = Vector3.Distance(player.position, soldier.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = soldier;
            }
        }

        if (nearest == null)
            return false;

        nearestPosition = nearest.position;
        return true;
    }

    public void RegisterSoldier(Transform soldier)
    {
        if (! _activeSoldiers.Contains(soldier))
            _activeSoldiers.Add(soldier);
    }

    public void UnregisterSoldier(Transform soldier)
    {
        _activeSoldiers.Remove(soldier);
    }

    public int ActiveCount => _activeSoldiers.Count;
}
