using System;
using UnityEngine;

public class Health
{
    public float Max { get; }
    public float Current { get; private set; }
    public bool IsAlive => Current > 0f;

    public event Action<float, float> Changed;
    public event Action<DamageInfo> Damaged;
    public event Action Died;

    public Health(float max)
    {
        Max = Mathf.Max(1f, max);
        Current = Max;
    }

    public void TakeDamage(DamageInfo damage)
    {
        if (!IsAlive || damage.Amount <= 0f)
        {
            return;
        }

        Current = Mathf.Max(0f, Current - damage.Amount);
        Damaged?.Invoke(damage);
        Changed?.Invoke(Current, Max);

        if (!IsAlive)
        {
            Died?.Invoke();
        }
    }

    public float Heal(float amount)
    {
        if (!IsAlive || amount <= 0f)
        {
            return 0f;
        }

        float healed = Mathf.Min(amount, Max - Current);
        if (healed <= 0f)
        {
            return 0f;
        }

        Current += healed;
        Changed?.Invoke(Current, Max);
        return healed;
    }
}
