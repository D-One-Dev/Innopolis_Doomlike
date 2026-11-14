using UnityEngine;
using Zenject;

public sealed class PlayerWeaponController : MonoBehaviour
{
    [SerializeField] private Camera viewCamera;
    [SerializeField] private Transform muzzle;
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private float emptyFireDelay = 0.25f;
    [SerializeField] private bool drawHitscanDebug = true;

    private PlayerInputService _input;
    private PlayerHealth _health;
    private WeaponInventory _inventory;
    private ProjectileSpawner _projectileSpawner;
    private HitscanImpactService _impactService;
    private float _nextFireTime;
    private bool _semiAutoRequested;

    private void Awake()
    {
        if (viewCamera == null)
        {
            viewCamera = Camera.main;
        }
    }

    [Inject]
    private void Construct(
        PlayerInputService input,
        PlayerHealth health,
        WeaponInventory inventory,
        ProjectileSpawner projectileSpawner,
        HitscanImpactService impactService)
    {
        _input = input;
        _health = health;
        _inventory = inventory;
        _projectileSpawner = projectileSpawner;
        _impactService = impactService;

        _input.AttackPressed += OnAttackPressed;
        _input.NextWeaponPressed += OnNextWeaponPressed;
        _input.PreviousWeaponPressed += OnPreviousWeaponPressed;
    }

    private void OnDestroy()
    {
        if (_input == null)
        {
            return;
        }

        _input.AttackPressed -= OnAttackPressed;
        _input.NextWeaponPressed -= OnNextWeaponPressed;
        _input.PreviousWeaponPressed -= OnPreviousWeaponPressed;
    }

    private void Update()
    {
        if (_input == null || _health == null || !_health.IsAlive || _inventory == null || viewCamera == null)
        {
            return;
        }

        WeaponConfig weapon = _inventory.CurrentWeapon;
        if (weapon == null)
        {
            return;
        }

        bool wantsToFire = weapon.Automatic ? _input.AttackHeld : _semiAutoRequested;
        if (wantsToFire && Time.time >= _nextFireTime)
        {
            Fire(weapon);
        }
    }

    private void Fire(WeaponConfig weapon)
    {
        int ammoCost = Mathf.Max(1, weapon.AmmoPerShot);
        if (!_inventory.TryConsumeAmmo(weapon, ammoCost))
        {
            _nextFireTime = Time.time + emptyFireDelay;
            _semiAutoRequested = false;
            Debug.Log($"Out of ammo: {weapon.DisplayName}");
            return;
        }

        _nextFireTime = Time.time + Mathf.Max(0.01f, weapon.FireInterval);
        _semiAutoRequested = false;

        if (weapon.FireMode == WeaponFireMode.Hitscan)
        {
            FireHitscan(weapon);
        }
        else
        {
            FireProjectile(weapon);
        }
    }

    private void FireHitscan(WeaponConfig weapon)
    {
        Vector3 origin = viewCamera.transform.position;
        int pelletCount = Mathf.Max(1, weapon.PelletsPerShot);

        for (int i = 0; i < pelletCount; i++)
        {
            Vector3 direction = ApplySpread(viewCamera.transform.forward, weapon.SpreadDegrees);

            if (Physics.Raycast(origin, direction, out RaycastHit hit, weapon.Range, hitMask, QueryTriggerInteraction.Ignore))
            {
                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(new DamageInfo
                    {
                        Amount = weapon.Damage,
                        Point = hit.point,
                        Normal = hit.normal,
                        Source = gameObject,
                        Instigator = gameObject
                    });
                }

                _impactService?.Spawn(hit.point, hit.normal, hit.collider.transform);

                if (drawHitscanDebug)
                {
                    Debug.DrawLine(origin, hit.point, Color.yellow, 0.2f);
                }
            }
            else if (drawHitscanDebug)
            {
                Debug.DrawRay(origin, direction * weapon.Range, Color.yellow, 0.2f);
            }
        }
    }

    private void FireProjectile(WeaponConfig weapon)
    {
        if (weapon.ProjectilePrefab == null || _projectileSpawner == null)
        {
            Debug.LogWarning($"Weapon '{weapon.DisplayName}' has no projectile prefab.");
            return;
        }

        Vector3 origin = muzzle != null ? muzzle.position : viewCamera.transform.position;
        Vector3 direction = ApplySpread(viewCamera.transform.forward, weapon.SpreadDegrees);

        Projectile projectile = _projectileSpawner.Spawn(
            weapon.ProjectilePrefab,
            origin,
            Quaternion.LookRotation(direction));

        if (projectile == null)
        {
            return;
        }

        projectile.Launch(new ProjectileLaunchData
        {
            Damage = weapon.Damage,
            Speed = weapon.ProjectileSpeed,
            MaxDistance = weapon.Range,
            Direction = direction,
            Owner = gameObject
        });
    }

    private Vector3 ApplySpread(Vector3 forward, float spreadDegrees)
    {
        if (spreadDegrees <= 0f)
        {
            return forward;
        }

        float spread = Mathf.Tan(spreadDegrees * Mathf.Deg2Rad);
        Vector2 offset = Random.insideUnitCircle * spread;
        Transform cameraTransform = viewCamera.transform;
        return (forward + cameraTransform.right * offset.x + cameraTransform.up * offset.y).normalized;
    }

    private void OnAttackPressed()
    {
        _semiAutoRequested = true;
    }

    private void OnNextWeaponPressed()
    {
        _semiAutoRequested = false;
        _inventory?.CycleWeapon(1);
    }

    private void OnPreviousWeaponPressed()
    {
        _semiAutoRequested = false;
        _inventory?.CycleWeapon(-1);
    }
}
