using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShieldAttack : EnemyAttack
{
    public override IEnumerator Attack(Transform target)
    {
        _behaviourScript.EnemyState = EnemyState.Attacking;

        _behaviourScript.Animator.SetTrigger("PrepareAttack");

        yield return new WaitForSeconds(BuildUpPounceTime);

        _behaviourScript.Animator.SetTrigger("Pounce");
        //_behaviourScript.Rb.AddForce(Vector3.up * 5, ForceMode.Impulse);
        //_behaviourScript.Rb.AddForce((target.position - transform.position).normalized * 10, ForceMode.Impulse);

        ResetAttackGlobals();

        yield return new WaitForSeconds(PounceRecoveryTime);

    }
}
