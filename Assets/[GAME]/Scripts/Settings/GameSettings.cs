using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(GameSettings), menuName = nameof(GameSettings))]
public class GameSettings : SerializedScriptableObject
{
    [HideLabel] [OdinSerialize] [NonSerialized] [Space(10)]
    public PlayerSettings PlayerSettings;

    [HideLabel] [OdinSerialize] [NonSerialized] [Space(10)]
    public LevelSettings LevelSettings;

    [HideLabel] [OdinSerialize] [NonSerialized] [Space(10)]
    public WeaponSettings WeaponSettings;

    [HideLabel] [OdinSerialize] [NonSerialized] [Space(10)]
    public EnemySettings EnemySettings;

    [HideLabel] [OdinSerialize] [NonSerialized] [Space(10)]
    public SaveSettings SaveSettings;
    
    [HideLabel] [OdinSerialize] [NonSerialized] [Space(10)]
    public VFXSettings VFXSettings;
}

[Serializable]
public class PlayerSettings
{
    public float StayingAimOffset = 6f;
    public float MovingAimOffset = 6f;
    public float LookNewTargetSpeed = 6f;
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
    public float HitImpulseForce = 10f;
    public float BaseSpeed = 3.3f;
    public Dictionary<EnemySpeedMode, float> SpeedMultipliers;
    public Dictionary<EnemyAttackType, float> HitTimePerAnimation;

    [Title("Optimization")] public float AgentSetDestinationRate = .2f;

    [Title("Pool")] [OdinSerialize] [NonSerialized] [Space(10)]
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

[Serializable]
public class VFXSettings
{
    public Pool<ParticleType>.PoolSettings PoolSettings;
}

public enum ParticleType
{
    Blood
}