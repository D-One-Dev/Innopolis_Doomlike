using UnityEngine;

public sealed class ProjectileAttack : EnemyAttackBase
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float projectileSpeed = 14f;
    [SerializeField] private float projectileRange = 30f;

    protected override void Execute(Transform target)
    {
        if (projectilePrefab == null)
        {
            return;
        }

        Vector3 origin = firePoint != null ? firePoint.position : EyePosition;
        Vector3 targetPoint = target.position + Vector3.up * TargetAimHeight;
        Vector3 direction = (targetPoint - origin).normalized;

        Projectile projectile = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(direction));
        projectile.Launch(new ProjectileLaunchData
        {
            Damage = damage,
            Speed = projectileSpeed,
            MaxDistance = projectileRange,
            Direction = direction,
            Owner = gameObject
        });
    }
}
