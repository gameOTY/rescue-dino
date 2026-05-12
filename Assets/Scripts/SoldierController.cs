using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SoldierController : MonoBehaviour
{
  public enum RescueSoldierResult { Rescued, Dead }

  [SerializeField] private Animator animator;

  private GameConfig gameConfig;
  private bool isCompleted;
  private bool isInitialized;
  private int currentTransitionId;
  private PrefabPool objectPool;
  private Coroutine lifetimeCoroutine;

  public event Action<SoldierController, RescueSoldierResult> Completed;

  /// <summary>
  /// Rescue duration is read by RescueZoneController child. Set during Initialize.
  /// </summary>
  public float RescueDuration { get; private set; }

  public void SetPool(PrefabPool pool) => objectPool = pool;

  public void Initialize(GameConfig config)
  {
    gameConfig = config;
    isInitialized = false;
    isCompleted = true;

    if (gameConfig == null)
    {
      Debug.LogError("[SoldierController] GameConfig was not provided.");
      Release();
      return;
    }

    isInitialized = true;
    isCompleted = false;
    RescueDuration = gameConfig.RescueTime;
    currentTransitionId++;
    lifetimeCoroutine = StartCoroutine(LifetimeCountdown());

    if (animator == null)
      animator = GetComponent<Animator>();
  }

  public void HandleAuthoritativeRescue()
  {
    StopCoroutineSafe(ref lifetimeCoroutine);
    Complete(RescueSoldierResult.Rescued);
  }

  private void OnDisable()
  {
    if (!isInitialized) return;
    if (!isCompleted)
    {
      Complete(RescueSoldierResult.Dead);
    }
    currentTransitionId = 0;
  }

  private IEnumerator LifetimeCountdown()
  {
    yield return new WaitForSeconds(gameConfig.SoldierLifetime);
    Complete(RescueSoldierResult.Dead);
  }

  private void Complete(RescueSoldierResult result)
  {
    if (isCompleted) return;

    isCompleted = true;
    currentTransitionId++;
    StopCoroutineSafe(ref lifetimeCoroutine);

    if (animator != null)
      animator.SetTrigger(result == RescueSoldierResult.Rescued ? "Rescued" : "Dead");

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
