using System.Collections.Generic;
using UnityEngine;

public class SoldierTracker : MonoBehaviour
{
    private readonly List<Transform> _activeSoldiers = new();

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
