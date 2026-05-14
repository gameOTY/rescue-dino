using UnityEngine;
using UnityEngine.UI;

public class TargetLifetimeIndicator : MonoBehaviour
{
  [SerializeField] private Image _lifetimeImage;

  [Header("Warning Thresholds")]
  [SerializeField] private float _warningThreshold1 = 3f;
  [SerializeField] private float _warningThreshold2 = 2f;
  [SerializeField] private float _warningThreshold3 = 1f;

  private float _lifetime;
  private float _remainingTime;
  private float _flashTimer;
  private Color _originalColor;
  private Vector3 _originalScale;

  public bool IsExpired => _remainingTime <= 0f;
  public bool IsStopped { get; private set; }

  private void Awake()
  {
    if (_lifetimeImage != null)
    {
      _originalColor = _lifetimeImage.color;
      _originalScale = _lifetimeImage.transform.localScale;
    }
  }

  public void Initialize(float lifetime)
  {
    _lifetime = lifetime;
    _remainingTime = lifetime;
    IsStopped = false;
    _flashTimer = 0f;

    if (_lifetimeImage != null)
    {
      _lifetimeImage.color = _originalColor;
      _lifetimeImage.transform.localScale = _originalScale;
      _lifetimeImage.fillAmount = 1f;
      gameObject.SetActive(true);
      Debug.Log($"[TargetLifetimeIndicator] Initialized with lifetime: {_lifetime:F2}s");
    }
    else
    {
      Debug.LogWarning("[TargetLifetimeIndicator] Lifetime Image is not assigned.");
    }
  }

  private void Update()
  {
    if (IsStopped || _lifetimeImage == null || IsGamePaused())
    {
      return;
    }

    _remainingTime -= Time.deltaTime;

    if (_remainingTime <= _warningThreshold3)
    {
      FlashWarningCritical();
    }
    else if (_remainingTime <= _warningThreshold2)
    {
      FlashWarningSevere();
    }
    else if (_remainingTime <= _warningThreshold1)
    {
      FlashWarning();
    }

    float fill = Mathf.Clamp01(_remainingTime / _lifetime);
    _lifetimeImage.fillAmount = fill;
  }

  private void FlashWarning()
  {
    _flashTimer += Time.deltaTime;
    if (_flashTimer >= 0.25f)
    {
      _flashTimer = 0f;

      bool isWhite = _lifetimeImage.color == Color.white;
      _lifetimeImage.color = isWhite ? _originalColor : Color.white;

      Vector3 scale = isWhite ? _originalScale : _originalScale * 1.2f;
      _lifetimeImage.transform.localScale = scale;
    }
  }

  private void FlashWarningSevere()
  {
    _flashTimer += Time.deltaTime;
    if (_flashTimer >= 0.15f)
    {
      _flashTimer = 0f;

      Color orange = new Color(1f, 0.5f, 0f, 1f);
      bool isOrange = _lifetimeImage.color == orange;
      _lifetimeImage.color = isOrange ? _originalColor : orange;

      Vector3 scale = isOrange ? _originalScale : _originalScale * 1.3f;
      _lifetimeImage.transform.localScale = scale;
    }
  }

  private void FlashWarningCritical()
  {
    _flashTimer += Time.deltaTime;
    if (_flashTimer >= 0.08f)
    {
      _flashTimer = 0f;

      Color red = new Color(1f, 0f, 0f, 1f);
      bool isRed = _lifetimeImage.color == red;
      _lifetimeImage.color = isRed ? _originalColor : red;

      Vector3 scale = isRed ? _originalScale : _originalScale * 1.4f;
      _lifetimeImage.transform.localScale = scale;
    }
  }

  public void StopCountdown()
  {
    IsStopped = true;
  }

  public void Hide()
  {
    StopCountdown();

    if (_lifetimeImage != null)
      _lifetimeImage.gameObject.SetActive(false);
  }

  private bool IsGamePaused()
  {
    // Guard: GameManager singleton initialized by SceneBootstrap before any
    // indicators activate. Null-check here handles edge case during scene transitions.
    return GameManager.Instance != null && GameManager.Instance.IsPaused;
  }
}
