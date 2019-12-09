using MyBox;
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
    [Range(0,90)]
    public float AngleRangeForRebound;

    public float SpwaningShieldFrontDistance;

    public Renderer[] Renderers;
    public Color NormalColor = Color.blue;
    public Color AttackingColor = Color.red;

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
    public Vector2 minMaxAttackSpeed = new Vector2(7,15);
    public AnimationCurve attackSpeedVariation;
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

        navMeshAgent.enabled = false;
    }

    public override void PreparingAttackState()
    {
        base.PreparingAttackState();
        foreach (var renderer in Renderers)
        {
            renderer.material.SetColor("_Color", Color.Lerp(AttackingColor, NormalColor , anticipationTime));
        }
    }

    public override void AttackingState()
    {
        if (!navMeshAgent.enabled)
        {
            navMeshAgent.enabled = true;
        }

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
            navMeshAgent.speed = Mathf.Lerp(minMaxAttackSpeed.x, minMaxAttackSpeed.y, attackSpeedVariation.Evaluate(attackTimeProgression));
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

            navMeshAgent.enabled = false;
        }
        else if (attackTimeProgression >= whenToTriggerEndOfAttackAnim && !endOfAttackTriggerLaunched)
        {
            endOfAttackTriggerLaunched = true;
            Animator.SetTrigger("EndOfAttackTrigger");
        }
        else if (attackTimeProgression >= whenToTriggerEndOfAttackAnim)
        {
            float rationalizedProgression = (1 - attackTimeProgression) / (1 - whenToTriggerEndOfAttackAnim);
            Debug.Log("progression = " + rationalizedProgression);
            foreach (var renderer in Renderers)
            {
                renderer.material.SetColor("_Color", Color.Lerp(NormalColor,  AttackingColor, rationalizedProgression)); // Time prgression isn't good
            }
        }
    }

    // Ususally called when hitting player
    public void StopAttack()
    {
        navMeshAgent.SetDestination(_self.position);
        attackTimeProgression = whenToTriggerEndOfAttackAnim;
        mustCancelAttack = true;
        Animator.SetTrigger("AttackTouchedTrigger");

        navMeshAgent.enabled = false;
    }

    // BUMPED
    public override void EnterBumpedState()
    {
        IsShieldActivated = false;
        foreach (var renderer in Renderers)
        {
            renderer.material.SetColor("_Color", NormalColor);
        }
        base.EnterBumpedState();
    }

    public override void ExitBumpedState()
    {
        IsShieldActivated = true;
    }
    protected override void Die()
    {
        base.Die();
        Destroy(Shield);
    }

}
