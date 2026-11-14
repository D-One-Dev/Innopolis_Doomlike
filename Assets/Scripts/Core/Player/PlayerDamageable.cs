using UnityEngine;
using Zenject;

public sealed class PlayerDamageable : MonoBehaviour, IDamageable
{
    private PlayerHealth _health;

    public bool IsAlive => _health != null && _health.IsAlive;
    public float CurrentHealth => _health?.Current ?? 0f;
    public float MaxHealth => _health?.Max ?? 0f;

    [Inject]
    private void Construct(PlayerHealth health)
    {
        _health = health;
    }

    public void TakeDamage(DamageInfo damage)
    {
        _health?.TakeDamage(damage);
    }
}
