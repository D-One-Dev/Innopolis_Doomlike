using UnityEngine;
using Zenject;

public sealed class GameplayInstaller : MonoInstaller
{
    [SerializeField] private WeaponConfig startingWeapon;
    [SerializeField] private int startingAmmo = 32;
    [SerializeField] private float playerMaxHealth = 100f;
    [SerializeField] private bool logGameplayEvents = true;

    [Header("Hitscan Impacts")]
    [Tooltip("Texture with transparency used for the bullet hole decal.")]
    [SerializeField] private Texture2D impactMarkTexture;
    [Tooltip("Optional flash texture. Falls back to the impact mark texture when empty.")]
    [SerializeField] private Texture2D impactFlashTexture;

    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<PlayerInputService>().AsSingle();
        Container.Bind<PlayerHealth>().AsSingle().WithArguments(playerMaxHealth);
        Container.Bind<WeaponInventory>().AsSingle().WithArguments(startingWeapon, startingAmmo);
        Container.Bind<PlayerTargetService>().AsSingle();
        Container.Bind<ProjectileSpawner>().AsSingle();
        Container.BindInterfacesAndSelfTo<HitscanImpactService>()
            .AsSingle()
            .WithArguments(impactMarkTexture, impactFlashTexture);

        if (logGameplayEvents)
        {
            Container.BindInterfacesTo<GameplayLogger>().AsSingle();
        }
    }
}
