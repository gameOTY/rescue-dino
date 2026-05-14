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

  [Header("Pause Menu")]
  [SerializeField] private Canvas pauseOverlayCanvas;
  [SerializeField] private Button pauseButton;
  [SerializeField] private GameObject pausePanel;
  [SerializeField] private Button resumeButton;
  [SerializeField] private Button pauseSettingsButton;
  [SerializeField] private Button pauseRestartButton;
  [SerializeField] private Button pauseMainMenuButton;

  [Header("In-Game Settings")]
  [SerializeField] private GameObject settingsPanel;
  [SerializeField] private Toggle fullscreenToggle;
  [SerializeField] private Button settingsBackButton;

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
  [SerializeField] private GameObject player;

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
    if (player == null) player = GameObject.FindGameObjectWithTag("Player");
    if (edgeIndicatorImage != null)
      edgeIndicatorImage.enabled = false;

    SettingsPreferences.ApplySavedFullscreen();
    RegisterNewGameButton(newGameButton);
    RegisterPauseButtons();
    UpdatePauseUi();
  }

  private void Update()
  {
    bool isEndScreenVisible = gameManager.IsTerminal;
    if (isEndScreenVisible && gameManager.IsPaused)
      gameManager.ResumeGame();

    if (endPanel != null)
      endPanel.SetActive(isEndScreenVisible);

    if (isEndScreenVisible)
      UpdateEndScreen();

    UpdatePauseUi();

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

  public void OnPausePressed()
  {
    if (gameManager != null && gameManager.PauseGame())
      UpdatePauseUi();
  }

  public void OnResumePressed()
  {
    if (gameManager != null)
      gameManager.ResumeGame();

    UpdatePauseUi();
  }

  public void OnPauseSettingsPressed()
  {
    if (gameManager == null || !gameManager.IsPaused || gameManager.IsTerminal)
      return;

    if (fullscreenToggle != null)
      fullscreenToggle.SetIsOnWithoutNotify(SettingsPreferences.IsFullscreenEnabled());

    if (pausePanel != null)
      pausePanel.SetActive(false);

    if (settingsPanel != null)
      settingsPanel.SetActive(true);
  }

  public void OnSettingsBackPressed()
  {
    if (settingsPanel != null)
      settingsPanel.SetActive(false);

    UpdatePauseUi();
  }

  public void OnFullscreenToggled(bool isOn)
  {
    SettingsPreferences.SetFullscreen(isOn);
  }

  public void OnMainMenuPressed()
  {
    if (gameManager != null)
      gameManager.ResumeGame();

    SceneManager.LoadScene("MainMenu");
  }

  private void RegisterNewGameButton(Button button)
  {
    if (button == null) return;

    button.onClick.RemoveListener(OnNewGamePressed);
    if (button.onClick.GetPersistentEventCount() == 0)
      button.onClick.AddListener(OnNewGamePressed);
  }

  private void RegisterPauseButtons()
  {
    RegisterButton(pauseButton, OnPausePressed);
    RegisterButton(resumeButton, OnResumePressed);
    RegisterButton(pauseSettingsButton, OnPauseSettingsPressed);
    RegisterButton(pauseRestartButton, OnNewGamePressed);
    RegisterButton(pauseMainMenuButton, OnMainMenuPressed);
    RegisterButton(settingsBackButton, OnSettingsBackPressed);

    if (fullscreenToggle != null)
    {
      fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenToggled);
      fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
      fullscreenToggle.SetIsOnWithoutNotify(SettingsPreferences.IsFullscreenEnabled());
    }
  }

  private void RegisterButton(Button button, UnityEngine.Events.UnityAction action)
  {
    if (button == null) return;

    button.onClick.RemoveListener(action);
    if (button.onClick.GetPersistentEventCount() == 0)
      button.onClick.AddListener(action);
  }

  private void UpdatePauseUi()
  {
    if (gameManager == null) return;

    bool canPause = !gameManager.IsTerminal;
    if (pauseOverlayCanvas != null)
      pauseOverlayCanvas.gameObject.SetActive(canPause || gameManager.IsPaused);

    if (pauseButton != null)
      pauseButton.gameObject.SetActive(canPause && !gameManager.IsPaused);

    bool showingSettings = settingsPanel != null && settingsPanel.activeSelf;
    if (pausePanel != null)
      pausePanel.SetActive(canPause && gameManager.IsPaused && !showingSettings);

    if (!canPause && settingsPanel != null)
      settingsPanel.SetActive(false);
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
