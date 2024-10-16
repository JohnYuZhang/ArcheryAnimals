using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-2)]
public class PlayerLocomotionInput : MonoBehaviour, PlayerControls.IPlayerLocomotionMapActions
{
    #region Class Variables
    [SerializeField] private bool holdToSprint = true;

    public class PlayerLocomotionState
    {
        public Vector2 MovementInput { get; set; }
        public Vector2 LookInput { get; set; }
        public bool JumpPressed { get; set; }
        public bool SprintToggledOn { get; set; }
        public bool WalkToggledOn { get; set; }
    }

    public PlayerLocomotionState currentPlayerLocomotionState = new PlayerLocomotionState();

    public PlayerControls PlayerControls { get; private set; }
    
    #endregion

    #region Startup
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
    public void OnTransientInputConsumed()
    {
        currentPlayerLocomotionState.JumpPressed = false;
    }
    #endregion

    #region Input Callbacks
    public void OnMovement(InputAction.CallbackContext context) {
        currentPlayerLocomotionState.MovementInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context) {
        currentPlayerLocomotionState.LookInput = context.ReadValue<Vector2>();
    }

    // Review logic for toggle vs hold
    public void OnToggleSprint(InputAction.CallbackContext context) {
        if(context.performed) {
            currentPlayerLocomotionState.SprintToggledOn = holdToSprint || !currentPlayerLocomotionState.SprintToggledOn;
        }
        else if(context.canceled) {
            currentPlayerLocomotionState.SprintToggledOn = !holdToSprint && currentPlayerLocomotionState.SprintToggledOn;
        }
    }

    public void OnJump(InputAction.CallbackContext context) {
        if (!context.performed) {
            return;
        }

        currentPlayerLocomotionState.JumpPressed = true;
    }

    public void OnToggleWalk(InputAction.CallbackContext context) {

        if (!context.performed) {
            return;
        }

        currentPlayerLocomotionState.WalkToggledOn = !currentPlayerLocomotionState.WalkToggledOn;
    }
    #endregion
}
