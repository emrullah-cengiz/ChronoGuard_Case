using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(EnemyData), menuName = nameof(EnemyData), order = 0)]
public class EnemyData : ScriptableObject
{
    public EnemyType Type;
    public int Health;
    public int Damage;
    public float AttackRange = .5f;
    public float AttackRateInSeconds = .5f;
    [EnumToggleButtons]
    public EnemySpeedMode SpeedMode;
}


public enum EnemyType
{
    Normal,
    Elite,
    Boss
}

public enum EnemySpeedMode
{
    Walk,
    Run
}