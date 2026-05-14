using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "MapTilePalette", menuName = "Settings/MapTilePalette")]
public class MapTilePalette : ScriptableObject
{
  [Header("Water")]
  [SerializeField] private TileBase waterBackgroundTile;
  [SerializeField] private TileBase waterFoamTile;

  [Header("Height")]
  [SerializeField] private TileBase shadowTile;

  [Header("Land")]
  [SerializeField] private TileBase flatGroundTile;
  [SerializeField] private TileBase invisibleColliderTile;
  [SerializeField] private TileBase cliffTile;

  [Header("Fallbacks")]
  [SerializeField] private TileBase fallbackFloorTile;
  [SerializeField] private TileBase fallbackWallTile;

  public TileBase WaterBackgroundTile => waterBackgroundTile;
  public TileBase WaterFoamTile => waterFoamTile;
  public TileBase ShadowTile => shadowTile;
  public TileBase FlatGroundTile => flatGroundTile != null ? flatGroundTile : fallbackFloorTile;
  public TileBase InvisibleColliderTile => invisibleColliderTile != null ? invisibleColliderTile : fallbackWallTile;
  public TileBase CliffTile => cliffTile != null ? cliffTile : fallbackWallTile;
}
