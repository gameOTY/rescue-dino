using UnityEngine;
using UnityEngine.UI;

namespace RescueGame
{
  public class RescueProgressIndicator : MonoBehaviour
  {
    [SerializeField] private Image _progressRingImage;

    private float _currentProgress; // 0.0 to 1.0
    private float _elapsedTime; // survives pause/resume
    private float _rescueDuration;
    private bool _isRunning;
    private Coroutine _progressCoroutine;
    private bool _isVisible;

    private void Awake()
    {
      _currentProgress = 0f;
      _elapsedTime = 0f;
      _isRunning = false;
      _isVisible = false;

      if (_progressRingImage != null)
      {
        _progressRingImage.fillAmount = 0f;
      }
    }

    private void OnDisable()
    {
      StopProgress();
    }

    public void Initialize(float rescueDuration)
    {
      _rescueDuration = rescueDuration;
      _elapsedTime = 0f;
      _currentProgress = 0f;
      _isRunning = false;
      if (_progressRingImage != null)
      {
        _progressRingImage.gameObject.SetActive(true);
        _progressRingImage.fillAmount = 0f;
        Color c = _progressRingImage.color;
        c.a = 1f;
        _progressRingImage.color = c;
      }
    }

    public void StartProgress(float rescueDuration)
    {
      _rescueDuration = rescueDuration;
      _elapsedTime = 0f;
      _currentProgress = 0f;
      _isRunning = true;

      if (_progressRingImage != null)
      {
        _progressRingImage.gameObject.SetActive(true);
        _progressRingImage.fillAmount = 0f;
      }

      RestartCoroutine();
    }

    public void PauseProgress()
    {
      _isRunning = false;
      StopCoroutineRef();

      if (_progressRingImage != null)
      {
        Color c = _progressRingImage.color;
        c.a = 0.4f;
        _progressRingImage.color = c;
      }
    }

    public void Hide()
    {
      StopProgress();

      if (_progressRingImage != null)
        _progressRingImage.gameObject.SetActive(false);
    }

    public void ResumeProgress(float startElapsedTime, float rescueDuration)
    {
      _elapsedTime = startElapsedTime;
      _rescueDuration = rescueDuration;
      _currentProgress = Mathf.Clamp01(startElapsedTime / rescueDuration);
      _isRunning = true;

      if (_progressRingImage != null)
      {
        _progressRingImage.gameObject.SetActive(true);
        _progressRingImage.fillAmount = _currentProgress;
        Color c = _progressRingImage.color;
        c.a = 1f;
        _progressRingImage.color = c;
      }

      RestartCoroutine();
    }

    public void SetProgress(float percentage)
    {
      _currentProgress = Mathf.Clamp01(percentage);

      if (_progressRingImage != null)
      {
        _progressRingImage.fillAmount = _currentProgress;
      }
    }

    private void RestartCoroutine()
    {
      StopCoroutineRef();
      _progressCoroutine = StartCoroutine(ProgressCountdown());
    }

    private void StopCoroutineRef()
    {
      if (_progressCoroutine != null)
      {
        StopCoroutine(_progressCoroutine);
        _progressCoroutine = null;
      }
    }

    private System.Collections.IEnumerator ProgressCountdown()
    {
      while (_elapsedTime < _rescueDuration)
      {
        if (_isRunning)
        {
          if (!IsGamePaused())
          {
            _elapsedTime += Time.deltaTime;
            _currentProgress = Mathf.Clamp01(_elapsedTime / _rescueDuration);
            _progressRingImage.fillAmount = _currentProgress;
          }
        }

        yield return null;
      }

      _currentProgress = 1f;
      if (_progressRingImage != null)
      {
        _progressRingImage.fillAmount = 1f;
      }
    }

    private void StopProgress()
    {
      _isRunning = false;
      StopCoroutineRef();
    }

    private bool IsGamePaused()
    {
      return GameManager.Instance != null && GameManager.Instance.IsPaused;
    }
  }
}
