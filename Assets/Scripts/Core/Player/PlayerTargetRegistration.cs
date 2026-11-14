using UnityEngine;
using Zenject;

public sealed class PlayerTargetRegistration : MonoBehaviour
{
    private PlayerTargetService _targetService;
    private PlayerDamageable _damageable;

    [Inject]
    private void Construct(PlayerTargetService targetService)
    {
        _targetService = targetService;
    }

    private void Start()
    {
        _damageable = GetComponent<PlayerDamageable>();
        if (_damageable == null)
        {
            Debug.LogError("PlayerTargetRegistration requires PlayerDamageable on the same object.");
            return;
        }

        _targetService.SetTarget(transform, _damageable);
    }

    private void OnDestroy()
    {
        if (_targetService != null)
        {
            _targetService.ClearTarget(transform);
        }
    }
}
