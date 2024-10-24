using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Example.ColliderRollbacks;
using Unity.Cinemachine;

[DefaultExecutionOrder(-1)]
public class PlayerController : NetworkBehaviour
{
    #region Class Variables
    [Header("Components")]
    private CharacterController _characterController;
    private Camera _playerCamera;
    public float RotationMismatch { get; private set; } = 0f;
    public bool IsRotatingToTarget { get; private set; } = false;

    [Header("Base Movement")]
    public float walkAcceleration = 25f;
    public float walkSpeed = 2f;
    public float runAcceleration = 35f;
    public float runSpeed = 4f;
    public float sprintAcceleration = 50f;
    public float sprintSpeed = 7f;
    public float inAirAcceleration = 25f;
    public float drag = 20f;
    public float inAirDrag = 5f;
    public float gravity = 25f;
    public float verticalTerminalVelocity = 50f;
    public float jumpSpeed = 0.8f;
    public float movingThreshold = 0.01f;

    [Header("Animation")]
    public float playerModelRotationSpeed = 10f;
    public float rotateToTargetTime = 0.67f;

    [Header("Camera Settings")]
    public float lookSenseH = 0.1f;
    public float lookSenseV = 0.1f;
    public float lookLimitV = 70f;

    [Header("Environmental Details")]
    [SerializeField] private LayerMask _groundLayers;

    private PlayerLocomotionInput _playerLocomotionInput;
    private PlayerState _playerState;

    private Vector2 _cameraRotation = Vector2.zero;
    private Vector2 _playerTargetRotation = Vector2.zero;

    private bool _jumpedLastFrame = false;
    private bool _isRotatingClockwise = false;
    private float _rotatingToTargetTimer = 0f;
    private float _verticalVelocity = 0f;
    // Allows running up slopes and over hills without bumping (basically pulls player down)
    private float _antiBump;
    // Prevents hitching when on steps (Dynamically sets vs built in unity)
    private float _stepOffSet;

    private PlayerMovementState _lastMovementState = PlayerMovementState.Falling;
    #endregion

    #region Startup

    public override void OnStartClient() {
        base.OnStartClient();
        if (!base.IsOwner) {
            GetComponent<PlayerController>().enabled = false;

        }

    }
    private void Awake() {

        _characterController = GetComponent<CharacterController>();
        _playerCamera = Camera.main;
        CinemachineCamera cam = GameObject.Find("FreeLook Camera").GetComponent<CinemachineCamera>();
        cam.Follow = transform;
        cam.LookAt = transform;
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        _playerState = GetComponent<PlayerState>();
        _antiBump = sprintSpeed;
        _stepOffSet = _characterController.stepOffset;

    }

    #endregion

    #region Update Logic
    private void Update() {
        // Order matters
        UpdateMovementState();
        HandleVerticalMovement();
        // Lateral movement needs to be last because it handles the move call
        HandleLateralMovement();
        UpdateCameraRotation();
    }

    private void UpdateMovementState() {
        _lastMovementState = _playerState.CurrentPlayerMovementState;

        bool canRun = CanRun();
        bool isMovementInput = _playerLocomotionInput.MovementInput != Vector2.zero;
        bool isMovingLaterally = IsMovingLaterally();
        bool isSprinting = _playerLocomotionInput.SprintToggledOn && isMovingLaterally;
        bool isWalking = isMovingLaterally && (!canRun || _playerLocomotionInput.WalkToggledOn);
        bool isGrounded = IsGrounded();

        PlayerMovementState lateralState = isWalking ? PlayerMovementState.Walking :
                                           isSprinting ? PlayerMovementState.Sprinting :
                                           isMovingLaterally || isMovementInput ? PlayerMovementState.Running : 
                                           PlayerMovementState.Idling;
        _playerState.SetPlayerMovementState(lateralState);

        // Airborn State
        if ((!isGrounded || _jumpedLastFrame) && _characterController.velocity.y >= 0f) {
            _playerState.SetPlayerMovementState(PlayerMovementState.Jumping);
            _jumpedLastFrame = false;
            _characterController.stepOffset = 0f;
        } else if ((!isGrounded || _jumpedLastFrame) && _characterController.velocity.y < 0f) {
            _playerState.SetPlayerMovementState(PlayerMovementState.Falling);
            _jumpedLastFrame = false;
            _characterController.stepOffset = 0f;
        } else {
            _characterController.stepOffset = _stepOffSet;
        }
    }

