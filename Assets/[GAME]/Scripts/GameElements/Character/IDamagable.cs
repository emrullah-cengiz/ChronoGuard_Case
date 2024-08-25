using UnityEngine;

public interface IDamagable
{
    void TakeDamage(int damage, Vector3 hitDirection, float? hitSpeed = null);
}