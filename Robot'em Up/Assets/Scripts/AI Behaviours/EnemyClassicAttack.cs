using MyBox;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyClassicAttack : EnemyAttack
{
    [Separator("Local Variables")]

    public float preparationTimeBeforeAttack = 0.5f;
    public float chargeDuration = 1f;   // i want  the charge to last x seconds
    public float chargeDistance = 10f;  // and during those x seconds i want the enemy to travel y meters
    public AnimationCurve chargeForceVariation;
    
    public override IEnumerator Attack_C(Transform _target)
    {
        transform.LookAt(SwissArmyKnife.GetFlattedDownPosition(_target.position, transform.position));

        yield return null;

        hitSomething = false;
        Vector3 internal_startPos = transform.position;

        _behaviourScript.EnemyState = EnemyState.Attacking;
        Vector3 internal_attackDirection = SwissArmyKnife.GetFlattedDownDirection((_target.position - transform.position).normalized);

        yield return new WaitForSeconds(preparationTimeBeforeAttack);  // wait the given time 

        _behaviourScript.animator.SetTrigger("AttackTrigger");
        float waitTime = _behaviourScript.animator.runtimeAnimatorController.animationClips.First(x => x.name == "Anticipation").length;
        yield return new WaitForSeconds(waitTime);  // wait for the animation to end
        
        float t = 0;
        while(t < chargeDuration)
        {
            // Must stop if hitting something, a wall of a player
            if (hitSomething || (internal_startPos-transform.position).magnitude >= chargeDistance)
            {
                t = chargeDuration;
            }
            _behaviourScript.rb.AddForce(internal_attackDirection * (chargeDistance/chargeDuration) * chargeForceVariation.Evaluate(t), ForceMode.VelocityChange);
            t += Time.deltaTime;
            
            transform.LookAt(transform.position + internal_attackDirection);
            Debug.DrawRay(transform.position, internal_attackDirection, Color.green);
            yield return null;
        }
        _behaviourScript.animator.SetTrigger("EndOfAttackTrigger");

        ResetAttackGlobals();

        _behaviourScript.rb.velocity = Vector3.zero;
        //_behaviourScript.IsAttacking = false;

        yield return new WaitForSeconds(PounceRecoveryTime);

    }
}
