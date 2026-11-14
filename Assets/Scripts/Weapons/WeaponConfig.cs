using UnityEngine;

[CreateAssetMenu(fileName = "WeaponConfig", menuName = "Doomlike/Weapon Config")]
public sealed class WeaponConfig : ScriptableObject
{
    [Header("Identity")]
    public string DisplayName = "Weapon";
    public WeaponFireMode FireMode = WeaponFireMode.Hitscan;

    [Header("Ammo")]
    public AmmoDefinition Ammo;
    public int AmmoPerShot = 1;
    public int PickupAmmo = 20;

    [Header("Damage")]
    public float Damage = 10f;
    public int PelletsPerShot = 1;
    public float SpreadDegrees = 0f;
    public float Range = 100f;

    [Header("Fire Rate")]
    public float FireInterval = 0.2f;
    public bool Automatic = true;

    [Header("Projectile")]
    public Projectile ProjectilePrefab;
    public float ProjectileSpeed = 25f;
}
