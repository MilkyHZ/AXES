using UnityEngine;

[RequireComponent(typeof(AABBCollider))]
public abstract class AABBCollisionReceiver : MonoBehaviour
{
    protected AABBCollider _collider;

    protected virtual void Awake() => EnsureCollider();

    protected virtual void OnEnable()
    {
        EnsureCollider();
        if (_collider != null)
            _collider.OnCollisionAction += HandleCollision;
    }

    protected virtual void OnDisable()
    {
        if (_collider != null)
            _collider.OnCollisionAction -= HandleCollision;
    }

    private void EnsureCollider()
    {
        if (_collider == null)
            _collider = GetComponent<AABBCollider>();
    }

    protected abstract void HandleCollision(AABBCollider other);
}
