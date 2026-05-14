using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SoldierController : MonoBehaviour
{
  public enum RescueSoldierResult { Rescued, Dead }

  private static readonly int IdleStateHash = Animator.StringToHash("Warrior_Idle_Blue");
  private static readonly int DeadTriggerHash = Animator.StringToHash("Dead");

  [SerializeField] private Animator animator;
  [SerializeField] private float animationDuration = 0.5f;

  private GameConfig gameConfig;
  private bool isCompleted;
  private int currentTransitionId;
  private PrefabPool objectPool;
  private Coroutine lifetimeCoroutine;
  private Coroutine releaseCoroutine;
  private Collider2D[] colliders;

  public event Action<SoldierController, RescueSoldierResult> Completed;

  /// <summary>
  /// Rescue duration is read by RescueZoneController child. Set during Initialize.
  /// </summary>
  public float RescueDuration { get; private set; }

  public void SetPool(PrefabPool pool) => objectPool = pool;

  public void Initialize(GameConfig config)
  {
    gameConfig = config;
    isCompleted = true;

    if (gameConfig == null)
    {
      Debug.LogError("[SoldierController] GameConfig was not provided.");
      Release();
      return;
    }

    isCompleted = false;
    RescueDuration = gameConfig.RescueTime;
    currentTransitionId++;
    StopCoroutineSafe(ref releaseCoroutine);
    SetCollidersEnabled(true);

    if (animator == null)
      animator = GetComponent<Animator>();

    PlayAnimationState(IdleStateHash);
    lifetimeCoroutine = StartCoroutine(LifetimeCountdown());
  }

  public void HandleAuthoritativeRescue()
  {
    StopCoroutineSafe(ref lifetimeCoroutine);
    Complete(RescueSoldierResult.Rescued);
  }

  private void OnDisable()
  {
    StopCoroutineSafe(ref lifetimeCoroutine);
    StopCoroutineSafe(ref releaseCoroutine);
    currentTransitionId = 0;
  }

  private IEnumerator LifetimeCountdown()
  {
    yield return WaitForGameplaySeconds(gameConfig.SoldierLifetime);
    Complete(RescueSoldierResult.Dead);
  }

  private void Complete(RescueSoldierResult result)
  {
    if (isCompleted) return;

    isCompleted = true;
    StopCoroutineSafe(ref lifetimeCoroutine);
    SetCollidersEnabled(false);

    if (result == RescueSoldierResult.Rescued)
      Debug.Log("[SoldierController] Soldier rescued!");
    else
      animator.SetTrigger(DeadTriggerHash);

    Completed?.Invoke(this, result);

    if (!isActiveAndEnabled)
    {
      releaseCoroutine = null;
      return;
    }

    if (result == RescueSoldierResult.Rescued) Release(); // Immediate release for rescued soldiers
    else
      releaseCoroutine = StartCoroutine(DelayedRelease(animationDuration, currentTransitionId));
  }

  private IEnumerator DelayedRelease(float delay, int transitionId)
  {
    yield return WaitForGameplaySeconds(delay);

    if (transitionId != currentTransitionId)
      yield break;

    releaseCoroutine = null;
    Release();
  }

  private IEnumerator WaitForGameplaySeconds(float seconds)
  {
    float elapsed = 0f;
    while (elapsed < seconds)
    {
      if (!IsGamePaused())
        elapsed += Time.deltaTime;

      yield return null;
    }
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

  private void SetCollidersEnabled(bool isEnabled)
  {
    colliders ??= GetComponentsInChildren<Collider2D>();

    foreach (Collider2D collider in colliders)
      collider.enabled = isEnabled;
  }

  private void PlayAnimationState(int stateHash)
  {
    if (animator == null)
      return;

    animator.Play(stateHash, 0, 0f);
    animator.Update(0f);
  }

  private bool IsGamePaused()
  {
    return GameManager.Instance != null && GameManager.Instance.IsPaused;
  }
}
