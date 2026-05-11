using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class RescueZoneController : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float rescueRadiusMultiplier = 1.2f;
    [SerializeField] private string playerTag = "Player";

    [Header("Visual")]
    [SerializeField] private SpriteRenderer zoneRenderer;
    [SerializeField] private Color rescueZoneColor = new Color(0f, 1f, 1f, 0.35f);
    [SerializeField] private bool showZoneOnlyWhenPlayerInside = false;

    private CircleCollider2D rescueTrigger;
    private SoldierController soldier;

    private Coroutine rescueRoutine;
    private int rescueSessionVersion;
    private int playerColliderCountInside;
    private bool isRescueCounting;
    private float elapsedRescueSeconds;

    public event Action<RescueZoneController> RescueCompleted;
    public event Action PlayerEntered;
    public event Action PlayerExited;

    public bool IsPaused => elapsedRescueSeconds > 0f && !isRescueCounting;
    public float ElapsedRescueTime => elapsedRescueSeconds;
    public float RescueDuration => soldier != null ? soldier.RescueDuration : 3f;

    public float GetRescueDuration()
    {
        return RescueDuration;
    }

    private void Awake()
    {
        CacheComponents();

        if (soldier == null)
        {
            Debug.LogError("[RescueZoneController] No SoldierController found in parent.", this);
            enabled = false;
            return;
        }

        SetupTrigger();
        SetupZoneVisual();
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

        if (elapsedRescueSeconds > 0f)
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

        rescueRoutine = StartCoroutine(CountRescueProgress(rescueSessionVersion, 0f));
    }

    private void ResumeRescue()
    {
        isRescueCounting = true;
        rescueSessionVersion++;

        rescueRoutine = StartCoroutine(
            CountRescueProgress(rescueSessionVersion, elapsedRescueSeconds)
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
            elapsedRescueSeconds = currentElapsedSeconds;

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
        RescueCompleted?.Invoke(this);

        soldier.HandleAuthoritativeRescue();

        isRescueCounting = false;
        rescueRoutine = null;
        elapsedRescueSeconds = 0f;
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

        zoneRenderer.color = rescueZoneColor;
        zoneRenderer.transform.localPosition = rescueTrigger.offset;

        MatchZoneVisualToTriggerSize();
        MatchZoneVisualSortingToSoldier();
    }

    private void MatchZoneVisualToTriggerSize()
    {
        if (zoneRenderer.sprite == null)
            return;

        float targetDiameter = 1f * rescueRadiusMultiplier;
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
        if (zoneRenderer == null)
            return;

        zoneRenderer.enabled = !showZoneOnlyWhenPlayerInside || HasPlayerInside();
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
