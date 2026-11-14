using System;
using UnityEngine;
using Zenject;

public sealed class GameplayLogger : IInitializable, IDisposable
{
    private readonly PlayerHealth _health;
    private readonly WeaponInventory _inventory;

    public GameplayLogger(PlayerHealth health, WeaponInventory inventory)
    {
        _health = health;
        _inventory = inventory;
    }

    public void Initialize()
    {
        _health.Changed += OnHealthChanged;
        _health.Died += OnPlayerDied;
        _inventory.WeaponChanged += OnWeaponChanged;
        _inventory.AmmoChanged += OnAmmoChanged;

        LogWeapon();
        Debug.Log($"[Health] {_health.Current:0}/{_health.Max:0}");
    }

    public void Dispose()
    {
        _health.Changed -= OnHealthChanged;
        _health.Died -= OnPlayerDied;
        _inventory.WeaponChanged -= OnWeaponChanged;
        _inventory.AmmoChanged -= OnAmmoChanged;
    }

    private void OnHealthChanged(float current, float max)
    {
        Debug.Log($"[Health] {current:0}/{max:0}");
    }

    private void OnPlayerDied()
    {
        Debug.Log("[Player] Dead");
    }

    private void OnWeaponChanged(WeaponConfig weapon)
    {
        LogWeapon();
    }

    private void OnAmmoChanged(AmmoDefinition ammo, int amount)
    {
        Debug.Log($"[Ammo] {ammo.DisplayName}: {amount}/{ammo.MaxAmount}");
    }

    private void LogWeapon()
    {
        WeaponConfig weapon = _inventory.CurrentWeapon;
        if (weapon == null)
        {
            Debug.Log("[Weapon] None");
            return;
        }

        Debug.Log($"[Weapon] {weapon.DisplayName} ({_inventory.GetAmmo(weapon.Ammo)} ammo)");
    }
}
