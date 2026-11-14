using UnityEngine;

public sealed class HealthBehaviour : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;

    public Health HealthModel { get; private set; }
    public bool IsAlive => HealthModel != null && HealthModel.IsAlive;
    public float CurrentHealth => HealthModel?.Current ?? 0f;
    public float MaxHealth => HealthModel?.Max ?? 0f;

    private void Awake()
    {
        HealthModel = new Health(maxHealth);
    }

    public void TakeDamage(DamageInfo damage)
    {
        HealthModel?.TakeDamage(damage);
    }

    public void Heal(float amount)
    {
        HealthModel?.Heal(amount);
    }
}
