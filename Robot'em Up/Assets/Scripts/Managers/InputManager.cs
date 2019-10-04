using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [HideInInspector] public static InputManager i;

    [Header("Settings")]
    public bool inputDisabled;
    public float minJoystickStrength = 0.1f;

    Transform mainCamTransform;


    private void Start()
    {
        if (i == null)
        {
            i = this;
        }
        else
        {
            Debug.LogError("There is already an InputManager");
        }

        InitializeRefs();
    }

    public void InitializeRefs()
    {
        mainCamTransform = GameManager.i.mainCameraGO.transform;
    }

    public Vector3 GetMoveAbsoluteDirection(float xMove, float zMove)
    {
        Vector3 moveDirection;

        moveDirection = new Vector3(xMove, 0, zMove).normalized;

        return moveDirection;
    }

    public Vector3 GetMoveAsViewedWithCamera(float xMove, float zMove)
    {
        Vector3 unrotatedMoveDirection = GetMoveAbsoluteDirection(xMove, zMove);

        // Flat down camera orientation. Only take the components that are parrallel to the ground 
        Vector3 flattedCameraRight = new Vector3 (mainCamTransform.right.x,0 ,mainCamTransform.right.z);
        Vector3 flattedCameraForward = new Vector3(mainCamTransform.forward.x, 0, mainCamTransform.forward.z);

        Vector3 rotatedMoveDirection = (flattedCameraRight * unrotatedMoveDirection.x  + flattedCameraForward * unrotatedMoveDirection.z).normalized;

        return rotatedMoveDirection;
    }

    
}
