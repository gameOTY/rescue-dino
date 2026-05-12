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
          _elapsedTime += Time.deltaTime;
          _currentProgress = Mathf.Clamp01(_elapsedTime / _rescueDuration);
          _progressRingImage.fillAmount = _currentProgress;
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
  }
}
