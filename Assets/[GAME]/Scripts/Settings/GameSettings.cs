using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(GameSettings), menuName = nameof(GameSettings))]
public class GameSettings : SerializedScriptableObject
{    
    [HideLabel][OdinSerialize] [NonSerialized] [Space(10)] public PlayerSettings PlayerSettings;
    [HideLabel][OdinSerialize] [NonSerialized] [Space(10)] public LevelSettings LevelSettings;
    [HideLabel][OdinSerialize] [NonSerialized] [Space(10)] public WeaponSettings WeaponSettings;
    [HideLabel][OdinSerialize] [NonSerialized] [Space(10)] public EnemySettings EnemySettings;

    [Space(10)] public SaveSettings SaveSettings;
}

[Serializable]
public class PlayerSettings
{
    public float AimRotationOffset = 6f;
}

[Serializable]
public class WeaponSettings
{
    [OdinSerialize] [NonSerialized] [Space(10)]
    public Bullet.Pool.PoolSettings PoolSettings;
}

[Serializable]
public class EnemySettings
{
    public float InWaveSpawnDelayInSeconds = .3f;
    public float DespawnDelayAfterDead = .5f;
    public Dictionary<EnemySpeedMode, float> SpeedMultipliers;
    public Dictionary<EnemyAttackType, float> HitTimePerAnimation;
    
    [Title("Optimization")]
    public float SetDestinationRate = .2f;

    [Title("Pool")]
    [OdinSerialize] [NonSerialized] [Space(10)]
    public Enemy.Pool.PoolSettings PoolSettings;
}

[Serializable]
public class SaveSettings
{
    public int SaveIntervalInSeconds = 5;
}

[Serializable]
public class LevelSettings
{
    public int LevelCountdownDurationInSeconds = 180;
    public int MaxLevel = 3;
}