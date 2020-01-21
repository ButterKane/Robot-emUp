using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkBehaviourToAnimator : MonoBehaviour
{
    EnemyBehaviour enemyScriptRef;
    EnemyBoss BossScriptRef;
    // Start is called before the first frame update
    void Start()
    {
        enemyScriptRef = GetComponentInParent<EnemyBehaviour>();
        BossScriptRef = GetComponentInParent<EnemyBoss>();
    }


    public void ActivateMeleeHitBox()
    {
        enemyScriptRef.ActivateAttackHitBox();
    }
    public void DestroyMeleeHitBox()
    {
        enemyScriptRef.DestroyAttackHitBox();
    }


    public void Boss_ActivateMeleeHitBox()
    {
        BossScriptRef.ActivateAttackHitBox();
    }
    public void Boss_DestroyMeleeHitBox()
    {
        BossScriptRef.DestroyAttackHitBox();
    }



    public void EndAttack()
    {
        enemyScriptRef.ChangeState(EnemyState.PauseAfterAttack);
    }

}
