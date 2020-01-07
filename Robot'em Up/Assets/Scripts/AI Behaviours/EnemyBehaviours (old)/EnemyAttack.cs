using MyBox;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Separator("Parent Class Variables")]
    [SerializeField] protected EnemyBehaviour _behaviourScript;

    [Space(2)]
    [Header("Variables")]
    public float BuildUpPounceTime = 1.5f;
    public float PounceRecoveryTime = 1.5f;
    public bool hitSomething;

    [Space(2)]
    [Header("AttackJump Variables")]
    public float AttackUpForce = 10f;
    public float AttackForwardForce = 30f;

    public void LaunchAttack(Transform target)
    {
        StartCoroutine(Attack_C(target));
    }

    public virtual IEnumerator Attack_C(Transform target)
    {
        _behaviourScript.EnemyState = EnemyState.Attacking;

        _behaviourScript.animator.SetTrigger("AttackTrigger");

        float waitTime = _behaviourScript.animator.runtimeAnimatorController.animationClips.First(x => x.name == "Anticipation").length;

        yield return new WaitForSeconds(waitTime);
        
        _behaviourScript.rb.AddForce(Vector3.up * AttackUpForce, ForceMode.Impulse);
        _behaviourScript.rb.AddForce((target.position - transform.position).normalized * AttackForwardForce, ForceMode.Impulse);

        ResetAttackGlobals();

        // Still has to set the moment the animation of end attack plays

        yield return new WaitForSeconds(PounceRecoveryTime);

        //_behaviourScript.IsAttacking = false;

    }

    public void ResetAttackGlobals()
    {
        GameManager.i.enemyManager.enemyCurrentlyAttacking = null;
    }
}
