using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBoss : EnemyBehaviour
{
    [Space(2)]
    [Separator("Boss Variables")]
    public Renderer[] renderers;
    public Color normalColor = Color.blue;
    public Color attackingColor = Color.red;
   

    [Space(2)]
    [Header("Attack")]
    public Vector2 minMaxAttackSpeed = new Vector2(7,15);
    public AnimationCurve attackSpeedVariation;
    public float maxRotationSpeed = 20; // How many angle it can rotates in one second
    public float BumpOtherDistanceMod = 0.5f;
    public float BumpOtherDurationMod = 0.2f;
    public float BumpOtherRestDurationMod = 0.3f;
   
    // ATTACK
    public override void EnterPreparingAttackState()
    {
        currentSpeed = navMeshAgent.speed;
        acceleration = navMeshAgent.acceleration;
        anticipationTime = maxAnticipationTime;
        animator.SetTrigger("AttackTrigger");

        navMeshAgent.enabled = false;
    }

    public override void PreparingAttackState()
    {
        base.PreparingAttackState();
        foreach (var renderer in renderers)
        {
            renderer.material.SetColor("_Color", Color.Lerp(attackingColor, normalColor , anticipationTime));
        }
    }


    public override void EnterAttackingState(string attackSound = "EnemyAttack")
    {
        attackSound = "EnemyShieldAttack";
        base.EnterAttackingState();
    }

    public override void AttackingState()
    {
        if (!navMeshAgent.enabled)
        {
            navMeshAgent.enabled = true;
        }

        attackTimeProgression += Time.deltaTime / maxAttackDuration;

        //must stop ?
        int internal_attackRaycastMask = 1 << LayerMask.NameToLayer("Environment");
        if (Physics.Raycast(transform.position, transform.forward, attackRaycastDistance, internal_attackRaycastMask) && !mustCancelAttack)
        {
            attackTimeProgression = whenToTriggerEndOfAttackAnim;
            mustCancelAttack = true;
        }

        if (!mustCancelAttack)
        {
            navMeshAgent.speed = Mathf.Lerp(minMaxAttackSpeed.x, minMaxAttackSpeed.y, attackSpeedVariation.Evaluate(attackTimeProgression));
            navMeshAgent.angularSpeed = maxRotationSpeed;
            navMeshAgent.acceleration = 100f;

            Vector3 internal_direction = Vector3.Lerp(transform.forward, focusedPlayer.position - transform.position, (maxRotationSpeed/360) *Time.deltaTime );

            Debug.DrawRay(transform.position + internal_direction * 5, Vector3.up, Color.green, 2f);
            navMeshAgent.SetDestination(transform.position + internal_direction * 5);
        }

        if (attackTimeProgression >= 1)
        {
            navMeshAgent.speed = currentSpeed;
            navMeshAgent.acceleration = acceleration;
            ChangeState(EnemyState.PauseAfterAttack);
            animator.SetTrigger("EndOfAttackTrigger");

            navMeshAgent.enabled = false;
        }
        /*else if (attackTimeProgression >= whenToTriggerEndOfAttackAnim && !endOfAttackTriggerLaunched)
        {
            endOfAttackTriggerLaunched = true;
            Animator.SetTrigger("EndOfAttackTrigger");
        }*/
        else if (attackTimeProgression >= whenToTriggerEndOfAttackAnim)
        {
            float internal_rationalizedProgression = (1 - attackTimeProgression) / (1 - whenToTriggerEndOfAttackAnim);
            foreach (var renderer in renderers)
            {
                renderer.material.SetColor("_Color", Color.Lerp(normalColor,  attackingColor, internal_rationalizedProgression)); // Time prgression isn't good
            }
        }
    }

    // Ususally called when hitting player
    public void StopAttack()
    {
        navMeshAgent.SetDestination(transform.position);
        attackTimeProgression = whenToTriggerEndOfAttackAnim;
        mustCancelAttack = true;
        animator.SetTrigger("AttackTouchedTrigger");

        navMeshAgent.enabled = false;
    }

    // BUMPED
    public override void EnterBumpedState()
    {
        foreach (var renderer in renderers)
        {
            renderer.material.SetColor("_Color", normalColor);
        }
        base.EnterBumpedState();
    }

    public override void ExitBumpedState()
    {
    }
    

}
