using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class WeaponInventory
{
    private readonly List<WeaponConfig> _weapons = new List<WeaponConfig>();
    private readonly Dictionary<AmmoDefinition, int> _ammo = new Dictionary<AmmoDefinition, int>();
    private int _currentIndex = -1;

    public IReadOnlyList<WeaponConfig> Weapons => _weapons;
    public int WeaponCount => _weapons.Count;
    public WeaponConfig CurrentWeapon => _currentIndex >= 0 && _currentIndex < _weapons.Count
        ? _weapons[_currentIndex]
        : null;

    public event Action<WeaponConfig, bool> WeaponAdded;
    public event Action<WeaponConfig> WeaponChanged;
    public event Action<AmmoDefinition, int> AmmoChanged;

    public WeaponInventory(WeaponConfig startingWeapon, int startingAmmo)
    {
        if (startingWeapon == null)
        {
            return;
        }

        _weapons.Add(startingWeapon);
        _currentIndex = 0;
        AddAmmo(startingWeapon.Ammo, startingAmmo);
    }

    public int GetAmmo(AmmoDefinition ammo)
    {
        if (ammo == null)
        {
            return 0;
        }

        return _ammo.TryGetValue(ammo, out int amount) ? amount : 0;
    }

    public int AddAmmo(AmmoDefinition ammo, int amount)
    {
        if (ammo == null || amount <= 0)
        {
            return 0;
        }

        int current = GetAmmo(ammo);
        int next = Mathf.Min(current + amount, Mathf.Max(0, ammo.MaxAmount));
        int added = next - current;

        if (added <= 0)
        {
            return 0;
        }

        _ammo[ammo] = next;
        AmmoChanged?.Invoke(ammo, next);
        return added;
    }

    public bool AddWeapon(WeaponConfig weapon, int ammoAmount)
    {
        if (weapon == null)
        {
            return false;
        }

        bool isNew = !_weapons.Contains(weapon);

        if (isNew)
        {
            _weapons.Add(weapon);
            _currentIndex = _weapons.Count - 1;
            WeaponChanged?.Invoke(CurrentWeapon);
        }

        int addedAmmo = ammoAmount > 0 ? AddAmmo(weapon.Ammo, ammoAmount) : 0;
        if (!isNew && addedAmmo <= 0)
        {
            return false;
        }

        WeaponAdded?.Invoke(weapon, isNew);
        return true;
    }

    public bool TryConsumeAmmo(WeaponConfig weapon, int amount)
    {
        if (weapon == null || weapon.Ammo == null || amount <= 0)
        {
            return false;
        }

        int current = GetAmmo(weapon.Ammo);
        if (current < amount)
        {
            return false;
        }

        int next = current - amount;
        _ammo[weapon.Ammo] = next;
        AmmoChanged?.Invoke(weapon.Ammo, next);
        return true;
    }

    public bool SelectWeapon(WeaponConfig weapon)
    {
        int index = _weapons.IndexOf(weapon);
        if (index < 0)
        {
            return false;
        }

        if (index != _currentIndex)
        {
            _currentIndex = index;
            WeaponChanged?.Invoke(CurrentWeapon);
        }

        return true;
    }

    public void CycleWeapon(int direction)
    {
        if (_weapons.Count <= 1)
        {
            return;
        }

        int next = (_currentIndex + direction) % _weapons.Count;
        if (next < 0)
        {
            next += _weapons.Count;
        }

        _currentIndex = next;
        WeaponChanged?.Invoke(CurrentWeapon);
    }
}
