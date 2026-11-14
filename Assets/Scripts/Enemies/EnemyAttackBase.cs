using UnityEngine;

public abstract class EnemyAttackBase : MonoBehaviour
{
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float cooldown = 1.5f;
    [SerializeField] private Transform eye;
    [SerializeField] private float eyeHeight = 1.5f;
    [SerializeField] private float targetAimHeight = 1f;
    [SerializeField] private LayerMask lineOfSightMask = ~0;

    private float _nextAttackTime;

    public float AttackRange => attackRange;

    protected Vector3 EyePosition => eye != null ? eye.position : transform.position + Vector3.up * eyeHeight;
    protected float TargetAimHeight => targetAimHeight;
    protected LayerMask LineOfSightMask => lineOfSightMask;

    public bool TryAttack(Transform target)
    {
        if (target == null || Time.time < _nextAttackTime)
        {
            return false;
        }

        Vector3 targetPoint = target.position + Vector3.up * targetAimHeight;
        if (Vector3.Distance(EyePosition, targetPoint) > attackRange)
        {
            return false;
        }

        if (!HasLineOfSight(target, targetPoint))
        {
            return false;
        }

        _nextAttackTime = Time.time + Mathf.Max(0.01f, cooldown);
        Execute(target);
        return true;
    }

    protected abstract void Execute(Transform target);

    private bool HasLineOfSight(Transform target, Vector3 targetPoint)
    {
        Vector3 origin = EyePosition;
        Vector3 direction = targetPoint - origin;
        float distance = direction.magnitude;

        if (distance <= 0.001f)
        {
            return true;
        }

        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            direction / distance,
            distance,
            lineOfSightMask,
            QueryTriggerInteraction.Ignore);

        System.Array.Sort(hits, (first, second) => first.distance.CompareTo(second.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.transform.root == transform.root)
            {
                continue;
            }

            return hit.collider.transform.root == target.root;
        }

        return true;
    }
}
