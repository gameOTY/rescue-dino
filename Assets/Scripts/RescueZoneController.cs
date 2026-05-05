using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class RescueZoneController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float interactionScale = 1.2f;
    [SerializeField] private string playerTag = "Player";

    [Header("Visual")]
    [SerializeField] private SpriteRenderer zoneVisual;
    [SerializeField] private Color zoneColor = new Color(0f, 1f, 1f, 0.35f);
    [SerializeField] private bool showOnlyWhenPlayerInside = false;

    private CircleCollider2D triggerCollider;
    private SoliderController parentSoldier;

    private Coroutine rescueCoroutine;
    private int currentTransitionId;
    private int playerOverlapCount;
    private bool isRescuing;

    private void Awake()
    {
        triggerCollider = GetComponent<CircleCollider2D>();
        parentSoldier = GetComponentInParent<SoliderController>();

        if (parentSoldier == null)
        {
            Debug.LogError("[RescueZoneController] No SoliderController found in parent.", this);
            enabled = false;
            return;
        }

        triggerCollider.isTrigger = true;

        ConfigureColliderSize();
        ConfigureVisual();
        UpdateVisualState();
    }

    private void ConfigureColliderSize()
    {
        SpriteRenderer soldierSprite = parentSoldier.GetComponent<SpriteRenderer>();

        if (soldierSprite != null && soldierSprite.sprite != null)
        {
            Vector2 spriteSize = soldierSprite.sprite.bounds.size;
            float baseRadius = Mathf.Max(spriteSize.x, spriteSize.y) * 0.5f;

            triggerCollider.radius = baseRadius * interactionScale;
            triggerCollider.offset = Vector2.zero;
        }
        else
        {
            triggerCollider.radius = interactionScale;
            triggerCollider.offset = Vector2.zero;
        }
    }

    private void ConfigureVisual()
    {
        if (zoneVisual == null)
            return;

        zoneVisual.color = zoneColor;

        float diameter = triggerCollider.radius * 2f;

        // Giả sử sprite visual có size gốc là 1x1 Unity unit.
        // Nếu sprite của bạn không phải 1x1, xem version bên dưới.
        zoneVisual.transform.localPosition = triggerCollider.offset;
        zoneVisual.transform.localScale = new Vector3(diameter, diameter, 1f);

        SpriteRenderer soldierSprite = parentSoldier.GetComponent<SpriteRenderer>();
        if (soldierSprite != null)
        {
            zoneVisual.sortingLayerID = soldierSprite.sortingLayerID;
            zoneVisual.sortingOrder = soldierSprite.sortingOrder + 1;
        }
    }

    private void UpdateVisualState()
    {
        if (zoneVisual == null)
            return;

        zoneVisual.enabled = !showOnlyWhenPlayerInside || playerOverlapCount > 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerOverlapCount++;
        UpdateVisualState();

        if (isRescuing)
            return;

        StartRescue();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerOverlapCount = Mathf.Max(0, playerOverlapCount - 1);
        UpdateVisualState();

        if (playerOverlapCount == 0)
        {
            CancelRescue();
        }
    }

    private void StartRescue()
    {
        isRescuing = true;
        currentTransitionId++;
        rescueCoroutine = StartCoroutine(RescueCountdown(currentTransitionId));
    }

    private IEnumerator RescueCountdown(int transitionId)
    {
        float elapsed = 0f;
        float rescueDuration = parentSoldier.RescueDuration;

        while (elapsed < rescueDuration)
        {
            if (transitionId != currentTransitionId)
                yield break;

            if (playerOverlapCount <= 0)
                yield break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (transitionId != currentTransitionId)
            yield break;

        if (playerOverlapCount <= 0)
            yield break;

        parentSoldier.HandleAuthoritativeRescue();

        isRescuing = false;
        rescueCoroutine = null;
    }

    private void CancelRescue()
    {
        currentTransitionId++;

        if (rescueCoroutine != null)
        {
            StopCoroutine(rescueCoroutine);
            rescueCoroutine = null;
        }

        isRescuing = false;
    }

    private void OnDisable()
    {
        CancelRescue();
        playerOverlapCount = 0;
    }
}
