using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAnimationEvent : MonoBehaviour
{
    public TurretBehaviour MyScript;
    bool canCallFollowingState = true;

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
        if (canCallFollowingState)
        {
            MyScript.ChangeAimingCubeState(AimingCubeState.Following);
            canCallFollowingState = false;
        }
    }

    void AimingCubeRotateFalse()
    {
        MyScript.ChangeAimingCubeState(AimingCubeState.Locking);
    }

    void GoToRestAttackState()
    {
        if(MyScript.attackState == TurretAttackState.Attack)
        {
            MyScript.TransitionFromAttackToRest();
            canCallFollowingState = true;
        }
    }
}
