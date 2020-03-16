using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkBehaviourToAnimator : MonoBehaviour
{
    EnemyBehaviour enemyScriptRef;
	OldEnemyBoss BossScriptRef;
    // Start is called before the first frame update
    void Start()
    {
        enemyScriptRef = GetComponentInParent<EnemyBehaviour>();
        BossScriptRef = GetComponentInParent<OldEnemyBoss>();
    }


    public void ActivateMeleeHitBox()
    {
        if (enemyScriptRef.GetType() == typeof(EnemyMelee))
            enemyScriptRef.GetComponent<EnemyMelee>().ActivateAttackHitBox();
    }
    public void DestroyMeleeHitBox()
    {
        if (enemyScriptRef.GetType() == typeof(EnemyMelee))
            enemyScriptRef.GetComponent<EnemyMelee>().DestroySpawnedAttackUtilities();
    }


    public void Boss_ActivateMeleeHitBox()
    {
        BossScriptRef.ActivateAttackHitBox();
    }
    public void Boss_DestroyMeleeHitBox()
    {
        BossScriptRef.DestroyAttackHitBox();
    }


    public void Boss_ActivateHammerHitBox()
    {
        BossScriptRef.ActivateHammerAttackHitBox();
    }
    public void Boss_DestroyHammerHitBox()
    {
        BossScriptRef.DestroyAttackHitBox();
    }


    public void Boss_LaunchMissile()
    {
        BossScriptRef.LaunchMissiles();
    }
    public void Boss_InvokeShield()
    {
        BossScriptRef.InvokeShield();
    }



    public void EndAttack()
    {
        enemyScriptRef.ChangeState(EnemyState.PauseAfterAttack);
    }

    public void Die()
    {
        enemyScriptRef.Kill();
    }

}