    private void HandleVerticalMovement() {
        bool isGrounded = _playerState.InGroundedState();
        _verticalVelocity -= gravity * Time.deltaTime;

        if (isGrounded && _verticalVelocity < 0f) {
            _verticalVelocity = -_antiBump;
        }

        // Jumped
        if(_playerLocomotionInput.JumpPressed && isGrounded) {
            _verticalVelocity += Mathf.Sqrt(jumpSpeed * 3 * gravity);
            _jumpedLastFrame = true;
        }

        if(_playerState.IsStateGroundedState(_lastMovementState) && !isGrounded) {
            _verticalVelocity += _antiBump;
        }

        // Max falling velocity
        if (MathF.Abs(_verticalVelocity) > MathF.Abs(verticalTerminalVelocity)) {
            _verticalVelocity = -1f * Mathf.Abs(verticalTerminalVelocity);
        }
    }
    private void HandleLateralMovement() {
        // Create quick references for current state
        bool isWalking = _playerState.CurrentPlayerMovementState == PlayerMovementState.Walking;
        bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
        bool isGrounded = _playerState.InGroundedState();

        // Handle speed change with sprinting vs run
        float lateralAcceleration = !isGrounded ? inAirAcceleration :
                                    isWalking ? walkAcceleration :
                                    isSprinting ? sprintAcceleration : runAcceleration;
        float clampLateralMagnitude = !isGrounded ? sprintSpeed :
                                      isWalking ? walkSpeed : 
                                      isSprinting ? sprintSpeed : runSpeed;
        
        // Determine direction
        Vector3 cameraForwardXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
        Vector3 cameraRightXZ = new Vector3(_playerCamera.transform.right.x, 0f, _playerCamera.transform.right.z).normalized;
        Vector3 movementDirection = cameraRightXZ * _playerLocomotionInput.MovementInput.x + cameraForwardXZ * _playerLocomotionInput.MovementInput.y;

        // Review time.deltatime?
        Vector3 movementDelta = movementDirection * lateralAcceleration * Time.deltaTime;
        Vector3 newVelocity = _characterController.velocity + movementDelta;

        // Add drag to player
        float dragMagnitude = isGrounded ? drag : inAirDrag;
        Vector3 currentDrag = newVelocity.normalized * dragMagnitude * Time.deltaTime;
        newVelocity = (newVelocity.magnitude > dragMagnitude * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;

        newVelocity = Vector3.ClampMagnitude(new Vector3(newVelocity.x,0,newVelocity.z), clampLateralMagnitude);
        newVelocity.y += _verticalVelocity;
        newVelocity = !isGrounded ? HandleSteepWalls(newVelocity) : newVelocity;
        // Move character (Unity suggests only calling .Move once per frame)
        _characterController.Move(newVelocity * Time.deltaTime);

    }

    // Review this logic
    private Vector3 HandleSteepWalls(Vector3 velocity) {
        Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCast(_characterController, _groundLayers);
        float angle = Vector3.Angle(normal, Vector3.up);
        bool validAngle = angle <= _characterController.slopeLimit;

        if (!validAngle && _verticalVelocity < 0f)
            velocity = Vector3.ProjectOnPlane(velocity, normal);

        return velocity;
    }
    // Camera logic is recomended to happen after movement because it tracks latest player position, makes the camera smoother and reduces jitter etc.
    private void LateUpdate() {
        
    }

    private void UpdateCameraRotation() {
        _cameraRotation.x += lookSenseH * _playerLocomotionInput.LookInput.x;
        // Subtracted because inverted camera for y
        _cameraRotation.y = Mathf.Clamp(_cameraRotation.y - lookSenseV * _playerLocomotionInput.LookInput.y, -lookLimitV, lookLimitV);

        // This is kinda strange cuz transform.eulerAngles.x is always 0 (euler shit is backwards on XY, Y is horizontal, X is vertical and Z is roll). Leaving in for now since it doesn't break anything but remove later after full implementation
        _playerTargetRotation.x += transform.eulerAngles.x + lookSenseH * _playerLocomotionInput.LookInput.x;


        // Review Rotation code
        // Snaps player rotation if idle
        // If rotation mismatch not withing tolerance or rotate to target is active, rotate
        // Also rotate if not idling
        float rotationTolerance = 90f;
        bool isIdling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
        IsRotatingToTarget = _rotatingToTargetTimer > 0;

        if (!isIdling) {
            RotatePlayerToTarget();
        } else if (Mathf.Abs(RotationMismatch) > rotationTolerance || IsRotatingToTarget) {
            UpdateIdleRotation(rotationTolerance);
        }

        _playerCamera.transform.rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0f);

        // Get angle between camera and player
        // Review Linear Alg
        Vector3 camForwardProjectedXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
        Vector3 crossProduct = Vector3.Cross(transform.forward, camForwardProjectedXZ);
        float sign = Mathf.Sign(Vector3.Dot(crossProduct, transform.up));
        RotationMismatch = sign * Vector3.Angle(transform.forward, camForwardProjectedXZ); 
    }

