﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using UnityEngine.Events;
using UnityEngine.AI;
using UnityEngine.UI;

public enum EnemyState
{
    WaitForCombatStart,
    Idle,
    Following,
    Bumped,
    ChangingFocus,
    PreparingAttack,
    Attacking,
    PauseAfterAttack,
    Dying,
}
public enum WhatBumps
{
    Pass,
    Dunk,
    RedBarrel,
    Environment
}
public enum EnemyTypes
{
    Melee,
    Shield,
    RedBarrel,
    Turret,
    Sniper
}

public class EnemyBehaviour : PawnController, IHitable
{
    [System.NonSerialized] public EnemyState enemyState = EnemyState.Idle;

    [Separator("References")]
    public Transform healthBarRef;
    public GameObject healthBarPrefab;

    [Space(2)]
    [Separator("Auto-assigned References")]
    [SerializeField] protected Transform playerOneTransform;
    protected PawnController playerOnePawnController;
    [SerializeField] protected Transform playerTwoTransform;
    protected PawnController playerTwoPawnController;

    [Space(2)]
    [Separator("Tweakable variables")]
    public EnemyTypes enemyType = EnemyTypes.Melee;
    protected bool playerOneInRange;
    protected bool playerTwoInRange;
    protected float distanceWithPlayerOne;
    protected float distanceWithPlayerTwo;
    protected float distanceWithFocusedPlayer;
    [System.NonSerialized] public Transform focusedPlayer = null;
    public float energyGainedOnHit = 1;
    public int damage = 10;
    public float powerLevel = 1;
    [SerializeField] protected bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
    [SerializeField] protected float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }
    public bool arenaRobot;

    [Space(3)]
    [Header("Focus")]
    public float focusDistance = 3;
    public float unfocusDistance = 20;
    public float timeBetweenCheck = 0;
    public float distanceBeforeChangingPriority = 3;
    public float maxTimeBetweenCheck = 0.25f;

    [Space(3)]
    [Header("Movement")]
    public float randomSpeedMod;
    public float speedMultiplierFromPassHit;
    public float timeToRecoverSlowFromPass;
    public float speedMultiplierFromDunkHit;
    public float timeToRecoverSlowFromDunk;
    private WhatBumps whatBumps;

    [Space(3)]
    [Header("Common attack variables")]
    public float maxAnticipationTime = 0.5f;
    public float maxTimePauseAfterAttack = 1;
    public Vector3 hitBoxOffset;

    [Header("Melee Variables")]
    [ConditionalField(nameof(enemyType), false, EnemyTypes.Melee)] public float distanceToAttack = 5;
    [ConditionalField(nameof(enemyType), false, EnemyTypes.Melee)] [Range(0, 1)] public float rotationSpeedPreparingAttack = 0.2f;
    [ConditionalField(nameof(enemyType), false, EnemyTypes.Melee)] [Range(0, 1)] public float whenToTriggerEndOfAttackAnim;
    [ConditionalField(nameof(enemyType), false, EnemyTypes.Melee)] public GameObject attackHitBoxPrefab;
    [ConditionalField(nameof(enemyType), false, EnemyTypes.Melee)] public Transform attackHitBoxCenterPoint;
    [ConditionalField(nameof(enemyType), false, EnemyTypes.Melee)] public float attackRaycastDistance = 2;
    [ConditionalField(nameof(enemyType), false, EnemyTypes.Melee)] [Range(0, 1)] public float portionOfAnticipationWithRotation = 0.3f;
    [ConditionalField(nameof(enemyType), false, EnemyTypes.Melee)] [Range(0, 1)] public float portionOfAnticipationWithFlickering = 0.2f;

    private MeshRenderer attackPreviewPlaneRenderer;
    protected float anticipationTime;
    protected float attackDuration;
    protected bool endOfAttackTriggerLaunched;
    private GameObject attackHitBoxInstance;
    private GameObject attackPreviewPlane;
    GameObject myAttackHitBox;
    
    float timePauseAfterAttack;
    protected bool mustCancelAttack;
    private EnemyArmAttack armScript;
    
    //[ConditionalField(nameof(enemyType), false, EnemyTypes.Turret)]
    //[ConditionalField(nameof(enemyType), false, EnemyTypes.Sniper)]


    [Space(3)]
    [Header("Surrounding")]
    public bool canSurroundPlayer;
    [Range(0, 1)] public float bezierCurveHeight = 0.5f;
    public float bezierDistanceToHeightRatio = 100f;
    [System.NonSerialized] public Transform closestSurroundPoint;

    [Space(3)]
    [Header("Death")]
    public float coreDropChances = 1;
    public Vector2 minMaxDropForce;
    public Vector2 minMaxCoreHealthValue = new Vector2(1, 3);
    [System.NonSerialized] public UnityEvent onDeath = new UnityEvent();
    private HealthBar healthBar;

    protected void Start()
    {
        timeBetweenCheck = maxTimeBetweenCheck;
        playerOneTransform = GameManager.playerOne.transform;
        playerTwoTransform = GameManager.playerTwo.transform;
        playerOnePawnController = playerOneTransform.GetComponent<PlayerController>();
        playerTwoPawnController = playerTwoTransform.GetComponent<PlayerController>();
        GameManager.i.enemyManager.enemies.Add(this);
        if (canSurroundPlayer) { GameManager.i.enemyManager.enemiesThatSurround.Add(this); }
        armScript = GetComponentInChildren<EnemyArmAttack>();
        healthBar = Instantiate(healthBarPrefab, CanvasManager.i.mainCanvas.transform).GetComponent<HealthBar>();
        healthBar.target = this;

        if (arenaRobot)
        {
            ChangeState(EnemyState.WaitForCombatStart);
        }
        else
        {
            ChangeState(EnemyState.Idle);
        }
    }

    protected void Update()
    {
        UpdateDistancesToPlayers();
        UpdateState();
        UpdateSpeed();
        UpdateHealthBar();
    }
    protected void UpdateHealthBar()
    {
        if (currentHealth < maxHealth && healthBar != null)
        {
            healthBar.ToggleHealthBar(true);
        }
        else
        {
            healthBar.ToggleHealthBar(false);
        }
    }

    protected void UpdateSpeed()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = moveSpeed * GetSpeedCoef();
        }
    }

    public override void UpdateAnimatorBlendTree()
    {
        base.UpdateAnimatorBlendTree();
        if (canMove)
        {
            animator.SetFloat("IdleRunBlend", navMeshAgent.velocity.magnitude / navMeshAgent.speed);
        }
    }

    void UpdateState()
    {
        switch (enemyState)
        {
            case EnemyState.Idle:
                timeBetweenCheck -= Time.deltaTime;
                if (timeBetweenCheck <= 0)
                {
                    CheckDistanceAndAdaptFocus();
                    timeBetweenCheck = maxTimeBetweenCheck;
                }
                if (focusedPlayer != null)
                {
                    ChangeState(EnemyState.Following);
                }
                break;

            case EnemyState.Following:
                timeBetweenCheck -= Time.deltaTime;
                if (timeBetweenCheck <= 0)
                {
                    CheckDistanceAndAdaptFocus();
                    timeBetweenCheck = maxTimeBetweenCheck;
                }

                if (focusedPlayer != null)
                {
                    Quaternion _targetRotation = Quaternion.LookRotation(focusedPlayer.position - transform.position);
                    _targetRotation.eulerAngles = new Vector3(0, _targetRotation.eulerAngles.y, 0);
                    transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, rotationSpeedPreparingAttack);

                    if (closestSurroundPoint != null)
                    {
                        float i_distanceToPointRatio = (1 + (transform.position - closestSurroundPoint.position).magnitude / bezierDistanceToHeightRatio);  // widens the arc of surrounding the farther the surroundingPoint is

                        Vector3 i_p0 = transform.position;    // The starting point

                        Vector3 i_p2 = SwissArmyKnife.GetFlattedDownPosition(closestSurroundPoint.position, transform.position);  // The destination

                        float i_angle = Vector3.SignedAngle(i_p2 - i_p0, focusedPlayer.transform.position - i_p0, Vector3.up);

                        int i_moveSens = i_angle > 1 ? 1 : -1;

                        Vector3 i_p1 = i_p0 + (i_p2 - i_p0) / 0.5f + Vector3.Cross(i_p2 - i_p0, Vector3.up) * i_moveSens * bezierCurveHeight * i_distanceToPointRatio;  // "third point" of the bezier curve

                        // Calculating position on bezier curve, following start point, end point and avancement
                        // In this version, the avancement has been replaced by a constant because it's recalculated every frame
                        Vector3 i_positionOnBezierCurve = (Mathf.Pow(0.5f, 2) * i_p0) + (2 * 0.5f * 0.5f * i_p1) + (Mathf.Pow(0.5f, 2) * i_p2);
                        navMeshAgent.SetDestination(SwissArmyKnife.GetFlattedDownPosition(i_positionOnBezierCurve, focusedPlayer.position));
                    }
                    else
                    {
                        navMeshAgent.SetDestination(focusedPlayer.position);
                    }

                    if (distanceWithFocusedPlayer <= distanceToAttack)
                    {
                        ChangeState(EnemyState.PreparingAttack);
                    }
                }
                break;
            case EnemyState.ChangingFocus:
                break;

            case EnemyState.PreparingAttack:
                PreparingAttackState();
                break;

            case EnemyState.Attacking:
                AttackingState();
                break;

            case EnemyState.PauseAfterAttack:
                timePauseAfterAttack -= Time.deltaTime;
                if (timePauseAfterAttack <= 0)
                {
                    ChangeState(EnemyState.Idle);
                }
                break;

            case EnemyState.Dying:
                Kill();
                break;
        }
    }

    public void ChangeState(EnemyState _newState)
    {
        ExitState();
        EnterState(_newState);
        enemyState = _newState;
    }

    void EnterState(EnemyState _newState)
    {
        //print(_newState);
        switch (_newState)
        {
            case EnemyState.Idle:
                timeBetweenCheck = 0;
                //StartCoroutine(WaitABit(1));
                break;
            case EnemyState.Following:
                navMeshAgent.enabled = true;
                timeBetweenCheck = 0;
                break;
            case EnemyState.Bumped:
                EnterBumpedState();
                break;
            case EnemyState.ChangingFocus:
                break;
            case EnemyState.PreparingAttack:
                EnterPreparingAttackState();
                break;
            case EnemyState.Attacking:
                EnterAttackingState();
                break;
            case EnemyState.PauseAfterAttack:
                timePauseAfterAttack = maxTimePauseAfterAttack;
                break;
            case EnemyState.Dying:
                break;
        }
    }

    public virtual void EnterBumpedState()
    {
        navMeshAgent.enabled = false;
        animator.SetTrigger("BumpTrigger");
        mustCancelBump = false;
    }

    public virtual void EnterPreparingAttackState()
    {
        navMeshAgent.enabled = false;
        anticipationTime = maxAnticipationTime;
        animator.SetTrigger("AnticipateAttackTrigger");
        attackPreviewPlane = null;
    }

    public virtual void EnterAttackingState(string attackSound = "EnemyAttack")
    {
        endOfAttackTriggerLaunched = false;
        animator.SetTrigger("AttackTrigger");
        //myAttackHitBox = Instantiate(attackHitBoxPrefab, transform.position + hitBoxOffset.x * transform.right + hitBoxOffset.y * transform.up + hitBoxOffset.z * transform.forward, Quaternion.identity, transform);
        mustCancelAttack = false;
    }

    public virtual void PreparingAttackState()
    {
        if (attackHitBoxInstance == null && enemyType == EnemyTypes.Melee)
        {
            attackHitBoxInstance = Instantiate(attackHitBoxPrefab, new Vector3(attackHitBoxCenterPoint.position.x, attackHitBoxCenterPoint.position.y + attackHitBoxCenterPoint.localScale.y/2, attackHitBoxCenterPoint.position.z), Quaternion.identity, transform);
            attackHitBoxInstance.GetComponent<EnemyArmAttack>().attackDamage = damage;
            attackHitBoxInstance.GetComponent<EnemyArmAttack>().spawnParent = this;
            attackPreviewPlane = attackHitBoxInstance.GetComponent<EnemyArmAttack>().highlightPlane;
            attackPreviewPlaneRenderer = attackPreviewPlane.GetComponent<MeshRenderer>();
        }

        if (attackPreviewPlane != null)
        {
            // Make attack zone appear progressively
            if (anticipationTime > portionOfAnticipationWithFlickering * maxAnticipationTime)
            {
                attackPreviewPlane.transform.localScale = Vector3.one * (1 - ((anticipationTime - (portionOfAnticipationWithFlickering * maxAnticipationTime)) / (maxAnticipationTime - (maxAnticipationTime * portionOfAnticipationWithFlickering))));
            }
            // If max size is reached, flicker the color
            else 
            {
                attackPreviewPlaneRenderer.enabled = !attackPreviewPlaneRenderer.enabled;
            }
        }

        if (anticipationTime > portionOfAnticipationWithRotation * maxAnticipationTime)
        {
            Quaternion _targetRotation = Quaternion.LookRotation(focusedPlayer.position - transform.position);
            _targetRotation.eulerAngles = new Vector3(0, _targetRotation.eulerAngles.y, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, rotationSpeedPreparingAttack);
        }
        anticipationTime -= Time.deltaTime;

        if (anticipationTime <= 0)
        {
            if (attackPreviewPlane) { attackPreviewPlane.SetActive(false); } // making preview zone disappear
            ChangeState(EnemyState.Attacking);
        }
    }

    public virtual void AttackingState()
    {
        //ChangeState(EnemyState.PauseAfterAttack);
    }

    public void ActivateAttackHitBox()
    {
        if (attackHitBoxInstance != null)
        {
            attackHitBoxInstance.GetComponent<EnemyArmAttack>().ToggleArmCollider(true);
        }
    }
    public void DestroyAttackHitBox()
    {
        if (attackHitBoxInstance != null)
        {
            Destroy(attackHitBoxInstance);
            attackHitBoxInstance = null;
        }
    }

    void ExitState()
    {
        switch (enemyState)
        {
            case EnemyState.Idle:
                break;
            case EnemyState.Following:
                break;
            case EnemyState.Bumped:
                ExitBumpedState();
                break;
            case EnemyState.ChangingFocus:
                break;
            case EnemyState.PreparingAttack:
                break;
            case EnemyState.Attacking:
                navMeshAgent.enabled = true;
                Destroy(myAttackHitBox);
                break;
            case EnemyState.PauseAfterAttack:
                break;
            case EnemyState.Dying:
                break;
        }
    }

    public virtual void ExitBumpedState()
    {
        Staggered(whatBumps);
    }

    void UpdateDistancesToPlayers()
    {
        distanceWithPlayerOne = Vector3.Distance(transform.position, playerOneTransform.position);
        distanceWithPlayerTwo = Vector3.Distance(transform.position, playerTwoTransform.position);
        if (focusedPlayer != null)
            distanceWithFocusedPlayer = Vector3.Distance(transform.position, focusedPlayer.position);
    }

    Transform GetClosestAndAvailablePlayer()
    {
        if ((distanceWithPlayerOne >= distanceWithPlayerTwo && playerTwoPawnController.IsTargetable())
            || !playerOnePawnController.IsTargetable())
        {
            return playerTwoTransform;
        }
        else if ((distanceWithPlayerTwo >= distanceWithPlayerOne && playerOnePawnController.IsTargetable())
            || !playerTwoPawnController.IsTargetable())
        {
            return playerOneTransform;
        }
        else
        {
            return null;
        }
    }

    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source, Vector3 _bumpModificators = default(Vector3))
    {
        if (enemyType == EnemyTypes.Shield)
        {
            if (_ball != null) // Check if it's the ball that touched
            {
                EnemyShield i_selfRef = GetComponent<EnemyShield>();

                if ((_impactVector.normalized + transform.forward.normalized).magnitude < (i_selfRef.angleRangeForRebound / 63.5)) // This division makes it usable as a dot product
                {
                    FeedbackManager.SendFeedback("event.ShieldHitByBall", null);

                    Vector3 i_normalReboundDirection = Vector3.Reflect(_impactVector, transform.forward);

                    Vector3 i_newDirection = new Vector3 (i_normalReboundDirection.x, _impactVector.y, i_normalReboundDirection.z);

                    Vector3 i_directionToPlayerOne = playerOneTransform.position - i_selfRef.shield.transform.position;
                    Vector3 i_directionToPlayerTwo = playerTwoTransform.position - i_selfRef.shield.transform.position;

                    i_newDirection = SwissArmyKnife.GetClosestDirection(i_directionToPlayerOne, i_directionToPlayerTwo, i_newDirection, i_selfRef.angleToBounceBackToPlayer);

                    _ball.Bounce(i_newDirection, 1f);
                    
                    return;
                }
                else
                {
                    StartCoroutine(i_selfRef.DeactivateShieldForGivenTime(i_selfRef.timeShieldDisappearAfterHit));
                }
            }
        }

        Vector3 i_normalizedImpactVector;
        LockManager.UnlockTarget(this.transform);
        float i_BumpDistanceMod = 0.5f;
        float i_BumpDurationMod = 0.5f;
        float i_BumpRestDurationMod = 0.5f;

        switch (_source)
        {
            case DamageSource.Dunk:
                if (isBumpable)
                {
                    if (enemyType == EnemyTypes.RedBarrel)
                    {
                        EnemyRedBarrel i_selfRef = GetComponent<EnemyRedBarrel>();
                        Debug.Log("Touched with dunk avec " + i_selfRef);
                        i_selfRef.willExplode = false;
                    }

                    damageAfterBump = _damages;
					AnalyticsManager.IncrementData("DamageWithDunk", _damages);
					damageAfterBump = _damages;

                    i_normalizedImpactVector = new Vector3(_impactVector.x, 0, _impactVector.z);
                    if (_thrower.GetComponent<DunkController>() != null)
                    {
                        DunkController i_controller = _thrower.GetComponent<DunkController>();
                        i_BumpDistanceMod = i_controller.bumpDistanceMod;
                        i_BumpDurationMod = i_controller.bumpDurationMod;
                        i_BumpRestDurationMod = i_controller.bumpRestDurationMod;
                    }
                    BumpMe(10, 1, 1, i_normalizedImpactVector.normalized, i_BumpDistanceMod, i_BumpDurationMod, i_BumpRestDurationMod);
                    whatBumps = WhatBumps.Dunk;
                }
                else
                {
                    Damage(_damages);
                    //currentHealth -= _damages;
                }
                break;

            case DamageSource.RedBarrelExplosion:
                if (isBumpable)
                {
                    damageAfterBump = _damages;
                    EnergyManager.IncreaseEnergy(energyGainedOnHit);
                    i_normalizedImpactVector = new Vector3(_impactVector.x, 0, _impactVector.z);
                    if (_bumpModificators != default(Vector3))
                    {
                        i_BumpDistanceMod = _bumpModificators.x;
                        i_BumpDurationMod = _bumpModificators.y;
                        i_BumpRestDurationMod = _bumpModificators.z;
                    }
                    BumpMe(10, 1, 1, i_normalizedImpactVector.normalized, i_BumpDistanceMod, i_BumpDurationMod, i_BumpRestDurationMod);
                    whatBumps = WhatBumps.RedBarrel;
                }
                else
                {
                    Damage(_damages);
                    //currentHealth -= _damages;
                }
                break;

            case DamageSource.Ball:
				AnalyticsManager.IncrementData("DamageWithPass", _damages);
				animator.SetTrigger("HitTrigger");
                FeedbackManager.SendFeedback("event.BallHittingEnemy", this, _ball.transform.position, _impactVector, _impactVector);
                damageAfterBump = 0;
                EnergyManager.IncreaseEnergy(energyGainedOnHit);
                whatBumps = WhatBumps.Pass;
                Staggered(whatBumps);
                Damage(_damages);
                //currentHealth -= _damages;
                if (currentHealth <= 0)
                {
                    ChangeState(EnemyState.Dying);
                }
                break;
			case DamageSource.PerfectReceptionExplosion:
				Damage(_damages);
				if (currentHealth <= 0)
				{
					ChangeState(EnemyState.Dying);
				}
				FeedbackManager.SendFeedback("event.BallHittingEnemy", this, _ball.transform.position, _impactVector, _impactVector);
				break;
		}


    }

    public override void Kill()
    {
        if (healthBar != null) { Destroy(healthBar.gameObject); }
        onDeath.Invoke();
        GameManager.i.enemyManager.enemiesThatSurround.Remove(GetComponent<EnemyBehaviour>());
        if (Random.Range(0f, 1f) <= coreDropChances)
        {
            DropCore();
        }
        base.Kill();
    }
    protected void DropCore()
    {
        GameObject i_newCore = Instantiate(Resources.Load<GameObject>("EnemyResource/EnemyCore"));
        i_newCore.name = "Core of " + gameObject.name;
        i_newCore.transform.position = transform.position;
        Vector3 i_wantedDirectionAngle = SwissArmyKnife.RotatePointAroundPivot(Vector3.forward, Vector3.up, new Vector3(0, Random.Range(0, 360), 0));
        float i_throwForce = Random.Range(minMaxDropForce.x, minMaxDropForce.y);
        i_wantedDirectionAngle.y = i_throwForce * 0.035f;
        i_newCore.GetComponent<CorePart>().Init(null, i_wantedDirectionAngle.normalized * i_throwForce, 1, (int)Random.Range(minMaxCoreHealthValue.x, minMaxCoreHealthValue.y));
    }

    void CheckDistanceAndAdaptFocus()
    {
        //print(focusedPlayer);
        //Checking who is in range
        if (distanceWithPlayerOne < focusDistance && playerOnePawnController.IsTargetable())
        {
            playerOneInRange = true;
        }
        else
        {
            playerOneInRange = false;
        }

        if (distanceWithPlayerTwo < focusDistance && playerTwoPawnController.IsTargetable())
        {
            playerTwoInRange = true;
        }
        else
        {
            playerTwoInRange = false;
        }

        //Unfocus player because of distance
        if (focusedPlayer != null)
        {
            if ((focusedPlayer == playerOneTransform && (distanceWithPlayerOne > unfocusDistance || !playerOnePawnController.IsTargetable()))
                || ((focusedPlayer == playerTwoTransform && (distanceWithPlayerTwo > unfocusDistance || !playerTwoPawnController.IsTargetable()))))
            {
                ChangingFocus(null);
            }
        }

        //Changing focus between the two
        if ((playerOneInRange && playerOnePawnController.IsTargetable())
            && (playerTwoInRange && playerTwoPawnController.IsTargetable())
            && focusedPlayer != null)
        {
            if (focusedPlayer == playerOneTransform && distanceWithPlayerOne - distanceWithPlayerTwo > distanceBeforeChangingPriority)
            {
                ChangingFocus(playerTwoTransform);
            }
            else if (focusedPlayer == playerTwoTransform && distanceWithPlayerTwo - distanceWithPlayerOne > distanceBeforeChangingPriority)
            {
                ChangingFocus(playerOneTransform);
            }
        }

        //no focused yet ? Choose one
        if (((playerOneInRange && playerOnePawnController.IsTargetable())
            || (playerTwoInRange && playerTwoPawnController.IsTargetable()))
            && focusedPlayer == null)
        {
            ChangingFocus(GetClosestAndAvailablePlayer());
        }
    }

    void ChangingFocus(Transform _newFocus)
    {
        focusedPlayer = _newFocus;
        AddSpeedCoef(new SpeedCoef(0.5f, 2f, SpeedMultiplierReason.Dash, false));
    }

    public void Staggered(WhatBumps? cause = default)
    {
        switch (cause)
        {
            case WhatBumps.Pass:
                AddSpeedCoef(new SpeedCoef(speedMultiplierFromPassHit, timeToRecoverSlowFromPass, SpeedMultiplierReason.Pass, false));
                break;
            case WhatBumps.Dunk:
                AddSpeedCoef(new SpeedCoef(speedMultiplierFromDunkHit, timeToRecoverSlowFromDunk, SpeedMultiplierReason.Dunk, false));
                break;
            case WhatBumps.Environment:
                AddSpeedCoef(new SpeedCoef(0.5f, 0.5f, SpeedMultiplierReason.Environment, false));
                break;
            default:
                AddSpeedCoef(new SpeedCoef(0.5f, 0.5f, SpeedMultiplierReason.Unknown, false));
                break;
        }
    }

    public override void BumpMe(float _bumpDistance, float _bumpDuration, float _restDuration, Vector3 _bumpDirection, float _randomDistanceMod, float _randomDurationMod, float _randomRestDurationMod)
    {
        base.BumpMe(_bumpDistance, _bumpDuration, _restDuration, _bumpDirection, _randomDistanceMod, _randomDurationMod, _randomRestDurationMod);
        ChangeState(EnemyState.Bumped);
    }

    IEnumerator WaitABit_C(float _duration)
    {
        yield return new WaitForSeconds(_duration);
        ChangeState(EnemyState.Following);
    }
}