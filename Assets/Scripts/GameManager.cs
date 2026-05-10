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

  public event Action GameOver;

  public void RegisterRescued()
  {
    if (IsGameOver) return;

    ++SoldiersRescued;
  }

  public void RegisterDead()
  {
    if (IsGameOver) return;

    ++SoldiersDied;
    RegisterDamage(1, "soldier died");
  }

  public void RegisterDamage(int amount, string reason = "unknown")
  {
    if (IsTerminal) return;

    PlayerHealth -= amount;
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
