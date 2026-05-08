using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
  [SerializeField] private GameManager gameManager;
  [SerializeField] private TMP_Text rescuedText;
  [SerializeField] private TMP_Text diedText;
  [SerializeField] private TMP_Text timerText;
  [SerializeField] private TMP_Text playerHealthText;

  [Header("End Screens")]
  [SerializeField] private GameObject winPanel;
  [SerializeField] private GameObject losePanel;

  [Header("RectTransform References")]
  [SerializeField] private RectTransform rescuedRect;
  [SerializeField] private RectTransform diedRect;
  [SerializeField] private RectTransform timerRect;
  [SerializeField] private RectTransform playerHealthRect;
  [SerializeField] private RectTransform winPanelRect;
  [SerializeField] private RectTransform losePanelRect;

  [Header("Health Bar")]
  [SerializeField] private ResourceDisplay healthBarDisplay;

  [Header("Timer Bar")]
  [SerializeField] private Image timerBarFill;
  [SerializeField] private Image timerBarIcon;
  [SerializeField] private RectTransform timerBarContainerRect;

  private void Update()
  {
    if (winPanel != null)
      winPanel.SetActive(gameManager.IsWon);

    if (losePanel != null)
      losePanel.SetActive(gameManager.IsGameOver);

    rescuedText.text = "Rescued: " + gameManager.SoldiersRescued;
    diedText.text = "Died: " + gameManager.SoldiersDied;


    if (healthBarDisplay != null)
      healthBarDisplay.UpdateDisplay((int)gameManager.PlayerHealth, (int)gameManager.MaxHealth);

    if (timerBarFill != null)
      timerBarFill.fillAmount = Mathf.Clamp01(gameManager.NormalizedTime);

  }
}
