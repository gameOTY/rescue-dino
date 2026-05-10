using System;
using System.Collections;
using UnityEngine;

public class SoliderController : MonoBehaviour
{
  public enum RescueSoliderResult { Rescued, Dead }

  [SerializeField] private float lifetime = 10f;
  [SerializeField] private Animator animator;

  private bool isCompleted;
  private bool isInitialized;
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
    isInitialized = true;
    isCompleted = false;
    RescueDuration = rescueDuration;
    currentTransitionId++;
    lifetimeCoroutine = StartCoroutine(LifetimeCountdown());

    if (animator == null)
      animator = GetComponent<Animator>();
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
    if (!isInitialized) return;
    if (!isCompleted)
    {
      Complete(RescueSoliderResult.Dead);
    }
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

    if (animator != null)
      animator.SetTrigger(result == RescueSoliderResult.Rescued ? "Rescued" : "Dead");

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
