using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class PickupBase : MonoBehaviour, IPickup
{
    private void Awake()
    {
        foreach (Collider pickupCollider in GetComponents<Collider>())
        {
            pickupCollider.isTrigger = true;
        }

        if (!TryGetComponent(out Rigidbody body))
        {
            body = gameObject.AddComponent<Rigidbody>();
        }

        body.isKinematic = true;
        body.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerPickupCollector collector = other.GetComponentInParent<PlayerPickupCollector>();
        if (collector == null || !collector.IsAlive)
        {
            return;
        }

        if (TryCollect(collector))
        {
            Destroy(gameObject);
        }
    }

    public abstract bool TryCollect(PlayerPickupCollector collector);
}
