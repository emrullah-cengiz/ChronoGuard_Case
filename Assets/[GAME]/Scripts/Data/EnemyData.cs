using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(EnemyData), menuName = nameof(EnemyData), order = 0)]
public class EnemyData : CharacterData
{
    public override CharacterType CharacterType => CharacterType.Enemy;
    
    [PropertyOrder(-1)]
    public EnemyType Type;
    
    [EnumToggleButtons]
    public EnemyAttackType AttackType;
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

public enum EnemyAttackType
{
    Attack1,
    Attack2,
}