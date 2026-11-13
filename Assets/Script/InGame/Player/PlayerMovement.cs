using Common.Motion;
using PlayerInput;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform _camera;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private AnimationPlayer _animationPlayer;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Vector3 _groundRayOffset;
    [SerializeField] private float _groundRayDistance;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _dashSpeed;
    [SerializeField] private float _gravityPower;
    public bool IgnoreInput;
    private Vector3 _velocity;
    private bool _isCrouching = false;
    public Vector2 MoveAmount { get; private set; }

    public void Update()
    {
        Move();
        if (InputService.Player.Crouching.Down)
        {
            _isCrouching = !_isCrouching;
            _animationPlayer.SetCrouching(!_isCrouching);
        }
    }

    private void Move()
    {
        Vector2 moveInput = InputManager.Move;

        if (IgnoreInput)
            moveInput = Vector3.zero;

        float dashAmount = InputManager.Dash;

        //êÊêiÇÃì¸óÕÇ™Ç»Ç¢ÅAâ°é≤ÇÃà⁄ìÆÇ™ã≠Ç∑Ç¨ÇÈèÍçáëñÇÁÇ»Ç¢
        if (moveInput.y < 1f || Mathf.Abs(moveInput.x) > 0.9f)
            dashAmount = 0f;

            float moveSpeed = dashAmount > 0f ? _dashSpeed : _moveSpeed;

        Vector3 cameraInput = GetCameraDirection(moveInput);

        Vector3 groundDirection = cameraInput;
        var hit = GroundRayHit();
        if (hit.HasValue)
        {
            groundDirection = Vector3.ProjectOnPlane(cameraInput, hit.Value.normal);
            _velocity = groundDirection * moveSpeed;
        }
        else
        {
            _velocity = -Vector3.up * _gravityPower;
            moveInput = Vector2.zero;
        }

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
