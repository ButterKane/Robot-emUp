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

    public Vector3 GetMoveAbsoluteDirection(float _xMove, float _zMove)
    {
        Vector3 internal_moveDirection;

        internal_moveDirection = new Vector3(_xMove, 0, _zMove).normalized;

        return internal_moveDirection;
    }

    public Vector3 GetMoveAsViewedWithCamera(float _xMove, float _zMove)
    {
        Vector3 internal_unrotatedMoveDirection = GetMoveAbsoluteDirection(_xMove, _zMove);

        // Flat down camera orientation. Only take the components that are parrallel to the ground 
        Vector3 internal_flattedCameraRight = new Vector3 (mainCamTransform.right.x,0 ,mainCamTransform.right.z);
        Vector3 internal_flattedCameraForward = new Vector3(mainCamTransform.forward.x, 0, mainCamTransform.forward.z);

        Vector3 internal_rotatedMoveDirection = (internal_flattedCameraRight * internal_unrotatedMoveDirection.x  + internal_flattedCameraForward * internal_unrotatedMoveDirection.z).normalized;

        return internal_rotatedMoveDirection;
    }

    
}
