using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
  [SerializeField] private float moveSpeed = 5f;
  [SerializeField] private float arrivalThreshold = 0.1f;
  [SerializeField] private Camera mainCamera;
  [SerializeField] private GameManager gameManager;

  [SerializeField] private TilemapSpawnArea spawnArea;

  [SerializeField] private LayerMask obstacleLayer;
  [SerializeField] private float skinWidth = 0.02f;

  private readonly RaycastHit2D[] castHits = new RaycastHit2D[8];
  private ContactFilter2D movementFilter;

  private Rigidbody2D rb;
  private Vector2 mousePosition;

  private InputAction clickAction;
  private InputAction pointerPositionAction;

  private void Awake()
  {
    clickAction = InputSystem.actions.FindAction("Click", throwIfNotFound: true);
    pointerPositionAction = InputSystem.actions.FindAction("PointerPosition", throwIfNotFound: true);

    movementFilter = new ContactFilter2D
    {
        useLayerMask = true,
        layerMask = obstacleLayer,
        useTriggers = false
    };
  }


  private void Start()
  {
    rb = GetComponent<Rigidbody2D>();

    if (gameManager == null)
      gameManager = FindObjectOfType<GameManager>();

    if (mainCamera == null)
      mainCamera = Camera.main;
  }

  public void Initialize()
  {
    spawnArea.GetRandomWalkablePosition(out Vector3 spawnPos);
    transform.position = spawnPos;
    mousePosition = spawnPos;
  }

  private void Update()
  {
    if (gameManager.IsTerminal) return;

    if (clickAction.WasPerformedThisFrame())
    {
      Vector2 screenPos = pointerPositionAction.ReadValue<Vector2>();
      mousePosition = mainCamera != null
        ? mainCamera.ScreenToWorldPoint(screenPos)
        : screenPos;
    }
  }

  private void FixedUpdate()
  {
    if (gameManager.IsTerminal)
    {
      return;
    }

    MoveTowardTargetWithCollision();
  }

  private void MoveTowardTargetWithCollision()
  {
    Vector2 currentPosition = rb.position;
    Vector2 toTarget = mousePosition - currentPosition;

    float distanceToTarget = toTarget.magnitude;

    if (distanceToTarget <= arrivalThreshold)
    {
      return;
    }

    Vector2 direction = toTarget.normalized;

    float moveDistance = moveSpeed * Time.fixedDeltaTime;
    moveDistance = Mathf.Min(moveDistance, distanceToTarget);

    int hitCount = rb.Cast(
        direction,
        movementFilter,
        castHits,
        moveDistance + skinWidth
    );

    if (hitCount > 0)
    {
      float closestDistance = GetClosestHitDistance(hitCount);
      float allowedDistance = Mathf.Max(closestDistance - skinWidth, 0f);

      if (allowedDistance <= 0f)
      {
        return;
      }

      Vector2 blockedPosition = currentPosition + direction * allowedDistance;
      rb.MovePosition(blockedPosition);
      return;
    }

    Vector2 nextPosition = currentPosition + direction * moveDistance;
    rb.MovePosition(nextPosition);
  }

  private float GetClosestHitDistance(int hitCount)
  {
    float closestDistance = float.MaxValue;

    for (int i = 0; i < hitCount; i++)
    {
      if (castHits[i].distance < closestDistance)
      {
        closestDistance = castHits[i].distance;
      }
    }

    return closestDistance;
  }
}
