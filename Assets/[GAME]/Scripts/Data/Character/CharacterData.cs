﻿using Sirenix.OdinInspector;

public abstract class CharacterData : SerializedScriptableObject
{
    [ShowInInspector]
    public abstract CharacterType CharacterType { get; }

    public int MaxHealth;
    public int Damage;
    public float AttackRange = .5f;
    public float AttackRateInSeconds = .5f;
}

public enum CharacterType
{
    Player,
    Enemy
}