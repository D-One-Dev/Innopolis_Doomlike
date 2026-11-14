using UnityEngine;

public sealed class HitscanAttack : EnemyAttackBase
{
    [SerializeField] private float damage = 8f;

    protected override void Execute(Transform target)
    {
        Vector3 origin = EyePosition;
        Vector3 targetPoint = target.position + Vector3.up * TargetAimHeight;
        Vector3 direction = (targetPoint - origin).normalized;

        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            direction,
            AttackRange,
            LineOfSightMask,
            QueryTriggerInteraction.Ignore);

        System.Array.Sort(hits, (first, second) => first.distance.CompareTo(second.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.transform.root == transform.root)
            {
                continue;
            }

            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(new DamageInfo
                {
                    Amount = damage,
                    Point = hit.point,
                    Normal = hit.normal,
                    Source = gameObject,
                    Instigator = gameObject
                });
            }

            Debug.DrawLine(origin, hit.point, Color.red, 0.2f);
            return;
        }
    }
}
