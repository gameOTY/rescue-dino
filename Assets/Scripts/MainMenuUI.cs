using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;

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
        Screen.fullScreen = isOn;
        PlayerPrefs.SetInt("Fullscreen", isOn ? 1 : 0);
        PlayerPrefs.Save();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
}