using UnityEngine;

[RequireComponent(typeof(AABBBody), typeof(PlayerTag))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] 
    private float _speed = 8.0f;
    [SerializeField] 
    private float _jumpForce = 12.0f;

    private AABBCollider _collider;
    private AABBBody _aabbBody;
    private Vector3 _origPosition;

    private void Start()
    {
        _collider = GetComponent<AABBCollider>();
        _aabbBody = GetComponent<AABBBody>();
        _origPosition = transform.position;
    }

    private void Update()
    {
        Vector2 curVel = _aabbBody.Velocity;

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        curVel.x = horizontalInput * _speed;

        if (Input.GetKeyDown(KeyCode.Space) && _aabbBody.IsOnGround)
        {
            curVel.y = _jumpForce;
            _aabbBody.Velocity = curVel;
            _aabbBody.ResetGroundState();
        }

        _aabbBody.Velocity = curVel;
    }

    public void Respawn()
    {
        transform.position = _origPosition;

        _aabbBody.Velocity = Vector2.zero;
        _aabbBody.ResetGroundState();

        if (_collider != null)
        {
            _collider.CalculateBounds();
        }
    }
}