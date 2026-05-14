using UnityEngine;

[CreateAssetMenu(fileName = "HUDConfig", menuName = "Settings/HUDConfig")]
public class HUDConfig : ScriptableObject
{
  [Header("Edge Indicator")]
  [SerializeField] private float edgePadding = 50f;
  [SerializeField] private float pulseSpeed = 2f;
  [SerializeField] private float pulseMinAlpha = 0.4f;
  [SerializeField] private float pulseMaxAlpha = 1f;
  [SerializeField] private int maxTrackedSoldiers = 1;

  public float EdgePadding => edgePadding;
  public float PulseSpeed => pulseSpeed;
  public float PulseMinAlpha => pulseMinAlpha;
  public float PulseMaxAlpha => pulseMaxAlpha;
  public int MaxTrackedSoldiers => maxTrackedSoldiers;
}
