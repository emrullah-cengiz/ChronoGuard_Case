using UnityEngine;

[CreateAssetMenu(fileName = nameof(EnemyData), menuName = nameof(EnemyData), order = 0)]
public class EnemyData : ScriptableObject
{
    public EnemyType Type;
    public int BaseHealth;
    public int BaseDamage;
    public float BaseSpeed;
}


public enum EnemyType
{
    Normal,
    Elite,
    Boss
}