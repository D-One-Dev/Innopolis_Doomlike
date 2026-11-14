using UnityEngine;

public sealed class WeaponPickup : PickupBase
{
    [SerializeField] private WeaponConfig weapon;
    [SerializeField] private int ammoAmount;

    public override bool TryCollect(PlayerPickupCollector collector)
    {
        if (weapon == null)
        {
            return false;
        }

        int amount = ammoAmount > 0 ? ammoAmount : weapon.PickupAmmo;
        return collector.TryAddWeapon(weapon, amount);
    }
}
