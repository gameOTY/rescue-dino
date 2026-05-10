using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
  [SerializeField] private GameManager gameManager;
  [SerializeField] private TMP_Text rescuedText;
  [SerializeField] private TMP_Text diedText;
  [SerializeField] private TMP_Text timerText;
  [SerializeField] private TMP_Text playerHealthText;

  [Header("End Screens")]
  [SerializeField] private GameObject winPanel;
  [SerializeField] private GameObject losePanel;

  [Header("RectTransform References")]
  [SerializeField] private RectTransform rescuedRect;
  [SerializeField] private RectTransform diedRect;
  [SerializeField] private RectTransform timerRect;
  [SerializeField] private RectTransform playerHealthRect;
  [SerializeField] private RectTransform winPanelRect;
  [SerializeField] private RectTransform losePanelRect;

  [Header("Health Bar")]
  [SerializeField] private ResourceDisplay healthBarDisplay;

  [Header("Timer Bar")]
  [SerializeField] private Image timerBarFill;
  [SerializeField] private Image timerBarIcon;
  [SerializeField] private RectTransform timerBarContainerRect;

  [Header("Edge Indicator")]
  [SerializeField] private Image edgeIndicatorImage;
  [SerializeField] private SoldierTracker soldierTracker;
  [SerializeField] private Canvas hudCanvas;
  [SerializeField] private float edgePadding = 50f;
  [SerializeField] private float pulseSpeed = 2f;
  [SerializeField] private float pulseMinAlpha = 0.4f;
  [SerializeField] private float pulseMaxAlpha = 1f;

  private Camera _mainCamera;
  private float _pulsePhase;

  private void Start()
  {
    _mainCamera = Camera.main;
    if (edgeIndicatorImage != null)
      edgeIndicatorImage.enabled = false;
  }

  private void Update()
  {
    if (winPanel != null)
      winPanel.SetActive(gameManager.IsWon);

    if (losePanel != null)
      losePanel.SetActive(gameManager.IsGameOver);

    rescuedText.text = "Rescued: " + gameManager.SoldiersRescued;
    diedText.text = "Died: " + gameManager.SoldiersDied;

    if (healthBarDisplay != null)
      healthBarDisplay.UpdateDisplay((int)gameManager.PlayerHealth, (int)gameManager.MaxHealth);

    if (timerBarFill != null)
      timerBarFill.fillAmount = Mathf.Clamp01(gameManager.NormalizedTime);

    UpdateEdgeIndicator();
  }

  private void UpdateEdgeIndicator()
  {
    if (edgeIndicatorImage == null || soldierTracker == null || _mainCamera == null || hudCanvas == null)
      return;

    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (player == null) return;

    if (!soldierTracker.TryGetNearestSoldier(player.transform, out Vector3 soldierWorldPos))
    {
      edgeIndicatorImage.enabled = false;
      return;
    }

    Vector3 soldierScreenPos = _mainCamera.WorldToScreenPoint(soldierWorldPos);

    bool isOnScreen = soldierScreenPos.x >= edgePadding &&
                      soldierScreenPos.x <= Screen.width - edgePadding &&
                      soldierScreenPos.y >= edgePadding &&
                      soldierScreenPos.y <= Screen.height - edgePadding &&
                      soldierScreenPos.z > 0;

    if (isOnScreen)
    {
      edgeIndicatorImage.enabled = false;
      return;
    }

    // Clamp to screen edge in native pixel coords
    float clampedX = Mathf.Clamp(soldierScreenPos.x, edgePadding, Screen.width  - edgePadding);
    float clampedY = Mathf.Clamp(soldierScreenPos.y, edgePadding, Screen.height - edgePadding);

    Camera uiCamera = hudCanvas.renderMode == RenderMode.ScreenSpaceOverlay
        ? null
        : hudCanvas.worldCamera;

    bool converted = RectTransformUtility.ScreenPointToLocalPointInRectangle(
        hudCanvas.GetComponent<RectTransform>(),
        new Vector2(clampedX, clampedY),
        uiCamera,
        out Vector2 canvasLocalPoint
    );

    RectTransform indicatorRect = edgeIndicatorImage.rectTransform;

    if (!converted)
    {
      edgeIndicatorImage.enabled = false;
      return;
    }

    indicatorRect.localPosition = canvasLocalPoint;

    // Direction from clamped screen-edge position toward actual soldier (both in canvas scaled space)
    Vector2 dir = new Vector2(soldierScreenPos.x - clampedX, soldierScreenPos.y - clampedY).normalized;
    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

    indicatorRect.rotation = Quaternion.Euler(0, 0, angle);

    // Pulse alpha
    _pulsePhase += Time.deltaTime * pulseSpeed;
    float t = (Mathf.Sin(_pulsePhase) + 1f) * 0.5f;
    float alpha = Mathf.Lerp(pulseMinAlpha, pulseMaxAlpha, t);
    SetIndicatorAlpha(alpha);

    edgeIndicatorImage.enabled = true;
  }

  private void SetIndicatorAlpha(float alpha)
  {
    Color c = edgeIndicatorImage.color;
    c.a = alpha;
    edgeIndicatorImage.color = c;
  }
}
