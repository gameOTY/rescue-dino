using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  [Header("Game Rules")]
  [SerializeField] private float surviveDuration = 60f;
  [SerializeField] private int maxSoldierDeaths = 3;

  public float PlayerHealth { get; private set; }
  public int SoldiersRescued { get; private set; }
  public int SoldiersDied { get; private set; }
  public float TimeRemaining { get; private set; }
  public bool IsGameOver { get; private set; }
  public bool IsWon { get; private set; }
  public bool IsTerminal => IsGameOver || IsWon;

  public float MaxHealth => maxSoldierDeaths;
  public float TotalTime => surviveDuration;
  public float NormalizedHealth => PlayerHealth / maxSoldierDeaths;
  public float NormalizedTime => TimeRemaining / surviveDuration;


  // ─── Lifecycle ─────────────────────────────────────────

  private void Awake()
  {
    ResetRuntimeState();
  }

  private void Update()
  {
    if (IsTerminal) return;

    TimeRemaining -= Time.deltaTime;
    if (TimeRemaining <= 0f)
    {
      TimeRemaining = 0f;
      IsWon = true;
    }
  }

  public void ResetRuntimeState()
  {
    PlayerHealth = maxSoldierDeaths;
    SoldiersRescued = 0;
    SoldiersDied = 0;
    TimeRemaining = surviveDuration;
    IsGameOver = false;
    IsWon = false;
  }

  // ─── Events ─────────────────────────────────────────────

  public event Action<int> RescuedChanged;
  public event Action<int> DeadChanged;
  public event Action GameOver;

  public void RegisterRescued()
  {
    if (IsGameOver) return;

    ++SoldiersRescued;
    RescuedChanged?.Invoke(SoldiersRescued);
  }

  public void RegisterDead()
  {
    if (IsGameOver) return;

    RegisterDamage(1, "soldier died");
    ++SoldiersDied;
    Debug.Log($"[GameManager] Player lost health! Reason: Soldier died. SoldiersDied={SoldiersDied}/{maxSoldierDeaths}");
    DeadChanged?.Invoke(SoldiersDied);
  }

  public void RegisterDamage(int amount, string reason = "unknown")
  {
    if (IsTerminal) return;

    PlayerHealth -= amount;
    Debug.Log($"[GameManager] Player lost {amount} health! Reason: {reason}. PlayerHealth={PlayerHealth}");
    if (PlayerHealth <= 0)
    {
      PlayerHealth = 0;
      TriggerGameOver();
    }
  }

  private void TriggerGameOver()
  {
    if (IsGameOver) return;

    IsGameOver = true;
    GameOver?.Invoke();
  }
}
