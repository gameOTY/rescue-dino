using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Rigidbody2D))]

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
  private static readonly int DeathHash = Animator.StringToHash("Death");
  private static readonly int SpeedHash = Animator.StringToHash("Speed");

  [SerializeField] private Camera mainCamera;
  [SerializeField] private GameManager gameManager;
  [SerializeField] private TilemapSpawnArea spawnArea;
  [SerializeField] private GameConfig gameConfig;
  [SerializeField] private LayerMask obstacleLayer;
  [SerializeField] private Animator playerAnimator;
  [SerializeField] private SpriteRenderer spriteRenderer;

  public GameConfig Config => gameConfig;

  private Vector2 _facingDir;

  private readonly RaycastHit2D[] castHits = new RaycastHit2D[8];
  private ContactFilter2D movementFilter;

  private Rigidbody2D rb;
  private Vector2 mousePosition;
  private Color defaultSpriteColor = Color.white;
  private Coroutine damageFeedbackRoutine;
  private Vector3 activeShakeOffset;

  private InputAction clickAction;
  private InputAction pointerPositionAction;

  private void Awake()
  {
    if (Config == null)
    {
      Debug.LogError("[PlayerController] GameConfig was not assigned in the Inspector.");
      enabled = false;
      return;
    }

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

    playerAnimator = GetComponent<Animator>();
    spriteRenderer = GetComponent<SpriteRenderer>();

    if (spriteRenderer != null)
      defaultSpriteColor = spriteRenderer.color;

    if (gameManager != null)
    {
      gameManager.PlayerDamaged += OnPlayerDamaged;
      gameManager.GameOver += OnGameOver;
    }
  }

  private void OnDestroy()
  {
    if (gameManager != null)
    {
      gameManager.PlayerDamaged -= OnPlayerDamaged;
      gameManager.GameOver -= OnGameOver;
    }
  }

  private void OnGameOver()
  {
    if (!isActiveAndEnabled) return;
    TriggerDeath();
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

    bool didMove = false;
    MoveTowardTargetWithCollision(ref didMove);

    if (_facingDir.x > 0.01f)
      spriteRenderer.flipX = false;
    else if (_facingDir.x < -0.01f)
      spriteRenderer.flipX = true;

    SetAnimation(didMove ? "Run" : "Idle");
  }

  private void MoveTowardTargetWithCollision(ref bool didMove)
  {
    Vector2 currentPosition = rb.position;
    Vector2 toTarget = mousePosition - currentPosition;

    float distanceToTarget = toTarget.magnitude;

    if (distanceToTarget <= Config.StopDistance)
    {
      return;
    }

    Vector2 direction = toTarget.normalized;
    _facingDir = direction;

    float moveDistance = Config.MoveSpeed * Time.fixedDeltaTime;
    moveDistance = Mathf.Min(moveDistance, distanceToTarget);

    int hitCount = rb.Cast(
      direction,
      movementFilter,
      castHits,
      moveDistance + Config.SkinWidth
    );

    if (hitCount > 0)
    {
      float closestDistance = GetClosestHitDistance(hitCount);
      float allowedDistance = Mathf.Max(closestDistance - Config.SkinWidth, 0f);

      if (allowedDistance <= 0f)
      {
        return;
      }

      Vector2 blockedPosition = currentPosition + direction * allowedDistance;
      rb.MovePosition(blockedPosition);
      didMove = true;
      return;
    }

    Vector2 nextPosition = currentPosition + direction * moveDistance;
    rb.MovePosition(nextPosition);
    didMove = true;
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

  private void SetAnimation(string state)
  {
    if (playerAnimator == null) return;
    playerAnimator.SetFloat(SpeedHash, state == "Run" ? 1f : 0f);
  }

  public void TriggerDeath()
  {
    StopDamageFeedback();

    // Prevent further movement after death
    rb.linearVelocity = Vector2.zero;
    rb.bodyType = RigidbodyType2D.Kinematic;

    if (playerAnimator != null)
      playerAnimator.SetTrigger(DeathHash);
  }

  private void OnPlayerDamaged(int amount, string reason)
  {
    if (!isActiveAndEnabled) return;

    PlayDamageFeedback();
  }

  private void PlayDamageFeedback()
  {
    StopDamageFeedback();
    damageFeedbackRoutine = StartCoroutine(DamageFeedbackRoutine());
  }

  private IEnumerator DamageFeedbackRoutine()
  {
    float elapsed = 0f;
    float duration = Mathf.Max(Config.DamageFlashDuration, Config.DamageShakeDuration);

    while (elapsed < duration)
    {
      elapsed += Time.deltaTime;

      if (spriteRenderer != null)
      {
        float flashProgress = Config.DamageFlashDuration > 0f
          ? Mathf.Clamp01(elapsed / Config.DamageFlashDuration)
          : 1f;
        spriteRenderer.color = Color.Lerp(Config.DamageFlashColor, defaultSpriteColor, flashProgress);
      }

      if (Config.DamageShakeDuration > 0f && elapsed < Config.DamageShakeDuration)
      {
        float shakeProgress = Mathf.Clamp01(elapsed / Config.DamageShakeDuration);
        float shakeStrength = Config.DamageShakeDistance * (1f - shakeProgress);
        ApplyShakeOffset(Random.insideUnitCircle * shakeStrength);
      }
      else
      {
        ApplyShakeOffset(Vector3.zero);
      }

      yield return null;
    }

    if (spriteRenderer != null)
      spriteRenderer.color = defaultSpriteColor;

    ApplyShakeOffset(Vector3.zero);
    damageFeedbackRoutine = null;
  }

  private void StopDamageFeedback()
  {
    if (damageFeedbackRoutine != null)
    {
      StopCoroutine(damageFeedbackRoutine);
      damageFeedbackRoutine = null;
    }

    if (spriteRenderer != null)
      spriteRenderer.color = defaultSpriteColor;

    ApplyShakeOffset(Vector3.zero);
  }

  private void ApplyShakeOffset(Vector3 offset)
  {
    Vector3 delta = offset - activeShakeOffset;
    Transform shakeTarget = spriteRenderer != null ? spriteRenderer.transform : transform;

    if (shakeTarget == transform)
      transform.position += delta;
    else
      shakeTarget.localPosition += delta;

    activeShakeOffset = offset;
  }
}
