using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D), typeof(Animator))]
public class DangerZoneController : MonoBehaviour
{
  private GameConfig gameConfig;
  private PrefabPool objectPool;
  private SpriteRenderer sr;
  private Coroutine lifetimeCoroutine;
  private Coroutine releaseCoroutine;
  private bool hasDamaged;

  private readonly int HitTriggerHash = Animator.StringToHash("Hit");

  [SerializeField] private float hitAnimationDuration = 1.5f;
  [SerializeField] private bool spriteFacesRightByDefault = true;

  private Animator animator;

  public event Action<DangerZoneController> PlayerDamaged;
  public event Action<DangerZoneController> Completed;

  public void SetPool(PrefabPool pool) => objectPool = pool;

  public void Initialize(GameConfig config)
  {
    gameConfig = config;
    hasDamaged = false;
    StopCoroutineSafe(ref releaseCoroutine);

    if (gameConfig == null)
    {
      Debug.LogError("[DangerZoneController] GameConfig was not provided.");
      Release();
      return;
    }

    lifetimeCoroutine = StartCoroutine(LifetimeRoutine());
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (!other.CompareTag("Player")) return;
    FacePlayer(GetColliderRoot(other));

    if (hasDamaged) return;
    hasDamaged = true;
    DamageAndRelease();
  }

  private IEnumerator LifetimeRoutine()
  {
    yield return new WaitForSeconds(gameConfig.DangerZoneLifetime);
    Release();
  }

  private void DamageAndRelease()
  {
    StopCoroutineSafe(ref lifetimeCoroutine);
    animator.SetTrigger(HitTriggerHash);
    PlayerDamaged?.Invoke(this);

    if (hitAnimationDuration > 0f && isActiveAndEnabled)
    {
      releaseCoroutine = StartCoroutine(DelayedRelease(hitAnimationDuration));
      return;
    }

    // Release();
  }

  private IEnumerator DelayedRelease(float delay)
  {
    yield return new WaitForSeconds(delay);

    releaseCoroutine = null;
    Release();
  }

  private void Release()
  {
    Completed?.Invoke(this);
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

  private void Awake()
  {
    sr = GetComponent<SpriteRenderer>();
    animator = GetComponent<Animator>();
  }

  private Transform GetColliderRoot(Collider2D other)
  {
    return other.attachedRigidbody != null ? other.attachedRigidbody.transform : other.transform;
  }

  private void FacePlayer(Transform player)
  {
    bool playerIsRight = player.position.x > transform.position.x;
    sr.flipX = spriteFacesRightByDefault ? !playerIsRight : playerIsRight;
  }

  private void OnDisable()
  {
    StopCoroutineSafe(ref lifetimeCoroutine);
    StopCoroutineSafe(ref releaseCoroutine);
  }
}
