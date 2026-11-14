using UnityEngine;

public sealed class Projectile : MonoBehaviour
{
    [SerializeField] private float collisionRadius = 0.08f;
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private GameObject impactPrefab;

    private float _damage;
    private float _speed;
    private float _remainingDistance;
    private Vector3 _direction;
    private GameObject _owner;
    private bool _launched;

    public void Launch(ProjectileLaunchData data)
    {
        _damage = data.Damage;
        _speed = Mathf.Max(0.01f, data.Speed);
        _remainingDistance = Mathf.Max(0.01f, data.MaxDistance);
        _direction = data.Direction.normalized;
        _owner = data.Owner;
        _launched = true;

        if (_direction.sqrMagnitude > 0f)
        {
            transform.rotation = Quaternion.LookRotation(_direction);
        }
    }

    private void Update()
    {
        if (!_launched)
        {
            return;
        }

        float step = Mathf.Min(_speed * Time.deltaTime, _remainingDistance);
        if (step <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        bool hasHit = Physics.SphereCast(
            transform.position,
            collisionRadius,
            _direction,
            out RaycastHit hit,
            step,
            hitMask,
            QueryTriggerInteraction.Ignore);

        if (hasHit && !IsOwner(hit.collider))
        {
            HandleHit(hit);
            return;
        }

        transform.position += _direction * step;
        _remainingDistance -= step;

        if (_remainingDistance <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private bool IsOwner(Collider hitCollider)
    {
        return _owner != null && hitCollider.transform.root == _owner.transform.root;
    }

    private void HandleHit(RaycastHit hit)
    {
        IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(new DamageInfo
            {
                Amount = _damage,
                Point = hit.point,
                Normal = hit.normal,
                Source = gameObject,
                Instigator = _owner
            });
        }

        if (impactPrefab != null)
        {
            Instantiate(impactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }

        Destroy(gameObject);
    }
}
