using UnityEngine;

public sealed class AmmoPickup : PickupBase
{
    [SerializeField] private AmmoDefinition ammo;
    [SerializeField] private int amount = 20;

    public override bool TryCollect(PlayerPickupCollector collector)
    {
        return ammo != null && collector.TryAddAmmo(ammo, amount);
    }
}
