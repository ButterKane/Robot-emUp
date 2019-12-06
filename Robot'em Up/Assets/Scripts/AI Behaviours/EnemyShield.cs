﻿using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShield : EnemyBehaviour
{
    [Space(2)]
    [Separator("Shield Variables")]
    public GameObject ShieldPrefab;
    [System.NonSerialized] public GameObject Shield;
    public bool deactivateShieldWhenAttacking = true;

    // The "field of view" angle of the shield. If incident angle of ball is within this, ball will rebound
    public float angleRangeForRebound {
        get { return angleRangeForRebound; }
        set { angleRangeForRebound = value; Shield.GetComponent<Shield>().angleRangeForRebound = value; }
    }  

    public bool IsShieldActivated {
        get { return isShieldActivated; }
        set
        {
            isShieldActivated = value;
            Shield.SetActive(value);
        }
    }
    bool isShieldActivated;

    [Space(2)]
    [Header("Attack")]
    public float attackSpeed = 7;
    public float maxRotationSpeed = 20; // How many angle it can rotates in one second
    public float BumpOtherDistanceMod = 0.5f;
    public float BumpOtherDurationMod = 0.2f;
    public float BumpOtherRestDurationMod = 0.3f;

    private new void Start()
    {
        base.Start();
        Shield = Instantiate(ShieldPrefab);
        Shield.GetComponent<Shield>().Enemy = this;
    }

    // ATTACK
    public override void EnterPreparingAttackState()
    {
        ActualSpeed = navMeshAgent.speed;
        NormalAcceleration = navMeshAgent.acceleration;
        anticipationTime = maxAnticipationTime;
        Animator.SetTrigger("AttackTrigger");
    }   

    public override void AttackingState()
    {
        if (deactivateShieldWhenAttacking)
        {
            IsShieldActivated = false;
        }

        attackTimeProgression += Time.deltaTime / maxAttackDuration;

        //must stop ?
        int attackRaycastMask = 1 << LayerMask.NameToLayer("Environment");
        if (Physics.Raycast(_self.position, _self.forward, attackRaycastDistance, attackRaycastMask) && !mustCancelAttack)
        {
            attackTimeProgression = whenToTriggerEndOfAttackAnim;
            mustCancelAttack = true;
        }

        if (!mustCancelAttack)
        {
            navMeshAgent.speed = attackSpeed;
            navMeshAgent.angularSpeed = maxRotationSpeed;
            navMeshAgent.acceleration = 100f;

            Vector3 direction = Vector3.Lerp(_self.forward, focusedPlayer.position - _self.position, (maxRotationSpeed/360) *Time.deltaTime );

            Debug.DrawRay(_self.position + direction * 5, Vector3.up, Color.green, 2f);
            navMeshAgent.SetDestination(_self.position + direction * 5);
        }

        if (attackTimeProgression >= 1)
        {
            navMeshAgent.speed = ActualSpeed;
            navMeshAgent.acceleration = NormalAcceleration;
            IsShieldActivated = true;
            ChangingState(EnemyState.PauseAfterAttack);
        }
        else if (attackTimeProgression >= whenToTriggerEndOfAttackAnim && !endOfAttackTriggerLaunched)
        {
            endOfAttackTriggerLaunched = true;
            Animator.SetTrigger("EndOfAttackTrigger");
        }
    }

    // Ususally called when hitting player
    public void StopAttack()
    {
        navMeshAgent.SetDestination(_self.position);
        attackTimeProgression = whenToTriggerEndOfAttackAnim;
        mustCancelAttack = true;
        Animator.SetTrigger("AttackTouchedTrigger");
    }

    // BUMPED
    public override void EnterBumpedState()
    {
        IsShieldActivated = false;
        base.EnterBumpedState();
    }

    public override void ExitBumpedState()
    {
        IsShieldActivated = true;
    }
}
