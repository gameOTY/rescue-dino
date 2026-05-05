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

    [Header("RectTransform References")]
    [SerializeField] private RectTransform rescuedRect;
    [SerializeField] private RectTransform diedRect;
    [SerializeField] private RectTransform timerRect;
    [SerializeField] private RectTransform playerHealthRect;
    [SerializeField] private RectTransform winPanelRect;
    [SerializeField] private RectTransform losePanelRect;

    // (anchorMin, anchorMax) per element: rescued, died, timer, health
    private static readonly (Vector2 min, Vector2 max)[] LandscapeAnchors = new[]
    {
        (new Vector2(0.02f, 0.95f), new Vector2(0.25f, 0.98f)), // rescued
        (new Vector2(0.02f, 0.88f), new Vector2(0.25f, 0.91f)), // died
        (new Vector2(0.38f, 0.95f), new Vector2(0.62f, 0.98f)), // timer
        (new Vector2(0.38f, 0.88f), new Vector2(0.62f, 0.91f)), // health
    };

    private static readonly (Vector2 min, Vector2 max)[] PortraitAnchors = new[]
    {
        (new Vector2(0.02f, 0.95f), new Vector2(0.25f, 0.98f)), // rescued
        (new Vector2(0.02f, 0.88f), new Vector2(0.25f, 0.91f)), // died
        (new Vector2(0.28f, 0.95f), new Vector2(0.72f, 0.98f)), // timer
        (new Vector2(0.28f, 0.88f), new Vector2(0.72f, 0.91f)), // health
    };

    private float _lastAspect = -1;

    private void Start()
    {
        ApplyOrientationAnchors();
    }

    private void Update()
    {
        float aspect = Screen.width * 1f / Screen.height;
        if (Mathf.Abs(aspect - _lastAspect) > 0.01f)
        {
            _lastAspect = aspect;
            ApplyOrientationAnchors();
        }

        rescuedText.text = "Rescued: " + gameManager.SoldiersRescued;
        diedText.text = "Died: " + gameManager.SoldiersDied;
        timerText.text = "Time: " + Mathf.CeilToInt(gameManager.TimeRemaining) + "s";
        playerHealthText.text = "Health: " + gameManager.PlayerHealth;

        if (winPanel != null)
            winPanel.SetActive(gameManager.IsWon);

        if (losePanel != null)
            losePanel.SetActive(gameManager.IsGameOver);
    }

    private void ApplyOrientationAnchors()
    {
        bool isLandscape = Screen.width * 1f / Screen.height > 1f;
        var anchors = isLandscape ? LandscapeAnchors : PortraitAnchors;

        ApplyRectAnchors(rescuedRect, anchors[0]);
        ApplyRectAnchors(diedRect, anchors[1]);
        ApplyRectAnchors(timerRect, anchors[2]);
        ApplyRectAnchors(playerHealthRect, anchors[3]);

        // Panels always stretch to parent
        StretchToParent(winPanelRect);
        StretchToParent(losePanelRect);
    }

    private static void ApplyRectAnchors(RectTransform rect, (Vector2 min, Vector2 max) anchors)
    {
        if (rect == null) return;
        rect.anchorMin = anchors.min;
        rect.anchorMax = anchors.max;
    }

    private static void StretchToParent(RectTransform rect)
    {
        if (rect == null) return;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
