using UnityEngine;
using Zenject;

public sealed class ProjectileSpawner
{
    private readonly DiContainer _container;

    public ProjectileSpawner(DiContainer container)
    {
        _container = container;
    }

    public Projectile Spawn(Projectile prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            return null;
        }

        return _container.InstantiatePrefabForComponent<Projectile>(prefab, position, rotation, null);
    }
}
