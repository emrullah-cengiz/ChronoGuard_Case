using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(PlayerProperties), menuName = nameof(PlayerProperties), order = 0)]
public class PlayerProperties : CharacterData
{
    public override CharacterType CharacterType => CharacterType.Player;

    public float Speed = 4.5f;
    
    [MinValue(1)]
    [InfoBox("2+ for shotgun effect.")]
    public int BulletNumberPerShot;
    public float BulletSpeed = 10;
}