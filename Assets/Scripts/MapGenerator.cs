using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
  [Header("Tilemaps")]
  [SerializeField] private Tilemap floorTilemap;
  [SerializeField] private Tilemap wallTilemap;

  [Header("Config")]
  [SerializeField] private MapConfig mapConfig;

  public MapConfig Config => mapConfig;
  public int MapWidth => Config.MapWidth;
  public int MapHeight => Config.MapHeight;

  [Header("Tile Assets")]
  [SerializeField] private TileBase floorTile;
  [SerializeField] private TileBase wallTile;

  // 2D char array storing the room layout.
  // 'F' = floor, 'W' = wall, ' ' = empty (no tile placed)
  // Declared as [rows, cols] so indexing is [row, col] = [Y, X]
  private char[,] roomLayout;

  private void Awake()
  {
    if (Config == null)
    {
      throw new Exception("MapConfig reference is missing in MapGenerator.");
    }

    BuildRoomLayout();
  }

  private void Start()
  {
    // Tiles cleared by SceneBootstrap before GenerateMap — no need to clear here.
  }

  private bool IsFloor(int row, int col)
  {
    // Guard: if out of bounds, treat it as empty space (not floor).
    if (row < 0 || row >= MapHeight || col < 0 || col >= MapWidth)
      return false;

    return roomLayout[row, col] == 'F';
  }
  private void BuildRoomLayout()
  {
    roomLayout = new char[MapHeight, MapWidth];

    for (int row = 0; row < MapHeight; row++)
    {
      for (int col = 0; col < MapWidth; col++)
      {
        bool isTopOrBottom = row == 0 || row == MapHeight - 1;
        bool isLeftOrRight = col == 0 || col == MapWidth - 1;

        roomLayout[row, col] = (isTopOrBottom || isLeftOrRight) ? 'W' : 'F';
      }
    }

    DebugLogLayout();
  }

  private void DebugLogLayout()
  {
    string layoutStr = roomLayout.GetLength(0) + "x" + roomLayout.GetLength(1) + " layout:\n";
    for (int row = 0; row < roomLayout.GetLength(0); row++)
    {
      for (int col = 0; col < roomLayout.GetLength(1); col++)
      {
        layoutStr += roomLayout[row, col] + " ";
      }
      layoutStr += "\n";
    }
  }

  public void GenerateMap()
  {
    var floorTiles = new TileBase[MapWidth * MapHeight];
    var wallTiles = new TileBase[MapWidth * MapHeight];

    for (int row = 0; row < MapHeight; row++)
      for (int col = 0; col < MapWidth; col++)
      {
        int i = row * MapWidth + col;
        if (roomLayout[row, col] == 'F')
          floorTiles[i] = floorTile;
        if (roomLayout[row, col] == 'W')
          wallTiles[i] = wallTile;
      }

    var bounds = new BoundsInt(0, 0, 0, MapWidth, MapHeight, 1);
    floorTilemap.SetTilesBlock(bounds, floorTiles);
    wallTilemap.SetTilesBlock(bounds, wallTiles);
  }
}
