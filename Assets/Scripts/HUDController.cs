using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
  [SerializeField] private GameManager gameManager;
  [SerializeField] private HUDConfig hudConfig;
  [SerializeField] private TMP_Text rescuedText;
  [SerializeField] private TMP_Text diedText;
  [SerializeField] private TMP_Text timerText;
  [SerializeField] private TMP_Text playerHealthText;

  [Header("End Screens")]
  [SerializeField] private GameObject endPanel;
  [SerializeField] private TMP_Text endTitleText;
  [SerializeField] private TMP_Text endCurrentScoreText;
  [SerializeField] private TMP_Text endHighScoreText;
  [SerializeField] private Button newGameButton;

  [Header("RectTransform References")]
  [SerializeField] private RectTransform rescuedRect;
  [SerializeField] private RectTransform diedRect;
  [SerializeField] private RectTransform timerRect;
  [SerializeField] private RectTransform playerHealthRect;
  [SerializeField] private RectTransform endPanelRect;

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

  private Camera _mainCamera;
  private float _pulsePhase;

  public HUDConfig Config => hudConfig;

  private void Start()
  {
    if (Config == null)
    {
      Debug.LogError("[HUDController] HUDConfig was not assigned in the Inspector.");
      enabled = false;
      return;
    }

    _mainCamera = Camera.main;
    if (edgeIndicatorImage != null)
      edgeIndicatorImage.enabled = false;

    RegisterNewGameButton(newGameButton);
  }

  private void Update()
  {
    bool isEndScreenVisible = gameManager.IsTerminal;
    if (endPanel != null)
      endPanel.SetActive(isEndScreenVisible);

    if (isEndScreenVisible)
      UpdateEndScreen();

    rescuedText.text = "Rescued: " + gameManager.SoldiersRescued;
    diedText.text = "Died: " + gameManager.SoldiersDied;

    if (healthBarDisplay != null)
      healthBarDisplay.UpdateDisplay((int)gameManager.PlayerHealth, (int)gameManager.MaxHealth);

    if (timerBarFill != null)
      timerBarFill.fillAmount = Mathf.Clamp01(gameManager.NormalizedTime);

    UpdateEdgeIndicator();
  }

  public void OnNewGamePressed()
  {
    if (gameManager != null)
    {
      gameManager.StartNewGame();
      return;
    }

    Scene activeScene = SceneManager.GetActiveScene();
    if (activeScene.buildIndex >= 0)
    {
      SceneManager.LoadScene(activeScene.buildIndex);
      return;
    }

    SceneManager.LoadScene(activeScene.name);
  }

  private void RegisterNewGameButton(Button button)
  {
    if (button == null) return;

    button.onClick.RemoveListener(OnNewGamePressed);
    if (button.onClick.GetPersistentEventCount() == 0)
      button.onClick.AddListener(OnNewGamePressed);
  }

  private void UpdateEndScreen()
  {
    if (endTitleText != null)
      endTitleText.text = gameManager.IsWon ? "You Win!" : "Game Over";

    if (endCurrentScoreText != null)
      endCurrentScoreText.text = "Score: " + gameManager.CurrentScore;

    if (endHighScoreText != null)
      endHighScoreText.text = "High Score: " + gameManager.HighScore;
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

    bool isOnScreen = soldierScreenPos.x >= Config.EdgePadding &&
                      soldierScreenPos.x <= Screen.width - Config.EdgePadding &&
                      soldierScreenPos.y >= Config.EdgePadding &&
                      soldierScreenPos.y <= Screen.height - Config.EdgePadding &&
                      soldierScreenPos.z > 0;

    if (isOnScreen)
    {
      edgeIndicatorImage.enabled = false;
      return;
    }

    // Clamp to screen edge in native pixel coords
    float clampedX = Mathf.Clamp(soldierScreenPos.x, Config.EdgePadding, Screen.width - Config.EdgePadding);
    float clampedY = Mathf.Clamp(soldierScreenPos.y, Config.EdgePadding, Screen.height - Config.EdgePadding);

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
    _pulsePhase += Time.deltaTime * Config.PulseSpeed;
    float t = (Mathf.Sin(_pulsePhase) + 1f) * 0.5f;
    float alpha = Mathf.Lerp(Config.PulseMinAlpha, Config.PulseMaxAlpha, t);
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
