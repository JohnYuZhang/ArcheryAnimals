using System;
using System.Collections.Generic;
using Riptide;
using UnityEngine;

public class Player : MonoBehaviour
{
    private PlayerController _playerController;
    private PlayerLocomotionInput _playerLocomotionInput;
    private PlayerAnimation _playerAnimation;

    public ushort Id { get; private set; }
    public string Username { get; private set; }

    public static Player InflatePlayer(GameObject playerPrefab, ushort id, string username)
    {
        Player player = Instantiate(playerPrefab, new Vector3(0f, 1f, 0f), Quaternion.identity).GetComponent<Player>();
        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.Id = id;
        player.Username = string.IsNullOrEmpty(username) ? $"Guest {id}" : username;
        return player;
    }

    public Message AppendSpawnData(Message message)
    {
        message.AddUShort(Id);
        message.AddString(Username);
        message.AddVector3(transform.position);
        return message;
    }

    public Message AppendCurrentPosition(Message message)
    {
        message.AddUShort(Id);
        message.AddVector3(transform.position);
        message.AddQuaternion(transform.rotation);
        message.AddVector3(_playerAnimation._currentBlendInput);
        message.AddInt((int)_playerAnimation._playerState.CurrentPlayerMovementState);
        return message;
    }


    public void ApplyPlayerInputState(PlayerInputState playerInputState)
    {
        if (_playerController.isMainPlayer == false)
        {
            // Order matters
            _playerController.UpdateMovementState(playerInputState);
            _playerController.HandleVerticalMovement(playerInputState);
            // Lateral movement needs to be last because it handles the move call
            _playerController.HandleLateralMovement(playerInputState);
            _playerController._playerCamera.rotation = playerInputState.CameraRotation;
            _playerController.transform.rotation = playerInputState.PlayerRotation;
            _playerAnimation.CalculateBlendValue(playerInputState.MovementInput);
            _playerAnimation.UpdateAnimationState();
        }
    }

    public void ApplyPlayerAnimationState(Vector3 blendValue, int playerAnimationState)
    {
        _playerAnimation._playerState.SetPlayerMovementState((PlayerMovementState)playerAnimationState);
        _playerAnimation._currentBlendInput = blendValue;
        _playerAnimation.UpdateAnimationState();
    }

    public PlayerInputState GetCurrentInputState()
    {
        return new PlayerInputState
        {
            CameraRotation = _playerController._playerCamera.rotation,
            LookInput = _playerLocomotionInput.currentPlayerLocomotionState.LookInput,
            JumpPressed = _playerLocomotionInput.currentPlayerLocomotionState.JumpPressed,
            MovementInput = _playerLocomotionInput.currentPlayerLocomotionState.MovementInput,
            SprintToggledOn = _playerLocomotionInput.currentPlayerLocomotionState.SprintToggledOn,
            WalkToggledOn = _playerLocomotionInput.currentPlayerLocomotionState.WalkToggledOn,
            PlayerRotation = _playerController.transform.rotation,
        };
    }

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        _playerAnimation = GetComponent<PlayerAnimation>();
    }

    void Start()
    {

    }

    private void Update()
    {
        
    }
}