using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapSpawnArea : MonoBehaviour
{
  [Header("Tilemaps")]
  [SerializeField] private Tilemap floorTilemap;
  [SerializeField] private Tilemap wallTilemap;

  private readonly List<Vector3Int> walkableCells = new();

  public int WalkableCellCount => walkableCells.Count;

  public bool IsReady { get; private set; }

  private void Awake()
  {
    if (floorTilemap == null)
    {
      Debug.LogError("Floor Tilemap reference is not set on TilemapSpawnerArea.");
    }

    if (wallTilemap == null)
    {
      Debug.LogWarning("Wall Tilemap reference is not set on TilemapSpawnerArea. Assuming no walls.");
    }
  }

  public void BuildWalkableCells()
  {
    IsReady = false;
    walkableCells.Clear();

    if (floorTilemap == null) return;

    if (wallTilemap == null)
    {
      Debug.LogWarning("Wall Tilemap reference is not set on TilemapSpawnerArea. Assuming no walls.");
    }

    BoundsInt bounds = floorTilemap.cellBounds;

    foreach (var cell in bounds.allPositionsWithin)
    {
      bool hasFloor = floorTilemap.HasTile(cell);
      bool hasWall = wallTilemap != null && wallTilemap.HasTile(cell);

      if (!hasFloor || hasWall)
      {
        continue;
      }
      walkableCells.Add(cell);
    }

    IsReady = walkableCells.Count > 0;

    if (!IsReady)
    {
      Debug.LogError("TilemapSpawnArea has no valid walkable cells.");
    }
  }

  public bool GetRandomWalkablePosition(out Vector3 worldPosition,
        HashSet<Vector3Int> excludedCells = null)
  {
    worldPosition = default;
    if (!IsReady || walkableCells.Count == 0)
    {
      Debug.LogError("No walkable cells found in TilemapSpawnerArea.");
      return false;
    }

    Vector3Int randomCell;
    int attempts = 0;
    const int maxAttempts = 30;

    do
    {
      randomCell = walkableCells[Random.Range(0, walkableCells.Count)];
      attempts++;
    } while (excludedCells != null && excludedCells.Contains(randomCell) && attempts < maxAttempts);

    if (excludedCells != null && excludedCells.Contains(randomCell))
    {
      // Fallback: if the random cell is excluded, just return the first non-excluded cell.
      foreach (Vector3Int cell in walkableCells)
      {
        if (excludedCells != null && excludedCells.Contains(cell))
        {
          continue;
        }

        worldPosition = floorTilemap.GetCellCenterWorld(cell);
        return true;
      }
      return false;
    }
    else
    {
      worldPosition = floorTilemap.GetCellCenterWorld(randomCell);
      return true;
    }
  }


  public Vector3Int WorldToCell(Vector3 worldPosition)
  {
    if (floorTilemap == null)
    {
      Debug.LogError("Floor Tilemap reference is not set on TilemapSpawnerArea.");
      return Vector3Int.zero;
    }
    return floorTilemap.WorldToCell(worldPosition);
  }
}
