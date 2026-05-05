using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DangerZoneController : MonoBehaviour
{
  [SerializeField] private float lifetime = 3f;
  [SerializeField] private float blinkInterval = 0.2f;

  private PrefabPool objectPool;
  private SpriteRenderer sr;
  private Coroutine lifetimeCoroutine;
  private Coroutine blinkCoroutine;

  public event Action<DangerZoneController> PlayerDamaged;

  public void SetPool(PrefabPool pool) => objectPool = pool;

  public void Initialize(float lifetimeSeconds)
  {
    this.lifetime = lifetimeSeconds;
    lifetimeCoroutine = StartCoroutine(LifetimeRoutine());
    blinkCoroutine = StartCoroutine(BlinkRoutine());
  }

  /// <summary>
  /// Called by DangerZoneAreaController child when player enters the trigger zone.
  /// </summary>
  public void OnPlayerEnteredZone()
  {
    DamageAndRelease();
  }

  private IEnumerator LifetimeRoutine()
  {
    yield return new WaitForSeconds(lifetime);
    Release();
  }

  private void DamageAndRelease()
  {
    StopCoroutineSafe(ref lifetimeCoroutine);
    StopCoroutineSafe(ref blinkCoroutine);
    PlayerDamaged?.Invoke(this);
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

  private void Awake()
  {
    sr = GetComponent<SpriteRenderer>();
  }

  private void OnDisable()
  {
    StopCoroutineSafe(ref lifetimeCoroutine);
    StopCoroutineSafe(ref blinkCoroutine);
  }

  private IEnumerator BlinkRoutine()
  {
    while (true)
    {
      Color c = sr.color;
      c.a = c.a > 0.5f ? 0.15f : 1f;
      sr.color = c;
      yield return new WaitForSeconds(blinkInterval);
    }
  }
}