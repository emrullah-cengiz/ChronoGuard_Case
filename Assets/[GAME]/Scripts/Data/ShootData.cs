﻿using System;
using System.Collections;
using UnityEngine;

[Serializable]
public struct ShootData : IPoolableInitializationData
{
    public Vector3 StartPosition;
    public Vector3 Direction;
    public int Damage;
    public float Speed;
}