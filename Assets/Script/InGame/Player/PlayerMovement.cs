using PlayerInput;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform _camera;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Vector3 _groundRayOffset;
    [SerializeField] private float _groundRayDistance;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _dashSpeed;
    public bool IgnoreInput;
    private Vector3 _velocity;
    public Vector2 MoveAmount { get; private set; }

    public void Update()
    {
        Move();
    }

    private void Move()
    {
        Vector2 moveInput = InputManager.Move;

        if (IgnoreInput)
            moveInput = Vector3.zero;

        float moveSpeed = InputManager.Dash > 0f ? _dashSpeed : _moveSpeed;

        Vector3 cameraInput = GetCameraDirection(moveInput);

        Vector3 groundDirection = cameraInput;
        var hit = GroundRayHit();
        if (hit.HasValue)
        {
            groundDirection = Vector3.ProjectOnPlane(cameraInput, hit.Value.normal);
        }

        _velocity = groundDirection * moveSpeed;
        var dashAmount = 0f;

        if (moveInput.y > 0f)
            dashAmount = InputManager.Dash;

        MoveAmount = moveInput + Vector2.up * dashAmount;
        _rigidbody.linearVelocity = _velocity;
    }

    private RaycastHit? GroundRayHit()
    {
        Ray ray = new Ray(transform.position + _groundRayOffset, -Vector3.up);
        Debug.DrawRay(ray.origin, ray.direction * _groundRayDistance);

        if (Physics.Raycast(ray, out var hit, _groundRayDistance, _groundMask))
        {
            return hit;
        }

        return null;
    }

    private Vector3 GetCameraDirection(Vector2 input)
    {
        Vector3 forward = _camera.forward;
        Vector3 right = _camera.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        return (forward * input.y) + (right * input.x);
    }
}
