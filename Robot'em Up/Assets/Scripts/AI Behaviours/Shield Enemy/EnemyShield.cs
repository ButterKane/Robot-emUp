using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShield : EnemyBehaviour
{
    [Space(2)]
    [Separator("Shield Variables")]
    public GameObject shield;       // It's only cosmetic now
    public bool deactivateShieldWhenAttacking = true;
    // The "field of view" angle of the shield. If incident angle of ball is within this, ball will rebound
    [Range(0, 90)] public float angleRangeForRebound = 45;
    public float timeShieldDisappearAfterHit;
    public bool isShieldActivated_accesss
    {
        get { return isShieldActivated; }
        set
        {
            isShieldActivated = value;
            shield.SetActive(value);
        }
    }
    bool isShieldActivated;

    [Space(2)]
    [Header("Aspect Variables")]
    public Renderer[] renderers;
    public Color normalColor = Color.blue;
    public Color attackingColor = Color.red;

    [Space(2)]
    [Header("Attack")]
    public RushAttackHitBox attackHitBox;
    public Vector2 minMaxAttackSpeed = new Vector2(7, 15);
    public AnimationCurve attackSpeedVariation;
    public float attackChargeDuration = 5f;
    [Range(0, 1)] public float whenToTriggerEndOfAttackAnim = 0.9f;    // At what % of the attack duration do we want to stop animation to trigger?
    public float attackRaycastDistance = 2;
    public float maxRotationSpeed = 20; // How many angle it can rotates in one second
    private float attackTimeProgression;
    private float initialSpeed;

    private new void Start()
    {
        base.Start();
        eventOnBeingHit = "event.EnemyShieldHit";
        eventOnDeath = "event.EnemyShieldDeath";
        enemyType = EnemyTypes.Shield;
    }

    // ATTACK
    public override void PreparingAttackState()
    {
        base.PreparingAttackState();
        foreach (var renderer in renderers)
        {
            renderer.material.SetColor("_Color", Color.Lerp(attackingColor, normalColor , anticipationTime));
        }
    }

    public override void EnterPreparingAttackState()
    {
        ChangePawnState("ShieldEnemyCharging", StartAttackState_C(), StopAttackState_C());
    }

    public IEnumerator StartAttackState_C()
    {
        Debug.Log("start attackshield");

        initialSpeed = navMeshAgent.speed;
        acceleration = navMeshAgent.acceleration;
        anticipationTime = maxAnticipationTime;
        animator.SetTrigger("AnticipateAttackTrigger");

        navMeshAgent.enabled = false;
        yield return null;
    }

    public IEnumerator StopAttackState_C()
    {
        Debug.Log("stop attackshield");
        attackTimeProgression = whenToTriggerEndOfAttackAnim;
        mustCancelAttack = true;
        attackHitBox.ToggleCollider(false);
        navMeshAgent.enabled = false;
        yield return null;
    }

    public override void EnterAttackingState(string attackSound = "EnemyAttack")
    {
        attackSound = "EnemyShieldAttack";
        animator.ResetTrigger("EndOfAttackTrigger");
        animator.ResetTrigger("AttackTouchedTrigger");
        attackHitBox.ToggleCollider(true);
        attackTimeProgression = 0;
        base.EnterAttackingState();
    }

    public override void AttackingState()
    {
        if (!navMeshAgent.enabled)
        {
            navMeshAgent.enabled = true;
        }

        if (deactivateShieldWhenAttacking)
        {
            isShieldActivated_accesss = false;
        }

        //attackTimeProgression += Time.deltaTime / maxAttackDuration;

        //must stop ?
        int i_attackRaycastMask = 1 << LayerMask.NameToLayer("Environment");
        if (Physics.Raycast(transform.position, transform.forward, attackRaycastDistance, i_attackRaycastMask) && !mustCancelAttack)
        {
            attackTimeProgression = whenToTriggerEndOfAttackAnim;
            mustCancelAttack = true;
        }

        if (!mustCancelAttack)
        {
            moveSpeed = Mathf.Lerp(minMaxAttackSpeed.x, minMaxAttackSpeed.y, attackSpeedVariation.Evaluate(attackTimeProgression/attackChargeDuration));
            navMeshAgent.angularSpeed = maxRotationSpeed;
            navMeshAgent.acceleration = 100f;
            Vector3 i_direction = Vector3.Lerp(transform.forward, focusedPawnController.transform.position - transform.position, (maxRotationSpeed/360) *Time.deltaTime );

            Debug.DrawRay(transform.position + i_direction * 5, Vector3.up, Color.green, 2f);
            navMeshAgent.SetDestination(transform.position + i_direction * 5);
        }

        if (attackTimeProgression >= attackChargeDuration)
        {
            moveSpeed = initialSpeed;
            navMeshAgent.acceleration = acceleration;
            isShieldActivated_accesss = true;
            ChangeState(EnemyState.PauseAfterAttack);
            animator.SetTrigger("EndOfAttackTrigger");
            
            navMeshAgent.enabled = false;
        }
        else if (attackTimeProgression >= whenToTriggerEndOfAttackAnim)
        {
            float i_rationalizedProgression = (1 - attackTimeProgression) / (1 - whenToTriggerEndOfAttackAnim);
            foreach (var renderer in renderers)
            {
                renderer.material.SetColor("_Color", Color.Lerp(normalColor,  attackingColor, i_rationalizedProgression)); // Time prgression isn't good
            }
        }

        attackTimeProgression += Time.deltaTime;
    }

    // Ususally called when hitting player
    public void StopAttack()
    {
        //navMeshAgent.SetDestination(transform.position);
        attackTimeProgression = whenToTriggerEndOfAttackAnim;
        mustCancelAttack = true;
        animator.SetTrigger("AttackTouchedTrigger");
        attackHitBox.ToggleCollider(false);
        navMeshAgent.enabled = false;
    }

    public override void DestroySpawnedAttackUtilities()
    {
        attackHitBox.ToggleCollider(false); // well, it's not destroying it, more like deactivating it. But that's the same point
    }

    // BUMPED
    public override void EnterBumpedState()
    {
        isShieldActivated_accesss = false;
        foreach (var renderer in renderers)
        {
            renderer.material.SetColor("_Color", normalColor);
        }
        base.EnterBumpedState();
    }

    public override void ExitBumpedState()
    {
        isShieldActivated_accesss = true;
    }

    public override void Kill()
    {
        base.Kill();   // Override the death sound with the right one 
    }

    public IEnumerator DeactivateShieldForGivenTime( float timeToDeactivate)
    {
        isShieldActivated_accesss = false;
        yield return new WaitForSeconds(timeToDeactivate);
        isShieldActivated_accesss = true;
    }

	public override void OnHit ( BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, float _damages, DamageSource _source, Vector3 _bumpModificators = default )
	{
        StartCoroutine(DeactivateShieldForGivenTime(timeShieldDisappearAfterHit));
        base.OnHit(_ball, _impactVector, _thrower, _damages, _source, _bumpModificators);
	}
}
