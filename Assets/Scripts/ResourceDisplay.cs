using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ResourceDisplay : MonoBehaviour
{
  [Header("Icon Display")]
  [SerializeField] private Image[] iconImages; // Array of Image components for the icons
  [SerializeField] private Sprite filledSprite;
  [SerializeField] private Sprite emptySprite;

  [Header("Text Display")]
  [SerializeField] private TextMeshProUGUI countText;
  [SerializeField] private string countFormat = "{0}/{1}";

  public void UpdateDisplay(int currentCount, int maxCount)
  {
    UpdateIcons(currentCount, maxCount);
    UpdateText(currentCount, maxCount);
  }

  private void UpdateIcons(int currentCount, int maxCount)
  {
    for (int i = 0; i < iconImages.Length; i++)
    {
      if (iconImages[i] == null)
        continue;

      if (i >= maxCount)
      {
        iconImages[i].enabled = false; // Hide icons beyond max count
        continue;
      }

      iconImages[i].enabled = true; // Ensure icons within max count are visible

      bool isFilled = i < currentCount;

      iconImages[i].sprite = isFilled ? filledSprite : emptySprite;
    }
  }

  private void UpdateText(int current, int max)
  {
    if (countText == null) return;
    countText.text = string.Format(countFormat, current, max);
  }
}
