using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
  private const string HighScoreKey = "RescueDino.HighScore";

  [Header("Game Rules")]
  [SerializeField] private GameConfig gameConfig;

  public GameConfig Config => gameConfig;

  public float PlayerHealth { get; private set; }
  public int SoldiersRescued { get; private set; }
  public int SoldiersDied { get; private set; }
  public int HighScore { get; private set; }
  public float TimeRemaining { get; private set; }
  public bool IsGameOver { get; private set; }
  public bool IsWon { get; private set; }
  public bool IsTerminal => IsGameOver || IsWon;

  public int CurrentScore => SoldiersRescued;
  public float MaxHealth => Config.DeathLimit;
  public float TotalTime => Config.SurviveDuration;
  public float NormalizedHealth => MaxHealth > 0 ? PlayerHealth / MaxHealth : 0f;
  public float NormalizedTime => TotalTime > 0 ? TimeRemaining / TotalTime : 0f;


  // ─── Lifecycle ─────────────────────────────────────────

  private void Awake()
  {
    if (Config == null)
    {
      throw new Exception("GameConfig reference is missing in GameManager.");
    }

    HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
    ResetRuntimeState();
  }

  private void Update()
  {
    if (IsTerminal) return;

    TimeRemaining -= Time.deltaTime;
    if (TimeRemaining <= 0f)
    {
      TimeRemaining = 0f;
      TriggerWin();
    }
  }

  public void ResetRuntimeState()
  {
    PlayerHealth = Config.DeathLimit;
    SoldiersRescued = 0;
    SoldiersDied = 0;
    TimeRemaining = Config.SurviveDuration;
    IsGameOver = false;
    IsWon = false;
  }

  public void StartNewGame()
  {
    Scene activeScene = SceneManager.GetActiveScene();
    if (activeScene.buildIndex >= 0)
    {
      SceneManager.LoadScene(activeScene.buildIndex);
      return;
    }

    SceneManager.LoadScene(activeScene.name);
  }

  // ─── Events ─────────────────────────────────────────────

  public event Action GameOver;
  public event Action<int, string> PlayerDamaged;

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
    PlayerDamaged?.Invoke(amount, reason);

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
    SaveHighScoreIfNeeded();
    GameOver?.Invoke();
  }

  private void TriggerWin()
  {
    if (IsWon) return;

    IsWon = true;
    SaveHighScoreIfNeeded();
  }

  private void SaveHighScoreIfNeeded()
  {
    if (CurrentScore <= HighScore) return;

    HighScore = CurrentScore;
    PlayerPrefs.SetInt(HighScoreKey, HighScore);
    PlayerPrefs.Save();
  }
}
