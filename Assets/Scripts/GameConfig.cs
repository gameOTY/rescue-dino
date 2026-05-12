using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Settings/GameConfig")]
public class GameConfig : ScriptableObject
{
  [Header("Game Rules")]
  [SerializeField] private float _surviveDuration = 60f;
  [SerializeField] private int _deathLimit = 3;

  [Header("Player Movement")]
  [SerializeField] private float _moveSpeed = 5f;
  [SerializeField] private float _stopDistance = 0.1f;
  [SerializeField] private float _skinWidth = 0.02f;

  [Header("Player Damage Feedback")]
  [SerializeField] private Color _damageFlashColor = new(1f, 0.25f, 0.25f, 1f);
  [SerializeField] private float _damageFlashDuration = 0.12f;
  [SerializeField] private float _damageShakeDuration = 0.18f;
  [SerializeField] private float _damageShakeDistance = 0.06f;

  [Header("Soldier Spawning")]
  [SerializeField] private float _soldierSpawnInterval = 5f;
  [SerializeField] private float _soliderSpawnDecayRate = 0.1f;
  [SerializeField] private float _minSpawnInterval = 0.5f;
  [SerializeField] private int _maxActiveSoldiers = 3;
  [SerializeField] private float _rescueTime = 3f;
  [SerializeField] private float _soldierLifetime = 10f;

  [Header("Danger Zone")]
  [SerializeField] private float _dangerZoneSpawnInterval = 2f;
  [SerializeField] private float _dangerZoneLifetime = 3f;
  [SerializeField] private int _maxActiveDangerZones = 5;
  [SerializeField] private float _dangerZoneBlinkInterval = 0.2f;

  [Header("Rescue Zone")]
  [SerializeField] private float _rescueZoneDiameter = 1.2f;
  [SerializeField] private Color _rescueZoneColor = new(0f, 1f, 1f, 0.35f);
  [SerializeField] private bool _showZoneOnlyWhenPlayerInside;

  public float SurviveDuration => _surviveDuration;
  public int DeathLimit => _deathLimit;
  public float MoveSpeed => _moveSpeed;
  public float StopDistance => _stopDistance;
  public float SkinWidth => _skinWidth;
  public Color DamageFlashColor => _damageFlashColor;
  public float DamageFlashDuration => _damageFlashDuration;
  public float DamageShakeDuration => _damageShakeDuration;
  public float DamageShakeDistance => _damageShakeDistance;
  public float SoldierSpawnInterval => _soldierSpawnInterval;
  public float SoliderSpawnDecayRate => _soliderSpawnDecayRate;
  public float MinSpawnInterval => _minSpawnInterval;
  public int MaxActiveSoldiers => _maxActiveSoldiers;
  public float RescueTime => _rescueTime;
  public float SoldierLifetime => _soldierLifetime;
  public float DangerZoneSpawnInterval => _dangerZoneSpawnInterval;
  public float DangerZoneLifetime => _dangerZoneLifetime;
  public int MaxActiveDangerZones => _maxActiveDangerZones;
  public float DangerZoneBlinkInterval => _dangerZoneBlinkInterval;
  public float RescueZoneDiameter => _rescueZoneDiameter;
  public Color RescueZoneColor => _rescueZoneColor;
  public bool ShowZoneOnlyWhenPlayerInside => _showZoneOnlyWhenPlayerInside;
}
