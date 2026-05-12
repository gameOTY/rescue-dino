using UnityEngine;
using UnityEngine.UI;

public class TargetLifetimeIndicator : MonoBehaviour
{
  [SerializeField] private Image _lifetimeImage;

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
    if (IsStopped || _lifetimeImage == null)
    {
      return;
    }

    _remainingTime -= Time.deltaTime;

    if (_remainingTime <= 2f)
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

  public void StopCountdown()
  {
    IsStopped = true;
  }
}
