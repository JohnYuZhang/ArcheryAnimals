using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Connection;
using FishNet.Object;

public class PlayerLocomotionInput : NetworkBehaviour, PlayerControls.IPlayerLocomotionMapActions
{

    #region Class Variables
    [SerializeField] private bool holdToSprint = true;


    public PlayerControls PlayerControls { get; private set; }
    public Vector2 MovementInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool SprintToggledOn { get; private set; }
    public bool WalkToggledOn { get; private set; }
    #endregion

    #region Startup
    public override void OnStartClient() {
        base.OnStartClient();
        if (base.IsOwner) {

        } else {
            gameObject.GetComponent<PlayerLocomotionInput>().enabled = false;
        }
    }
    private void OnEnable() {
        PlayerControls = new PlayerControls();
        PlayerControls.Enable();

        PlayerControls.PlayerLocomotionMap.Enable();
        PlayerControls.PlayerLocomotionMap.SetCallbacks(this);

    }

    private void OnDisable() {
        PlayerControls.PlayerLocomotionMap.Disable();
        PlayerControls.PlayerLocomotionMap.RemoveCallbacks(this);
    }
    #endregion

    #region Update
    private void LateUpdate() {
        // Reset jump pressed at end of frame to avoid double jump
        JumpPressed = false;
    }
    #endregion

    #region Input Callbacks
    public void OnMovement(InputAction.CallbackContext context) {
        MovementInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context) {
        LookInput = context.ReadValue<Vector2>();
    }

    // Review logic for toggle vs hold
    public void OnToggleSprint(InputAction.CallbackContext context) {
        if(context.performed) {
            SprintToggledOn = holdToSprint || !SprintToggledOn;
        }
        else if(context.canceled) {
            SprintToggledOn = !holdToSprint && SprintToggledOn;
        }
    }

    public void OnJump(InputAction.CallbackContext context) {
        if (!context.performed) {
            return;
        }

        JumpPressed = true;
    }

    public void OnToggleWalk(InputAction.CallbackContext context) {

        if (!context.performed) {
            return;
        }

        WalkToggledOn = !WalkToggledOn;
    }
        #endregion
}
