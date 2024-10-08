using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float locomotionBlendSpeed = 4f;

    private PlayerLocomotionInput _playerLocomotionInput;
    private PlayerState _playerState;
    private PlayerController _playerController;

    // Create reference to animator parameters
    private static int inputXHash = Animator.StringToHash("inputX");
    private static int inputYHash = Animator.StringToHash("inputY");
    private static int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");

    private static int isGroundedHash = Animator.StringToHash("isGrounded");
    private static int isIdlingHash = Animator.StringToHash("isIdling");
    private static int isRotatingToTargetHash = Animator.StringToHash("isRotatingToTarget");
    private static int isFallingHash = Animator.StringToHash("isFalling");
    private static int isJumpingHash = Animator.StringToHash("isJumping");
    private static int rotationMismatchHash = Animator.StringToHash("rotationMismatch");

    private Vector3 _currentBlendInput = Vector3.zero;

    private float _sprintMaxBlendValue = 1.5f;
    private float _runMaxBlendValue = 1f;
    private float _walkMaxBlendValue = 0.5f;

    private void Awake() {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        _playerState = GetComponent<PlayerState>();
        _playerController = GetComponent<PlayerController>();
    }

    private void Update() {
        UpdateAnimationState();
        // Create update animation state for actions 
    }

    private void UpdateAnimationState() {
        bool isIdling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
        bool isRunning = _playerState.CurrentPlayerMovementState == PlayerMovementState.Running;
        bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
        bool isJumping = _playerState.CurrentPlayerMovementState == PlayerMovementState.Jumping;
        bool isFalling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Falling;
        bool isGrounded = _playerState.InGroundedState();

        // Manual 1.5 scaled, doesn't matter cuz it's normalized? This determines which animation plays not the actual speed
        bool isRunBlendValue = isRunning || isJumping || isFalling;
        Vector2 inputTarget = isSprinting ? _playerLocomotionInput.MovementInput * _sprintMaxBlendValue :
                              isRunBlendValue ? _playerLocomotionInput.MovementInput * _runMaxBlendValue : 
                              _playerLocomotionInput.MovementInput * _walkMaxBlendValue;
        // Look into Lerp Linear Interpolation
        _currentBlendInput = Vector3.Lerp(_currentBlendInput, inputTarget, locomotionBlendSpeed * Time.deltaTime);

        _animator.SetBool(isGroundedHash, isGrounded);
        _animator.SetBool(isIdlingHash, isIdling);
        _animator.SetBool(isFallingHash, isFalling);
        _animator.SetBool(isJumpingHash, isJumping);
        _animator.SetBool(isRotatingToTargetHash, _playerController.IsRotatingToTarget);

        _animator.SetFloat(inputXHash, _currentBlendInput.x);
        _animator.SetFloat(inputYHash, _currentBlendInput.y);
        _animator.SetFloat(inputMagnitudeHash, _currentBlendInput.magnitude);
        _animator.SetFloat(rotationMismatchHash, _playerController.RotationMismatch);
    }
}
