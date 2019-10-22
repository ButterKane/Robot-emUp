using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("References")]
    public EnemyBehaviour behaviourScript;

    [Space(2)]
    [Header("Variables")]
    public float buildUpPounceTime = 1.5f;
    public float pounceRecoveryTime = 1.5f;


    public IEnumerator JumpAttack(Transform target)
    {
        behaviourScript.state = EnemyState.Attacking;

        behaviourScript.animator.SetTrigger("PrepareAttack");

        yield return new WaitForSeconds(buildUpPounceTime);

        behaviourScript.animator.SetTrigger("Pounce");
        behaviourScript.rb.AddForce(Vector3.up * 5, ForceMode.Impulse);
        behaviourScript.rb.AddForce((target.position - transform.position).normalized * 10, ForceMode.Impulse);

        ResetAttackGlobals();

        yield return new WaitForSeconds(pounceRecoveryTime);

        behaviourScript.state = EnemyState.Idle;
        behaviourScript.isAttacking = false;
    }

    public void ResetAttackGlobals()
    {
        GameManager.i.enemyManager.enemyCurrentlyAttacking = null;
    }
}
