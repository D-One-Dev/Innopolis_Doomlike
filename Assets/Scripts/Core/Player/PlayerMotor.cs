using UnityEngine;
using Zenject;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public sealed class PlayerMotor : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float jumpHeight = 1.1f;
    [SerializeField] private float mouseSensitivity = 0.12f;
    [SerializeField] private float maxPitch = 89f;
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private float groundCheckOffset = 0.05f;

    private readonly Collider[] _groundHits = new Collider[16];
    private Rigidbody _rigidbody;
    private Collider _collider;
    private PlayerInputService _input;
    private PlayerHealth _health;
    private Vector2 _moveInput;
    private bool _sprintHeld;
    private float _pitch;
    private bool _jumpRequested;
    private bool _isGrounded;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();

        _rigidbody.useGravity = true;
        _rigidbody.freezeRotation = true;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
            }
        }

        SetCursorLocked(true);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorLocked(hasFocus);
    }

    [Inject]
    private void Construct(PlayerInputService input, PlayerHealth health)
    {
        _input = input;
        _health = health;
        _input.JumpPressed += OnJumpPressed;
    }

    private void OnDestroy()
    {
        if (_input != null)
        {
            _input.JumpPressed -= OnJumpPressed;
        }
    }

    private void Update()
    {
        if (_input == null)
        {
            return;
        }

        _moveInput = _input.MoveInput;
        _sprintHeld = _input.SprintHeld;

        if (_health == null || !_health.IsAlive)
        {
            return;
        }

        UpdateLook();
    }

    private void FixedUpdate()
    {
        if (_rigidbody == null)
        {
            return;
        }

        if (_input == null || _health == null || !_health.IsAlive)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _jumpRequested = false;
            return;
        }

        _isGrounded = CheckGrounded();

        Vector3 direction = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        if (direction.sqrMagnitude > 1f)
        {
            direction.Normalize();
        }

        float speed = moveSpeed * (_sprintHeld ? sprintMultiplier : 1f);
        Vector3 horizontalVelocity = direction * speed;
        float verticalVelocity = _rigidbody.linearVelocity.y;

        if (_isGrounded)
        {
            if (verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if (_jumpRequested)
            {
                verticalVelocity = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * jumpHeight);
            }
        }

        _jumpRequested = false;
        _rigidbody.linearVelocity = new Vector3(horizontalVelocity.x, verticalVelocity, horizontalVelocity.z);
    }

    private void UpdateLook()
    {
        if (cameraTransform == null)
        {
            return;
        }

        Vector2 look = _input.ConsumeLookDelta();
        transform.Rotate(Vector3.up, look.x * mouseSensitivity, Space.Self);
        _pitch = Mathf.Clamp(_pitch - look.y * mouseSensitivity, -maxPitch, maxPitch);
        cameraTransform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    private bool CheckGrounded()
    {
        Bounds bounds = _collider.bounds;
        float radius = Mathf.Max(0.05f, groundCheckRadius);
        Vector3 origin = new Vector3(
            bounds.center.x,
            bounds.min.y + radius - groundCheckOffset,
            bounds.center.z);

        int hitCount = Physics.OverlapSphereNonAlloc(
            origin,
            radius,
            _groundHits,
            groundMask,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hitCount; i++)
        {
            Collider groundCollider = _groundHits[i];
            if (groundCollider != null && groundCollider.transform.root != transform.root)
            {
                return true;
            }
        }

        return false;
    }

    private void OnJumpPressed()
    {
        _jumpRequested = true;
    }

    private static void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
