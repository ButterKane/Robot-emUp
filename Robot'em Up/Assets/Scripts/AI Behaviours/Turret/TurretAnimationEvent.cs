using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAnimationEvent : MonoBehaviour
{
    public TurretBehaviour MyScript;

    void FinishedPrepareAttack()
    {
        MyScript.ChangingState(TurretState.Attacking);
    }
    void FinishedHiding()
    {
        MyScript.ChangingState(TurretState.Hidden);
    }
    void Attack()
    {
        MyScript.LaunchProjectile();
    }

    void AimingCubeRotateTrue()
    {
        MyScript.AimingCubeRotate(true);
    }

    void AimingCubeRotateFalse()
    {
        MyScript.AimingCubeRotate(false);
    }
}
