using Riptide;
using UnityEngine;
using static PlayerLocomotionInput;

public class PlayerInputState : PlayerLocomotionState
{
    public Quaternion CameraRotation { get; set; }
    public Quaternion PlayerRotation { get; set; }

    public Message AppendToMessage(Message message)
    {
        message.AddVector2(MovementInput);
        message.AddVector2(LookInput);
        message.AddQuaternion(CameraRotation);
        message.AddQuaternion(PlayerRotation);
        message.AddBool(SprintToggledOn);
        message.AddBool(JumpPressed);
        message.AddBool(WalkToggledOn);
        return message;
    }

    public static PlayerInputState CreateFromMessage(Message message)
    {
        var playerInputState = new PlayerInputState();
        playerInputState.MovementInput = message.GetVector2();
        playerInputState.LookInput = message.GetVector2();
        playerInputState.CameraRotation = message.GetQuaternion();
        playerInputState.PlayerRotation = message.GetQuaternion();
        playerInputState.SprintToggledOn = message.GetBool();
        playerInputState.JumpPressed = message.GetBool();
        playerInputState.WalkToggledOn = message.GetBool();
        return playerInputState;
    }
}