    private void UpdateIdleRotation(float rotationTolerance) {
        // Initiate new rotation direction
        if (Mathf.Abs(RotationMismatch) > rotationTolerance) {
            _rotatingToTargetTimer = rotateToTargetTime;
            _isRotatingClockwise = RotationMismatch > rotationTolerance;
        }
        _rotatingToTargetTimer -= Time.deltaTime;

        // Rotate player
        if (_isRotatingClockwise && RotationMismatch > 0f || !_isRotatingClockwise && RotationMismatch < 0f) {
            RotatePlayerToTarget();
        }
    }
    private void RotatePlayerToTarget() {
        Quaternion targetRotationX = Quaternion.Euler(0f, _playerTargetRotation.x, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotationX, playerModelRotationSpeed * Time.deltaTime);
    }
    #endregion

    #region State Checks
    private bool IsMovingLaterally() {

        Vector3 lateralVelocity = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.z);

        return lateralVelocity.magnitude > movingThreshold;
    }

    // Possibly rework this logic or rename
    // Need 2 different checks for grounded and air born to solve getting stuck on slanted walls
    private bool IsGrounded() {
        bool grounded = _playerState.InGroundedState() ? IsGroundedWhileGrounded() : IsGroundedWhileAirborn();
        return grounded;
    }

    private bool IsGroundedWhileGrounded() {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - _characterController.radius, transform.position.z);

        bool grounded = Physics.CheckSphere(spherePosition, _characterController.radius, _groundLayers, QueryTriggerInteraction.Ignore);

        return grounded;
    }

    // Review logic for slope limit
    private bool IsGroundedWhileAirborn() {
        Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCast(_characterController, _groundLayers);
        float angle = Vector3.Angle(normal, Vector3.up);
        bool validAngle = angle <= _characterController.slopeLimit;

        return _characterController.isGrounded && validAngle;
    }
    private bool CanRun() {
        // Restrict running to only 45deg forward from the player
        return (_playerLocomotionInput.MovementInput.y >= Mathf.Abs(_playerLocomotionInput.MovementInput.x) && _playerState.CurrentPlayerActionState != PlayerActionState.ChargingBow);
    }
    #endregion
}

