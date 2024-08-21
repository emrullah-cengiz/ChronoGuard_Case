using UnityEngine;

[CreateAssetMenu(fileName = nameof(PlayerProperties), menuName = nameof(PlayerProperties), order = 0)]
public class PlayerProperties : CharacterData
{
    public override CharacterType CharacterType => CharacterType.Player;

    public float BulletSpeed = 10;
    public float BaseSpeed = 4.5f;
}