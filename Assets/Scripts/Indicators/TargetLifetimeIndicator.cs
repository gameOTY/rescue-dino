using UnityEngine;
using UnityEngine.UI;

public class TargetLifetimeIndicator : MonoBehaviour
{
    [SerializeField] private Image _lifetimeImage;
    [SerializeField] private float _lifetime = 10f;

    private float _remainingTime;
    private bool _isStopped;
    private bool _isVisible;
    private float _flashTimer;
    private Color _originalColor;
    private Vector3 _originalScale;

    public bool IsExpired => _remainingTime <= 0f;
    public bool IsStopped => _isStopped;

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
        _remainingTime = lifetime;
        _isStopped = false;
        _flashTimer = 0f;

        if (_lifetimeImage != null)
        {
            _lifetimeImage.color = _originalColor;
            _lifetimeImage.transform.localScale = _originalScale;
            _lifetimeImage.fillAmount = 1f;
            gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (_isStopped || !_isVisible || _lifetimeImage == null)
            return;

        _remainingTime -= Time.deltaTime;

        if (_remainingTime <= 2f)
        {
            FlashWarning();
        }

        Debug.Log($"[TargetLifetimeIndicator] Remaining Time: {_remainingTime:F2}s");

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
        _isStopped = true;
    }

    private void OnBecameVisible()
    {
        _isVisible = Camera.current != null && Camera.current.name == "Main Camera";
    }

    private void OnBecameInvisible()
    {
        _isVisible = false;
    }
}
