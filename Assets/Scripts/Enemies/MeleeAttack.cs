using UnityEngine;

public sealed class MeleeAttack : EnemyAttackBase
{
    [SerializeField] private float damage = 12f;

    protected override void Execute(Transform target)
    {
        IDamageable damageable = target.GetComponentInParent<IDamageable>();
        if (damageable == null)
        {
            return;
        }

        damageable.TakeDamage(new DamageInfo
        {
            Amount = damage,
            Point = target.position,
            Normal = -target.forward,
            Source = gameObject,
            Instigator = gameObject
        });
    }
}
