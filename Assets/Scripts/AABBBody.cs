using UnityEngine;

public class AABBBody : AABBCollisionReceiver
{
    [Header("Physics Settings")]
    [SerializeField] 
    private float _gravity = 9.81f;
    [SerializeField] 
    private bool _useGravity = true;

    private Vector2 _velocity;
    private bool _isOnGround;

    public bool IsOnGround => _isOnGround;

    public Vector2 Velocity
    {
        get => _velocity;
        set => _velocity = value;
    }

    private void Update()
    {
        if (_useGravity && !_isOnGround)
        {
            _velocity.y -= _gravity * Time.deltaTime;
        }

        transform.Translate((Vector3)_velocity * Time.deltaTime);
        _collider.CalculateBounds();

        _isOnGround = false;
    }

    protected override void HandleCollision(AABBCollider other)
    {
        if (other.TryGetComponent<GroundTag>(out _) || other.TryGetComponent<ObstacleTag>(out _))
        {
            DisplaceAABB(other);
        }
    }

    private void DisplaceAABB(AABBCollider other)
    {
        _collider.CalculateBounds();
        other.CalculateBounds();

        float overlapX = Mathf.Min(_collider.Max.x, other.Max.x) - Mathf.Max(_collider.Min.x, other.Min.x);
        float overlapY = Mathf.Min(_collider.Max.y, other.Max.y) - Mathf.Max(_collider.Min.y, other.Min.y);

        if (overlapX < overlapY)
        {
            if (transform.position.x < other.transform.position.x)
                transform.position -= new Vector3(overlapX, 0, 0);
            else
                transform.position += new Vector3(overlapX, 0, 0);

            _velocity.x = 0;
        }
        else
        {
            if (transform.position.y > other.transform.position.y)
            {
                transform.position += new Vector3(0, overlapY, 0);
                _isOnGround = true;
                if (_velocity.y < 0) _velocity.y = 0;
            }
            else
            {
                transform.position -= new Vector3(0, overlapY, 0);
                if (_velocity.y > 0) _velocity.y = 0;
            }
        }

        _collider.CalculateBounds();
    }

    public void ResetGroundState()
    {
        _isOnGround = false;
    }
}