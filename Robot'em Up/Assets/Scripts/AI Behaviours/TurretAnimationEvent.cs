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
}
