using UnityEngine;

[CreateAssetMenu(fileName = nameof(PlayerProperties), menuName = nameof(PlayerProperties), order = 0)]
public class PlayerProperties : ScriptableObject
{
    public int MaxHealth = 100;
}