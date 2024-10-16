using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActionController : MonoBehaviour
{
    #region Class Variables
    [Header("Components")]
    [SerializeField] private CharacterController _characterController;

    private PlayerActionInput _playerActionInput;
    private PlayerState _playerState;
    private PlayerController _playerController;
    private PlayerActionState _lastActionState = PlayerActionState.Idling;
    #endregion

    #region Startup
    private void Awake() {
        _playerActionInput = GetComponent<PlayerActionInput>();
        _playerState = GetComponent<PlayerState>();
        _playerController = GetComponent<PlayerController>();
    }
    #endregion

    #region Update Logic
    private void Update() {
        if (_playerController.isMainPlayer)
        {
            UpdateActionState(_playerActionInput.DrawingBow);
            HandleAttackAction();
        }
    }

    public void UpdateActionState(bool drawingBow) {
        _lastActionState = _playerState.CurrentPlayerActionState;

        bool isChargingBow = drawingBow;
        bool isReleasingBow = !drawingBow && _lastActionState == PlayerActionState.ChargingBow;

        PlayerActionState actionState = isChargingBow ? PlayerActionState.ChargingBow :
                                        isReleasingBow ? PlayerActionState.ReleasingBow :
                                        PlayerActionState.Idling;

        _playerState.SetPlayerActionState(actionState);
    }

    private void HandleAttackAction() {
        
    }
    #endregion

}
