using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(LevelData), menuName = nameof(LevelData), order = 0)]
public class LevelData : ScriptableObject
{
    public PlayerProperties PlayerProperties;
    [Range(0.5f, 1)]
    public float DifficultyMultiplier = .5f;
    public List<WaveData> Waves = new();
}

[Serializable]
public class WaveData
{
   public EnemyType EnemiesType;
   public float SpawnTimeAfterLevelStart;
   public int EnemyNumber;
   // public float SpawnDistanceFromPlayer;
}
