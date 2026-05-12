using UnityEngine;

[CreateAssetMenu(fileName = "MapConfig", menuName = "Settings/MapConfig")]
public class MapConfig : ScriptableObject
{
  [Header("Map Dimensions")]
  [Range(5, 30)]
  [SerializeField] private int mapWidth = 20;
  [Range(5, 30)]
  [SerializeField] private int mapHeight = 20;

  public int MapWidth => mapWidth;
  public int MapHeight => mapHeight;
}
