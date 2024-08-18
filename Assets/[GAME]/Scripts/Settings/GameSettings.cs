using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(GameSettings), menuName = nameof(GameSettings))]
public class GameSettings : SerializedScriptableObject
{
    [OdinSerialize] [NonSerialized] 
    public Bullet.Pool.PoolSettings BulletPoolSettings;
    public WeaponSettings WeaponSettings;
    public SaveSettings SaveSettings;

}

[Serializable]
public class WeaponSettings
{
}

[Serializable]
public class SaveSettings
{
    public int SaveIntervalInSeconds = 5;
}

