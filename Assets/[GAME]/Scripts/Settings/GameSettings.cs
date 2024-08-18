using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(GameSettings), menuName = nameof(GameSettings))]
public class GameSettings : SerializedScriptableObject
{
    public WeaponSettings WeaponSettings;
    [OdinSerialize] [NonSerialized] public Bullet.Pool.PoolSettings BulletPoolSettings;

}

[Serializable]
public class WeaponSettings
{
}

