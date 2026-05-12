using System;
using UnityEngine;
using RescueGame;

public class TargetIndicatorController : MonoBehaviour
{
    [SerializeField] private RescueProgressIndicator rescueProgressIndicator;
    [SerializeField] private TargetLifetimeIndicator targetLifetimeIndicator;
    [SerializeField] private RescueZoneController rescueZoneController;
    [SerializeField] private SoldierController soldierController;

    private bool _isRescued = false;
    private bool _isVisible = false;

    public event Action Rescued;

    public void Initialize(float rescueDuration, float lifetime)
    {
        _isRescued = false;

        if (rescueProgressIndicator != null)
            rescueProgressIndicator.Initialize(rescueDuration);

        if (targetLifetimeIndicator != null)
            targetLifetimeIndicator.Initialize(lifetime);

        WireEvents();
    }

    private void WireEvents()
    {
        if (rescueZoneController != null)
        {
            rescueZoneController.RescueCompleted += OnRescueCompleted;
            rescueZoneController.PlayerEntered += OnPlayerEntered;
            rescueZoneController.PlayerExited += OnPlayerExited;
        }

        if (soldierController != null)
            soldierController.Completed += OnSoldierCompleted;
    }

    private void OnPlayerEntered()
    {
        if (rescueZoneController != null && rescueProgressIndicator != null)
        {
            float elapsed = rescueZoneController.ElapsedRescueTime;
            float duration = rescueZoneController.GetRescueDuration();
            if (elapsed > 0f)
                rescueProgressIndicator.ResumeProgress(elapsed, duration);
            else
                rescueProgressIndicator.StartProgress(duration);
        }
    }

    private void OnPlayerExited()
    {
        rescueProgressIndicator?.PauseProgress();
    }

    private void OnRescueCompleted(RescueZoneController zone)
    {
        _isRescued = true;
        targetLifetimeIndicator?.StopCountdown();
        Rescued?.Invoke();
    }

  private void OnSoldierCompleted(SoldierController soldier, SoldierController.RescueSoldierResult result)
  {
    if (result == SoldierController.RescueSoldierResult.Dead && !_isRescued)
    {
            HandleDeathSequence();
        }
    }

    private void HandleDeathSequence()
    {
        if (rescueProgressIndicator != null)
            rescueProgressIndicator.gameObject.SetActive(false);

        if (targetLifetimeIndicator != null)
            targetLifetimeIndicator.gameObject.SetActive(false);
    }

    private void OnBecameVisible()
    {
        if (Camera.current != null && Camera.current.name == "Main Camera")
        {
            _isVisible = true;
        }
    }

    private void OnBecameInvisible()
    {
        _isVisible = false;
    }

    private void OnDisable()
    {
        UnwireEvents();
    }

    private void UnwireEvents()
    {
        if (rescueZoneController != null)
        {
            rescueZoneController.RescueCompleted -= OnRescueCompleted;
            rescueZoneController.PlayerEntered -= OnPlayerEntered;
            rescueZoneController.PlayerExited -= OnPlayerExited;
        }

        if (soldierController != null)
            soldierController.Completed -= OnSoldierCompleted;
    }
}
