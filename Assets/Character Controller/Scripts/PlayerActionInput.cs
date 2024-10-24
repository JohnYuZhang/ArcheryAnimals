using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Connection;
using FishNet.Object;
public class PlayerActionInput : NetworkBehaviour, PlayerControls.IPlayerActionMapActions
{

    #region Class Variables
    public PlayerControls PlayerControls { get; private set; }
    public bool DrawingBow { get; private set; }
    #endregion

    #region Startup
    public override void OnStartClient() {
        base.OnStartClient();
        if (base.IsOwner) {

        } else {
            gameObject.GetComponent<PlayerActionInput>().enabled = false;
        }
    }
    private void OnEnable() {
        PlayerControls = new PlayerControls();
        PlayerControls.Enable();

        PlayerControls.PlayerActionMap.Enable();
        PlayerControls.PlayerActionMap.SetCallbacks(this);

    }

    private void OnDisable() {
        PlayerControls.PlayerActionMap.Disable();
        PlayerControls.PlayerActionMap.RemoveCallbacks(this);
    }
    #endregion


    #region Input Callbacks

    public void OnDrawBow(InputAction.CallbackContext context) {
        if (context.performed) {
            DrawingBow = true;
        } else if (context.canceled) {
            // Why does Bowdrawn = false on initial click then true? how does context.canceled work. This is ok behavior because you want bow draw to default to false until pressed.
            DrawingBow = false;
        }
    }
    #endregion
}
