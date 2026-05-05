using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(BoxCollider2D))]
public class TilemapCameraBoundsBuilder : MonoBehaviour
{
  [SerializeField] private Tilemap wallTilemap;
  [SerializeField] private BoxCollider2D cameraBounds;
  [SerializeField] private CinemachineConfiner2D confiner;

  [SerializeField] private Vector2 padding = Vector2.zero;

  public void RebuildFromTilemap()
  {
    wallTilemap.CompressBounds();

    Bounds localBounds = wallTilemap.localBounds;

    Vector3 worldCenter = wallTilemap.transform.TransformPoint(localBounds.center);

    Vector3 worldSize = Vector3.Scale(
        localBounds.size,
        wallTilemap.transform.lossyScale
    );

    cameraBounds.transform.SetPositionAndRotation(new Vector3(
        worldCenter.x,
        worldCenter.y,
        cameraBounds.transform.position.z
    ), Quaternion.identity);
    cameraBounds.transform.localScale = Vector3.one;

    cameraBounds.size = new Vector2(
        Mathf.Max(0f, worldSize.x - padding.x * 2f),
        Mathf.Max(0f, worldSize.y - padding.y * 2f)
    );

    cameraBounds.offset = Vector2.zero;

    confiner.BoundingShape2D = cameraBounds;
    confiner.InvalidateBoundingShapeCache();

    // Confiner2D may apply unwanted rotation to the camera — reset to identity
    confiner.transform.root.rotation = Quaternion.identity;
  }
}
