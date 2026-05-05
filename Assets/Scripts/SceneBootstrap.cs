using UnityEngine;

public class SceneBootstrap : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private TilemapSpawnArea spawnArea;
    [SerializeField] private TilemapCameraBoundsBuilder cameraBoundsBuilder;

    [Header("Player")]
    [SerializeField] private PlayerController playerController;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        Debug.Log("[SceneBootstrap] Starting init...");
        Debug.Log($"[SceneBootstrap] mapGenerator={(mapGenerator != null ? "OK" : "NULL")}, spawnArea={(spawnArea != null ? "OK" : "NULL")}");
        mapGenerator.GenerateMap();
        Debug.Log("[SceneBootstrap] GenerateMap done");
        cameraBoundsBuilder.RebuildFromTilemap();
        Debug.Log("[SceneBootstrap] RebuildFromTilemap done");
        spawnArea.BuildWalkableCells();
        Debug.Log($"[SceneBootstrap] BuildWalkableCells done. IsReady={spawnArea.IsReady}, WalkableCells={spawnArea.WalkableCellCount}");
        playerController.Initialize();
        Debug.Log("[SceneBootstrap] Init complete");
    }

    public void RegenerateMap()
    {
        mapGenerator.GenerateMap();
        spawnArea.BuildWalkableCells();
    }
}
