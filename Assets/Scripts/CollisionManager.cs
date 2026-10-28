using UnityEngine;
using System.Collections.Generic;

public class CollisionManager : MonoBehaviour
{
    public static CollisionManager I;
    
    private readonly List<AABBCollider> _colliders = new();

    private void Awake()
    {
        if (I == null)
        {
            I = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Register(AABBCollider collider)
    {
        if (!_colliders.Contains(collider))
            _colliders.Add(collider);
    }

    public void Unregister(AABBCollider collider)
    {
        if (_colliders.Contains(collider))
            _colliders.Remove(collider);
    }

    public void FixedUpdate()
    {
        for (int i = 0; i < _colliders.Count; i++)
        {
            var colliderA = _colliders[i];
            for (int j = i + 1; j < _colliders.Count; j++)
            {
                var colliderB = _colliders[j];
                if (Overlaps(colliderA, colliderB))
                {
                    colliderA.OnCollision(colliderB);
                    colliderB.OnCollision(colliderA);
                }
            }
        }
    }

    bool Overlaps(AABBCollider a, AABBCollider b)
    {
        return a.Min.x <= b.Max.x && b.Min.x <= a.Max.x &&
               a.Min.y <= b.Max.y && b.Min.y <= a.Max.y &&
               a.Min.z <= b.Max.z && b.Min.z <= a.Max.z; 
    }
}
