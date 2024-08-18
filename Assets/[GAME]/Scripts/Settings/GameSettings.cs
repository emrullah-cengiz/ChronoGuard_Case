using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(GameSettings), menuName = nameof(GameSettings))]
public class GameSettings : SerializedScriptableObject
{
    public PlayerSettings PlayerSettings;

    [OdinSerialize] [NonSerialized] 
    public WeaponSettings WeaponSettings;
    
    [OdinSerialize] [NonSerialized] [Space(10)]
    public EnemySettings EnemySettings;
    
    [Space(10)]
    public SaveSettings SaveSettings;
}

[Serializable]
public class PlayerSettings
{
    public float BaseSpeed = 4.5f;
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
    public float BaseSpeed;
    public Dictionary<EnemySpeedMode, float> SpeedMultipliers;
    
    [OdinSerialize] [NonSerialized] [Space(10)]
    public Enemy.Pool.PoolSettings PoolSettings;

}

[Serializable]
public class SaveSettings
{
    public int SaveIntervalInSeconds = 5;
}
