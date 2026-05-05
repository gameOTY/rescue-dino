using System;
using System.Collections;
using UnityEngine;

public class SoliderController : MonoBehaviour
{
  public enum RescueSoliderResult { Rescued, Dead }

  [SerializeField] private float lifetime = 10f;

  private bool isCompleted;
  private int currentTransitionId;
  private PrefabPool objectPool;
  private Coroutine lifetimeCoroutine;

  public event Action<SoliderController, RescueSoliderResult> Completed;

  /// <summary>
  /// Rescue duration is read by RescueZoneController child. Set during Initialize.
  /// </summary>
  public float RescueDuration { get; private set; }

  public void SetPool(PrefabPool pool) => objectPool = pool;

  public void Initialize(float rescueDuration)
  {
    RescueDuration = rescueDuration;
    currentTransitionId++;
    isCompleted = false;
    lifetimeCoroutine = StartCoroutine(LifetimeCountdown());
  }

  /// <summary>
  /// Called by RescueZoneController when player holds interaction long enough.
  /// </summary>
  public void HandleAuthoritativeRescue()
  {
    StopCoroutineSafe(ref lifetimeCoroutine);
    Complete(RescueSoliderResult.Rescued);
  }

  private void OnDisable()
  {
    StopCoroutineSafe(ref lifetimeCoroutine);
    currentTransitionId = 0;
  }

  private IEnumerator LifetimeCountdown()
  {
    yield return new WaitForSeconds(lifetime);
    Complete(RescueSoliderResult.Dead);
  }

  private void Complete(RescueSoliderResult result)
  {
    if (isCompleted) return;

    isCompleted = true;
    currentTransitionId++;
    StopCoroutineSafe(ref lifetimeCoroutine);

    Completed?.Invoke(this, result);
    Release();
  }

  private void Release()
  {
    if (objectPool != null)
      objectPool.Release(gameObject);
    else
      gameObject.SetActive(false);
  }

  private void StopCoroutineSafe(ref Coroutine coroutine)
  {
    if (coroutine == null) return;
    StopCoroutine(coroutine);
    coroutine = null;
  }
}
