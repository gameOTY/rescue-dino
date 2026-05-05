using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
  [Header("Tilemaps")]
  [SerializeField] private Tilemap floorTilemap;
  [SerializeField] private Tilemap wallTilemap;

  public int MapWidth => mapWidth;
  public int MapHeight => mapHeight;

  [Header("Tile Assets")]
  [SerializeField] private TileBase floorTile;
  [SerializeField] private TileBase wallTile;

  [Header("Map Dimensions")]
  // [Range] shows a slider in the Inspector — tweak room size without touching code.
  [Range(5, 30)][SerializeField] private int mapWidth = 20;
  [Range(5, 30)][SerializeField] private int mapHeight = 20;

  // 2D char array storing the room layout.
  // 'F' = floor, 'W' = wall, ' ' = empty (no tile placed)
  // Declared as [rows, cols] so indexing is [row, col] = [Y, X]
  private char[,] roomLayout;

  private void Awake()
  {
    BuildRoomLayout();
  }

  private void Start()
  {
    // Tiles cleared by SceneBootstrap before GenerateMap — no need to clear here.
  }

  private bool IsFloor(int row, int col)
  {
    // Guard: if out of bounds, treat it as empty space (not floor).
    if (row < 0 || row >= mapHeight || col < 0 || col >= mapWidth)
      return false;

    return roomLayout[row, col] == 'F';
  }
  private void BuildRoomLayout()
  {
    roomLayout = new char[mapHeight, mapWidth];

    for (int row = 0; row < mapHeight; row++)
    {
      for (int col = 0; col < mapWidth; col++)
      {
        bool isTopOrBottom = row == 0 || row == mapHeight - 1;
        bool isLeftOrRight = col == 0 || col == mapWidth - 1;

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
    Debug.Log("Room layout:\n" + layoutStr);
  }

  public void GenerateMap()
  {
    Debug.Log($"[MapGenerator] GenerateMap called. floorTilemap={(floorTilemap != null)}, floorTile={(floorTile != null)}");
    var floorTiles = new TileBase[mapWidth * mapHeight];
    var wallTiles = new TileBase[mapWidth * mapHeight];

    for (int row = 0; row < mapHeight; row++)
      for (int col = 0; col < mapWidth; col++)
      {
        int i = row * mapWidth + col;
        if (roomLayout[row, col] == 'F')
          floorTiles[i] = floorTile;
        if (roomLayout[row, col] == 'W')
          wallTiles[i] = wallTile;
      }

    var bounds = new BoundsInt(0, 0, 0, mapWidth, mapHeight, 1);
    floorTilemap.SetTilesBlock(bounds, floorTiles);
    wallTilemap.SetTilesBlock(bounds, wallTiles);
  }
}
