using UnityEngine;

public sealed class HealthPickup : PickupBase
{
    [SerializeField] private float amount = 25f;

    public override bool TryCollect(PlayerPickupCollector collector)
    {
        return collector.TryHeal(amount);
    }
}
