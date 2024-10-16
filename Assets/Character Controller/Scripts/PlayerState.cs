using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Possibly refactor later to state machine
public class PlayerState : MonoBehaviour
{
    // [field: SerializeField] lets you serialize properties with private set to view in editor
    [field: SerializeField] public PlayerMovementState CurrentPlayerMovementState {  get; private set; } = PlayerMovementState.Idling;
    [field: SerializeField] public PlayerActionState CurrentPlayerActionState { get; private set; } = PlayerActionState.Idling;

    public void SetPlayerMovementState(PlayerMovementState playerMovementState) {
        CurrentPlayerMovementState = playerMovementState;
    }

    public void SetPlayerActionState(PlayerActionState playerActionState) {
        CurrentPlayerActionState = playerActionState;
    }

    public bool InGroundedState() {
        return IsStateGroundedState(CurrentPlayerMovementState);
    }

    public bool IsStateGroundedState(PlayerMovementState movementState) {
        return movementState == PlayerMovementState.Idling
            || movementState == PlayerMovementState.Walking
            || movementState == PlayerMovementState.Running
            || movementState == PlayerMovementState.Sprinting;
    }

}
public enum PlayerMovementState {
    Idling = 0,
    Walking = 1,
    Running = 2,
    Sprinting = 3,
    Jumping = 4,
    Falling = 5,
    Strafing = 6,
}

public enum PlayerActionState {
    Idling = 0,
    ChargingBow = 1,
    ReleasingBow = 2,
}