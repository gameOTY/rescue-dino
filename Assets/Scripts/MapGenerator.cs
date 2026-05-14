using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
  private const int WaterMargin = 2;

  [Header("Tilemaps")]
  [SerializeField] private Tilemap waterBackgroundTilemap;
  [SerializeField] private Tilemap waterFoamTilemap;
  [SerializeField] private Tilemap floorTilemap;
  [SerializeField] private Tilemap wallTilemap;
  [SerializeField] private Tilemap shadowTilemap;
  [SerializeField] private Tilemap cliffTilemap;

  [Header("Config")]
  [SerializeField] private MapConfig mapConfig;
  [SerializeField] private MapTilePalette tilePalette;

  public MapConfig Config => mapConfig;
  public int MapWidth => Config.MapWidth;
  public int MapHeight => Config.MapHeight;

  private void Awake()
  {
    if (Config == null)
    {
      throw new Exception("MapConfig reference is missing in MapGenerator.");
    }

    if (tilePalette == null)
    {
      throw new Exception("MapTilePalette reference is missing in MapGenerator.");
    }
  }

  private static readonly MapLayer[] SortedLayers = new[]
  {
    MapLayer.Water,
    MapLayer.Foam,
    MapLayer.Shadow,
    MapLayer.Ground,
    MapLayer.Cliff,
    MapLayer.BoundaryCollider
  };

  public void GenerateMap()
  {
    EnsureVisualTilemaps();
    ClearGeneratedTilemaps();

    foreach (var layer in SortedLayers)
    {
      GenerateLayer(layer);
    }
  }

  private void GenerateLayer(MapLayer layer)
  {
    switch (layer)
    {
      case MapLayer.Water:
        GenerateWaterLayer();
        break;
      case MapLayer.Foam:
        GenerateFoamLayer();
        break;
      case MapLayer.Shadow:
        GenerateShadowLayer();
        break;
      case MapLayer.Ground:
        GenerateGroundLayer();
        break;
      case MapLayer.Cliff:
        GenerateCliffLayer();
        break;
      case MapLayer.BoundaryCollider:
        GenerateBoundaryColliderLayer();
        break;
    }
  }

  private void GenerateWaterLayer()
  {
    if (waterBackgroundTilemap == null || tilePalette.WaterBackgroundTile == null)
    {
      return;
    }

    int waterWidth = MapWidth + WaterMargin * 2;
    int waterHeight = MapHeight + WaterMargin * 2;
    var waterTiles = new TileBase[waterWidth * waterHeight];

    for (int i = 0; i < waterTiles.Length; i++)
    {
      waterTiles[i] = tilePalette.WaterBackgroundTile;
    }

    var bounds = new BoundsInt(-WaterMargin, -WaterMargin, 0, waterWidth, waterHeight, 1);
    waterBackgroundTilemap.SetTilesBlock(bounds, waterTiles);
  }

  private void GenerateBorderTiles(Tilemap tilemap, TileBase tile)
  {
    if (tilemap == null || tile == null) return;

    for (int col = 0; col < MapWidth; col++)
    {
      tilemap.SetTile(new Vector3Int(col, -1, 0), tile);
      tilemap.SetTile(new Vector3Int(col, MapHeight - 1, 0), tile);
    }

    for (int row = -1; row < MapHeight - 1; row++)
    {
      tilemap.SetTile(new Vector3Int(0, row, 0), tile);
      tilemap.SetTile(new Vector3Int(MapWidth - 1, row, 0), tile);
    }
  }

  private void GenerateFoamLayer()
  {
    GenerateBorderTiles(waterFoamTilemap, tilePalette.WaterFoamTile);
  }

  private void GenerateShadowLayer()
  {
    GenerateBorderTiles(shadowTilemap, tilePalette.ShadowTile);
  }

  private void GenerateGroundLayer()
  {
    var floorTiles = new TileBase[MapWidth * MapHeight];
    Array.Fill(floorTiles, tilePalette.FlatGroundTile);

    var bounds = new BoundsInt(0, 0, 0, MapWidth, MapHeight, 1);
    floorTilemap.SetTilesBlock(bounds, floorTiles);
  }

  private void GenerateCliffLayer()
  {
    if (cliffTilemap == null || tilePalette.CliffTile == null)
    {
      return;
    }

    for (int col = 0; col < MapWidth; col++)
    {
      cliffTilemap.SetTile(new Vector3Int(col, -1, 0), tilePalette.CliffTile);
    }
  }

  private void GenerateBoundaryColliderLayer()
  {
    if (wallTilemap == null || tilePalette.InvisibleColliderTile == null)
    {
      return;
    }

    // Top/bottom collider extends to MapHeight (not MapHeight-1) to prevent
    // diagonal clipping — player cannot squeeze through corner diagonally.
    for (int col = -1; col <= MapWidth; col++)
    {
      wallTilemap.SetTile(new Vector3Int(col, -1, 0), tilePalette.InvisibleColliderTile);
      wallTilemap.SetTile(new Vector3Int(col, MapHeight, 0), tilePalette.InvisibleColliderTile);
    }

    for (int row = 0; row < MapHeight; row++)
    {
      wallTilemap.SetTile(new Vector3Int(-1, row, 0), tilePalette.InvisibleColliderTile);
      wallTilemap.SetTile(new Vector3Int(MapWidth, row, 0), tilePalette.InvisibleColliderTile);
    }
  }

  private void EnsureVisualTilemaps()
  {
    waterBackgroundTilemap = EnsureVisualTilemap("WaterBackground", -3);
    waterFoamTilemap = EnsureVisualTilemap("WaterFoam", -2);
    shadowTilemap = EnsureVisualTilemap("Shadow", -1);
    floorTilemap = EnsureVisualTilemap("Floor", 0);
    cliffTilemap = EnsureVisualTilemap("Cliff", 0);
    wallTilemap = EnsureVisualTilemap("Wall", 2);

    SetSortingOrder(floorTilemap, 0);
    SetSortingOrder(wallTilemap, 2);
  }

  private Tilemap EnsureVisualTilemap(string tilemapName, int sortingOrder)
  {
    Transform existing = transform.Find(tilemapName);
    GameObject tilemapObject = existing != null
      ? existing.gameObject
      : new GameObject(tilemapName);

    tilemapObject.transform.SetParent(transform, false);

    var tilemap = tilemapObject.GetComponent<Tilemap>();
    if (tilemap == null)
    {
      tilemap = tilemapObject.AddComponent<Tilemap>();
    }

    var renderer = tilemapObject.GetComponent<TilemapRenderer>();
    if (renderer == null)
    {
      renderer = tilemapObject.AddComponent<TilemapRenderer>();
    }

    renderer.sortingOrder = sortingOrder;
    CopyFloorMaterialTo(renderer);
    return tilemap;
  }

  private void ClearGeneratedTilemaps()
  {
    floorTilemap.ClearAllTiles();
    wallTilemap.ClearAllTiles();
    waterBackgroundTilemap.ClearAllTiles();
    waterFoamTilemap.ClearAllTiles();
    shadowTilemap.ClearAllTiles();
    cliffTilemap.ClearAllTiles();
  }

  private void SetSortingOrder(Tilemap tilemap, int sortingOrder)
  {
    if (tilemap != null && tilemap.TryGetComponent(out TilemapRenderer renderer))
    {
      renderer.sortingOrder = sortingOrder;
    }
  }

  private void CopyFloorMaterialTo(TilemapRenderer renderer)
  {
    var floorRenderer = floorTilemap.GetComponent<TilemapRenderer>();
    if (floorRenderer != null)
    {
      renderer.sharedMaterial = floorRenderer.sharedMaterial;
    }
  }
}
