using System;
using UnityEngine;

public sealed class PlayerTargetService
{
    public Transform Target { get; private set; }
    public IDamageable Damageable { get; private set; }
    public bool HasTarget => Target != null && Damageable != null && Damageable.IsAlive;

    public event Action<Transform> TargetChanged;

    public void SetTarget(Transform target, IDamageable damageable)
    {
        Target = target;
        Damageable = damageable;
        TargetChanged?.Invoke(target);
    }

    public void ClearTarget(Transform target)
    {
        if (Target != target)
        {
            return;
        }

        Target = null;
        Damageable = null;
        TargetChanged?.Invoke(null);
    }
}
