using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkBehaviourToAnimator : MonoBehaviour
{
    EnemyBehaviour enemyScriptRef;
    // Start is called before the first frame update
    void Start()
    {
        enemyScriptRef = GetComponentInParent<EnemyBehaviour>();
    }


    public void ActivateMeleeHitBox()
    {
        enemyScriptRef.ActivateAttackHitBox();
    }
    public void DestroyMeleeHitBox()
    {
        enemyScriptRef.DestroyAttackHitBox();
    }

}
