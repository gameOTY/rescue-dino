using TMPro;
using UnityEngine;

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

  private void Update()
  {
    rescuedText.text = "Rescued: " + gameManager.SoldiersRescued;
    diedText.text = "Died: " + gameManager.SoldiersDied;
    timerText.text = "Time: " + Mathf.CeilToInt(gameManager.TimeRemaining) + "s";
    playerHealthText.text = "Health: " + gameManager.PlayerHealth;

    if (winPanel != null)
    {
      winPanel.SetActive(gameManager.IsWon);
    }

    if (losePanel != null)
    {
      losePanel.SetActive(gameManager.IsGameOver);
    }
  }
}
