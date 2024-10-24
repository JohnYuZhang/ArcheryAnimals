using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
public class PlayerActionController : NetworkBehaviour 
{
    #region Class Variables
    [Header("Components")]
    [SerializeField] private CharacterController _characterController;

    private PlayerActionInput _playerActionInput;
    private PlayerState _playerState;

    private PlayerActionState _lastActionState = PlayerActionState.Idling;
    #endregion

    #region Startup
    public override void OnStartClient() {
        base.OnStartClient();
        if (base.IsOwner) {

        } else {
            gameObject.GetComponent<PlayerActionController>().enabled = false;
        }
    }
    private void Awake() {
        _playerActionInput = GetComponent<PlayerActionInput>();
        _playerState = GetComponent<PlayerState>();
    }
    #endregion

    #region Update Logic
    private void Update() {
        UpdateActionState();
        HandleAttackAction();
    }

    private void UpdateActionState() {
        _lastActionState = _playerState.CurrentPlayerActionState;

        bool isChargingBow = _playerActionInput.DrawingBow;
        bool isReleasingBow = !_playerActionInput.DrawingBow && _lastActionState == PlayerActionState.ChargingBow;

        PlayerActionState actionState = isChargingBow ? PlayerActionState.ChargingBow :
                                        isReleasingBow ? PlayerActionState.ReleasingBow :
                                        PlayerActionState.Idling;

        _playerState.SetPlayerActionState(actionState);
    }

    private void HandleAttackAction() {
        
    }
    #endregion

}
