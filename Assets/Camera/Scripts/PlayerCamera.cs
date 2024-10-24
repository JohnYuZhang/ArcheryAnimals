using UnityEngine;
using Unity.Cinemachine;

public class PlayerCamera : MonoBehaviour
{
    private static CinemachineCamera playerCam;

    void Awake() {
        // Cache the virtual camera component
        playerCam = GetComponent<CinemachineCamera>();
    }

    public static CinemachineCamera GetPlayerCamera() {
        return playerCam;
    }
}
