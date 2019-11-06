using MyBox;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyClassicAttack : EnemyAttack
{
    [Separator("Local Variables")]

    public float ChargeTimeBeforeAttack = 0.5f;
    public float ChargeForce = 2f;
    public float ChargeTimeLength = 1f;
    
    public override IEnumerator Attack(Transform target)
    {
        hitSomething = false; 

        _behaviourScript.State = EnemyState.Attacking;
        Vector3 attackDirection = SwissArmyKnife.GetFlattedDownDirection((target.position - transform.position).normalized);

        yield return new WaitForSeconds(ChargeTimeBeforeAttack);  // wait the given time 

        _behaviourScript.Animator.SetTrigger("AttackTrigger");

        float waitTime = _behaviourScript.Animator.runtimeAnimatorController.animationClips.First(x => x.name == "Anticipation").length;    

        yield return new WaitForSeconds(waitTime);  // wait for the animation to end

        Debug.Log("Wesh");
        float t = 0;
        while(t < ChargeTimeLength)
        {
            // Must stop if hitting something, a wall of a player
            if (hitSomething)
            {
                Debug.Log("hit something");

                t = ChargeTimeLength;
            }
            _behaviourScript.Rb.AddForce(attackDirection * ChargeForce, ForceMode.VelocityChange);
            t += Time.deltaTime;
            yield return null;
        }
        Debug.Log("end of attack1");

        _behaviourScript.Animator.SetTrigger("EndOfAttackTrigger");

        ResetAttackGlobals();

        _behaviourScript.Rb.velocity = Vector3.zero;

        _behaviourScript.IsAttacking = false;

        yield return new WaitForSeconds(PounceRecoveryTime);

        _behaviourScript.WhatShouldIDo();
    }

}
