using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager i;

    [Separator("Settings")]
    public bool inputDisabled;
    public float minJoystickStrength = 0.1f;

    // Auto-Assigned References
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
        mainCamTransform = GameManager.mainCamera.transform;
    }

    public Vector3 GetMoveAbsoluteDirection(float _xMove, float _zMove)
    {
        Vector3 i_moveDirection;

        i_moveDirection = new Vector3(_xMove, 0, _zMove).normalized;

        return i_moveDirection;
    }

    public Vector3 GetMoveAsViewedWithCamera(float _xMove, float _zMove)
    {
        Vector3 i_unrotatedMoveDirection = GetMoveAbsoluteDirection(_xMove, _zMove);

        // Flat down camera orientation. Only take the components that are parrallel to the ground 
        Vector3 i_flattedCameraRight = new Vector3 (mainCamTransform.right.x,0 ,mainCamTransform.right.z);
        Vector3 i_flattedCameraForward = new Vector3(mainCamTransform.forward.x, 0, mainCamTransform.forward.z);

        Vector3 i_rotatedMoveDirection = (i_flattedCameraRight * i_unrotatedMoveDirection.x  + i_flattedCameraForward * i_unrotatedMoveDirection.z).normalized;

        return i_rotatedMoveDirection;
    }

    
}
