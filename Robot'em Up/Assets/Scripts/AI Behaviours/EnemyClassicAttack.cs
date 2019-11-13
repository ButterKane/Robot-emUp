using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyClassicAttack : EnemyAttack
{
    [Separator("Local Variables")]

    public float PreparationTimeBeforeAttack = 0.5f;
    public float ChargeDuration = 1f;   // i want  the charge to last x seconds
    public float ChargeDistance = 10f;  // and during those x seconds i want the enemy to travel y meters
    public AnimationCurve ChargeForceVariation;
    
    public override IEnumerator Attack(Transform target)
    {
        _behaviourScript.State = EnemyState.Attacking;
        transform.LookAt(SwissArmyKnife.GetFlattedDownPosition(target.position, transform.position));

        yield return null;

        hitSomething = false;
        Vector3 startPos = transform.position;

        _behaviourScript.State = EnemyState.Attacking;
        Vector3 attackDirection = SwissArmyKnife.GetFlattedDownDirection((target.position - transform.position).normalized);

        yield return new WaitForSeconds(PreparationTimeBeforeAttack);  // wait the given time 

        _behaviourScript.Animator.SetTrigger("AttackTrigger");
        
        yield return new WaitForSeconds(waitTime);  // wait for the animation to end
        
        float t = 0;
        while(t < ChargeDuration)
        {
            // Must stop if hitting something, a wall of a player
            if (hitSomething || (startPos-transform.position).magnitude >= ChargeDistance)
            {
                t = ChargeDuration;
            }
            _behaviourScript.Rb.AddForce(attackDirection * (ChargeDistance/ChargeDuration) * ChargeForceVariation.Evaluate(t), ForceMode.VelocityChange);
            t += Time.deltaTime;
            
            transform.LookAt(transform.position + attackDirection);
            Debug.DrawRay(transform.position, attackDirection, Color.green);
            yield return null;
        }

        ResetAttackGlobals();

        yield return new WaitForSeconds(PounceRecoveryTime);

        _behaviourScript.WhatShouldIDo(EnemyState.Idle);
    }
}
