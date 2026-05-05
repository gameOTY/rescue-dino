using System;
using UnityEngine;

/// <summary>
/// Child zone component placed on a child GameObject with a Trigger Collider2D.
/// Detects player entry and notifies the parent DangerZoneController.
/// </summary>
public class DangerZoneAreaController : MonoBehaviour
{
    private DangerZoneController parentZone;

    private void Awake()
    {
        parentZone = GetComponentInParent<DangerZoneController>();
        if (parentZone == null)
        {
            Debug.LogError("[DangerZoneAreaController] No DangerZoneController found in parent.", this);
            return;
        }

        if (!GetComponent<Collider2D>().isTrigger)
        {
            Debug.LogWarning("[DangerZoneAreaController] Collider2D should be set as IsTrigger.", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        parentZone.OnPlayerEnteredZone();
    }
}
