using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Toggle fullscreenToggle;

    private void Start()
    {
        SettingsPreferences.ApplySavedFullscreen();

        if (fullscreenToggle == null && settingsPanel != null)
            fullscreenToggle = settingsPanel.GetComponentInChildren<Toggle>(true);

        if (fullscreenToggle != null)
            fullscreenToggle.SetIsOnWithoutNotify(SettingsPreferences.IsFullscreenEnabled());
    }

    public void OnStartPressed()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OnSettingsPressed()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void OnExitPressed()
    {
        Application.Quit();
    }

    public void OnFullscreenToggled(bool isOn)
    {
        SettingsPreferences.SetFullscreen(isOn);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
}
