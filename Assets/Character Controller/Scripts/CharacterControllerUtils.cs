using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CharacterControllerUtils
{
    // Calculates if feet hit really sloped wall
    public static Vector3 GetNormalWithSphereCast(CharacterController characterController, LayerMask layerMask = default) {
        Vector3 normal = Vector3.up;
        Vector3 center = characterController.transform.position + characterController.center;
        float distance = characterController.height / 2f + characterController.stepOffset + 0.01f;

        RaycastHit hit;
        if (Physics.SphereCast(center, characterController.radius, Vector3.down, out hit, distance, layerMask)) {
            normal = hit.normal;
        }

        return normal;
    }
}