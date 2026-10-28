using System;
using UnityEngine;

public class AABBCollider : MonoBehaviour
{
    [SerializeField] 
    private float _width = 1f;  
    [SerializeField]
    private float _height = 1f;
    [SerializeField]
    private float _length = 1f;

    public Vector3 Max { get; private set; }
    public Vector3 Min { get; private set; }

    public Action<AABBCollider> OnCollisionAction;

    private void Start()
    {
        if (CollisionManager.I != null)
        {
            CollisionManager.I.Register(this);
        }
    }

    private void OnDestroy()
    {
        if (CollisionManager.I != null)
        {
            CollisionManager.I.Unregister(this);
        }
    }

    private void FixedUpdate() => CalculateBounds();

    public void CalculateBounds()
    {
        Vector3 position = transform.position;

        Vector3 halfExtents = new Vector3(_width, _height, _length) * 0.5f;

        Max = position + halfExtents;
        Min = position - halfExtents;
    }

    public void OnCollision(AABBCollider other)
    {
        OnCollisionAction?.Invoke(other);
    }

    private void OnDrawGizmos()
    {
        CalculateBounds();

        Gizmos.color = Color.lightGreen;

        Vector3 center = transform.position;
        Vector3 size = new (_width, _height, _length);

        Gizmos.DrawWireCube(center, size);
    }

    [ContextMenu("Set To Local Scale")]
    public void SetToScaleEditor()
    {
        _width = transform.localScale.x;
        _height = transform.localScale.y;
        _length = transform.localScale.z;
    }

}
