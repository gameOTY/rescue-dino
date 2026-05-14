using UnityEngine;

public static class SettingsPreferences
{
  public const string FullscreenKey = "Fullscreen";

  public static bool IsFullscreenEnabled()
  {
    return PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
  }

  public static void ApplySavedFullscreen()
  {
    Screen.fullScreen = IsFullscreenEnabled();
  }

  public static void SetFullscreen(bool isOn)
  {
    Screen.fullScreen = isOn;
    PlayerPrefs.SetInt(FullscreenKey, isOn ? 1 : 0);
    PlayerPrefs.Save();
  }
}
