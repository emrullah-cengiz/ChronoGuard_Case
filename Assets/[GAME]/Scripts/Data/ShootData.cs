using System;
using UnityEngine;

[Serializable]
public struct ShootData : IPoolableInitializationData
{
    public Vector3 StartPosition;
    public Quaternion StartRotation;
    public Vector3 Direction;
    public int Damage;
    public float Speed;
}
