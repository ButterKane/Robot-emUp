﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAnimationEvent : MonoBehaviour
{
    public TurretBehaviour myScript;
    bool canCallFollowingState = true;

    private void Start()
    {
        if (myScript == null)
        {
            myScript = GetComponentInParent<TurretBehaviour>(); // supposedly, there is only one instance of TurretBehviour, in the top parent
        }
    }

    void FinishedPrepareAttack()
    {
        myScript.ChangingState(TurretState.Attacking);
    }
    void FinishedHiding()
    {
        myScript.ChangingState(TurretState.Hidden);
    }
    void Attack()
    {
        myScript.LaunchProjectile();
    }

    void AimingCubeRotateTrue()
    {
        if (canCallFollowingState)
        {
            myScript.ChangeAimingCubeState(AimingCubeState.Following);
            canCallFollowingState = false;
        }
    }

    void AimingCubeRotateFalse()
    {
        myScript.ChangeAimingCubeState(AimingCubeState.Locking);
    }

    void GoToRestAttackState()
    {
        if(myScript.attackState == TurretAttackState.Attack)
        {
            myScript.TransitionFromAttackToRest();
            canCallFollowingState = true;
        }
    }
}
