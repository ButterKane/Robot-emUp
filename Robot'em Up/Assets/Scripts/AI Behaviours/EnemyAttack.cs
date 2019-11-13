﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected EnemyBehaviour _behaviourScript;

    [Space(2)]
    [Header("Variables")]
    public float BuildUpPounceTime = 1.5f;
    public float PounceRecoveryTime = 1.5f;
    public bool hitSomething;

    public void LaunchAttack(Transform target)
    {
        StartCoroutine(Attack(target));
    }
    
    public virtual IEnumerator Attack(Transform target)
    {
        _behaviourScript.State = EnemyState.Attacking;

        _behaviourScript.Animator.SetTrigger("PrepareAttack");

        yield return new WaitForSeconds(BuildUpPounceTime);

        _behaviourScript.Animator.SetTrigger("Pounce");
        _behaviourScript.Rb.AddForce(Vector3.up * 5, ForceMode.Impulse);
        _behaviourScript.Rb.AddForce((target.position - transform.position).normalized * 10, ForceMode.Impulse);

        ResetAttackGlobals();

        yield return new WaitForSeconds(PounceRecoveryTime);

        _behaviourScript.WhatShouldIDo();
    }

    public void ResetAttackGlobals()
    {
        GameManager.i.enemyManager.enemyCurrentlyAttacking = null;
    }
}
