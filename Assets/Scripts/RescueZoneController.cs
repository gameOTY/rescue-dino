using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class RescueZoneController : MonoBehaviour
{
  [Header("Detection")]
  [SerializeField] private string playerTag = "Player";

  [Header("Visual")]
  [SerializeField] private SpriteRenderer zoneRenderer;
  [SerializeField] private GameObject healingAura;

  [Header("Config")]
  [SerializeField] private GameConfig gameConfig;

  public GameConfig Config => gameConfig;

  private CircleCollider2D rescueTrigger;
  private SoldierController soldier;

  private Coroutine rescueRoutine;
  private int rescueSessionVersion;
  private int playerColliderCountInside;
  private bool isRescueCounting;

  public event Action<RescueZoneController> RescueCompleted;
  public event Action PlayerEntered;
  public event Action PlayerExited;

  public bool IsPaused => ElapsedRescueTime > 0f && !isRescueCounting;
  public float ElapsedRescueTime { get; private set; }
  public float RescueDuration => soldier != null ? soldier.RescueDuration : 3f;

  public float GetRescueDuration()
  {
    return RescueDuration;
  }

  private void Awake()
  {
    CacheComponents();

    if (Config == null)
    {
      Debug.LogError("[RescueZoneController] GameConfig was not assigned in the Inspector.", this);
      enabled = false;
      return;
    }

    if (soldier == null)
    {
      Debug.LogError("[RescueZoneController] No SoldierController found in parent.", this);
      enabled = false;
      return;
    }

    SetupTrigger();
    SetupZoneVisual();
    SetHealingAuraActive(false);
    RefreshZoneVisualVisibility();
  }

  private void OnDisable()
  {
    PauseRescue();

    playerColliderCountInside = 0;
    RefreshZoneVisualVisibility();
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (!IsPlayerCollider(other))
      return;

    playerColliderCountInside++;

    RefreshZoneVisualVisibility();
    PlayerEntered?.Invoke();

    if (isRescueCounting)
      return;

    if (ElapsedRescueTime > 0f)
    {
      ResumeRescue();
    }
    else
    {
      StartRescue();
    }
  }

  private void OnTriggerExit2D(Collider2D other)
  {
    if (!IsPlayerCollider(other))
      return;

    playerColliderCountInside = Mathf.Max(0, playerColliderCountInside - 1);

    RefreshZoneVisualVisibility();

    if (playerColliderCountInside > 0)
      return;

    PauseRescue();
    PlayerExited?.Invoke();
  }

  private void StartRescue()
  {
    isRescueCounting = true;
    rescueSessionVersion++;
    SetHealingAuraActive(true);

    rescueRoutine = StartCoroutine(CountRescueProgress(rescueSessionVersion, 0f));
  }

  private void ResumeRescue()
  {
    isRescueCounting = true;
    rescueSessionVersion++;
    SetHealingAuraActive(true);

    rescueRoutine = StartCoroutine(
      CountRescueProgress(rescueSessionVersion, ElapsedRescueTime)
    );
  }

  private void PauseRescue()
  {
    rescueSessionVersion++;

    if (rescueRoutine != null)
    {
      StopCoroutine(rescueRoutine);
      rescueRoutine = null;
    }

    isRescueCounting = false;
    SetHealingAuraActive(false);
  }

  private IEnumerator CountRescueProgress(int sessionVersion, float startElapsedSeconds)
  {
    float currentElapsedSeconds = startElapsedSeconds;

    while (currentElapsedSeconds < RescueDuration)
    {
      if (!IsCurrentRescueSession(sessionVersion))
        yield break;

      if (!HasPlayerInside())
        yield break;

      currentElapsedSeconds += Time.deltaTime;
      ElapsedRescueTime = currentElapsedSeconds;

      yield return null;
    }

    if (!IsCurrentRescueSession(sessionVersion))
      yield break;

    if (!HasPlayerInside())
      yield break;

    CompleteRescue();
  }

  private void CompleteRescue()
  {
    SetHealingAuraActive(false);
    RescueCompleted?.Invoke(this);

    soldier.HandleAuthoritativeRescue();

    isRescueCounting = false;
    rescueRoutine = null;
    ElapsedRescueTime = 0f;
  }

  private void CacheComponents()
  {
    rescueTrigger = GetComponent<CircleCollider2D>();
    soldier = GetComponentInParent<SoldierController>();
  }

  private void SetupTrigger()
  {
    rescueTrigger.isTrigger = true;
    rescueTrigger.offset = Vector2.zero;
    rescueTrigger.radius = CalculateRescueRadius();
  }

  private float CalculateRescueRadius()
  {
    return 0.5f;
  }

  private void SetupZoneVisual()
  {
    if (zoneRenderer == null)
      return;

    zoneRenderer.color = Config.RescueZoneColor;
    zoneRenderer.transform.localPosition = rescueTrigger.offset;

    MatchZoneVisualToTriggerSize();
    MatchZoneVisualSortingToSoldier();
  }

  private void MatchZoneVisualToTriggerSize()
  {
    if (zoneRenderer.sprite == null)
      return;

    float targetDiameter = Config.RescueZoneDiameter;
    Vector2 spriteSize = zoneRenderer.sprite.bounds.size;

    zoneRenderer.transform.localScale = new Vector3(
      targetDiameter / spriteSize.x,
      targetDiameter / spriteSize.y,
      1f
    );
  }

  private void MatchZoneVisualSortingToSoldier()
  {
    SpriteRenderer soldierRenderer = soldier.GetComponent<SpriteRenderer>();

    if (soldierRenderer == null)
      return;

    zoneRenderer.sortingLayerID = soldierRenderer.sortingLayerID;
    zoneRenderer.sortingOrder = soldierRenderer.sortingOrder + 1;
  }

  private void RefreshZoneVisualVisibility()
  {
    if (zoneRenderer == null || Config == null)
      return;

    zoneRenderer.enabled = !Config.ShowZoneOnlyWhenPlayerInside || HasPlayerInside();
  }

  private void SetHealingAuraActive(bool isActive)
  {
    if (healingAura == null)
      return;

    healingAura.SetActive(isActive);
  }

  private bool IsPlayerCollider(Collider2D other)
  {
    return other.CompareTag(playerTag);
  }

  private bool HasPlayerInside()
  {
    return playerColliderCountInside > 0;
  }

  private bool IsCurrentRescueSession(int sessionVersion)
  {
    return sessionVersion == rescueSessionVersion;
  }
}
