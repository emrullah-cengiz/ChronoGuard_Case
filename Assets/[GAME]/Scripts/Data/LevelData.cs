using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(LevelData), menuName = nameof(LevelData), order = 0)]
public class LevelData : ScriptableObject
{
    public List<WaveData> Waves = new();
}

[Serializable]
public class WaveData
{
   public EnemyType EnemiesType;
   public int SpawnTimeAfterLevelStart;
   public int EnemyNumber;
}
