using UnityEngine;
using Zenject;

public sealed class PlayerPickupCollector : MonoBehaviour
{
    private WeaponInventory _inventory;
    private PlayerHealth _health;

    public bool IsAlive => _health != null && _health.IsAlive;

    [Inject]
    private void Construct(WeaponInventory inventory, PlayerHealth health)
    {
        _inventory = inventory;
        _health = health;
    }

    public bool TryAddWeapon(WeaponConfig weapon, int ammoAmount)
    {
        return _inventory != null && _inventory.AddWeapon(weapon, ammoAmount);
    }

    public bool TryAddAmmo(AmmoDefinition ammo, int amount)
    {
        return _inventory != null && _inventory.AddAmmo(ammo, amount) > 0;
    }

    public bool TryHeal(float amount)
    {
        return _health != null && _health.Heal(amount) > 0f;
    }
}
