using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using UnityEngine.Events;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.Analytics;

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
    Spawning,
    Deploying
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
    [ReadOnly] public Collider selfCollider;

    [Space(2)]
    [Separator("Tweakable variables")]
    public EnemyTypes enemyType = EnemyTypes.Melee;
    protected bool playerOneInRange;
    protected bool playerTwoInRange;
    protected float distanceWithPlayerOne;
    protected float distanceWithPlayerTwo;
    protected float distanceWithFocusedPlayer;
    protected float heightDeltaWithPlayerOne;
    protected float heightDeltaWithPlayerTwo;
    /*[System.NonSerialized]*/ public PawnController focusedPawnController = null;
    public float energyGainedOnHit = 1;
    public int damage = 10;
    public float powerLevel = 1;
    [SerializeField] protected bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
    [SerializeField] protected float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }

    [SerializeField] private Vector3 lockSize3DModifier = Vector3.one; public Vector3 lockSize3DModifier_access { get { return lockSize3DModifier; } set { lockSize3DModifier = value; } }
    public bool arenaRobot;
    public bool isDeploymentFast = true;

    [Space(3)]
    [Header("Focus")]
    public float focusDistance = 3;
    public float maxHeightOfDetection = 3;
    public float unfocusDistance = 20;
    [HideInInspector] public float timeBetweenCheck = 0;
    public float distanceBeforeChangingPriority = 3;
    public float maxTimeBetweenCheck = 0.25f;
    public float closestDistanceToplayer = 2; // The closest a following enemy can go to a player without touching it

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
    [Range(0, 1)] public float rotationSpeedPreparingAttack = 0.2f;
    public float distanceToAttack = 5;
    [Range(0, 1)] public float portionOfAnticipationWithRotation = 0.3f;
    public float cooldownAfterAttackTime = 1f;


    protected float anticipationTime;
    protected float attackDuration;
    protected float cooldownDuration;
    protected bool endOfAttackTriggerLaunched;
    

    float timePauseAfterAttack;
    protected bool mustCancelAttack;

    [Space(3)]
    [Header("Surrounding")]
    public bool canSurroundPlayer;
    [Range(0, 1)] public float bezierCurveHeight = 0.5f;
    public float bezierDistanceToHeightRatio = 100f;
    [System.NonSerialized] public Transform closestSurroundPoint;

    [Space(3)]
    [Header("Spawn")]
    public float spawnImpactRadius = 1f;
    public int spawnImpactDamages = 1;

    [Space(3)]
    [Header("Death")]
    public float coreDropChances = 1;
    public Vector2 minMaxDropForce;
    public Vector2 minMaxCoreHealthValue = new Vector2(1, 3);
    [System.NonSerialized] public UnityEvent onDeath = new UnityEvent();
    public float waitTimeBeforeDisappear = 1;
    private float currentDeathWaitTime;
    private HealthBar healthBar;

    protected virtual void Start()
    {
        animator.SetBool("isFastDeployment", isDeploymentFast);
        timeBetweenCheck = maxTimeBetweenCheck;
        playerOneTransform = GameManager.playerOne.transform;
        playerTwoTransform = GameManager.playerTwo.transform;
        playerOnePawnController = playerOneTransform.GetComponent<PlayerController>();
        playerTwoPawnController = playerTwoTransform.GetComponent<PlayerController>();
        GameManager.i.enemyManager.enemies.Add(this);
        if (canSurroundPlayer) { GameManager.i.enemyManager.enemiesThatSurround.Add(this); }
        healthBar = Instantiate(healthBarPrefab, CanvasManager.i.mainCanvas.transform).GetComponent<HealthBar>();
        healthBar.target = this;
        selfCollider = GetComponent<Collider>();
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
                if (focusedPawnController != null)
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

                if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
                {
                    if (distanceWithFocusedPlayer < closestDistanceToplayer)
                    {
                        navMeshAgent.isStopped = true;
                    }
                    else
                    {
                        navMeshAgent.isStopped = false;
                        if (focusedPawnController != null)
                        {
                            Quaternion _targetRotation = Quaternion.LookRotation(focusedPawnController.GetCenterPosition() - transform.position);
                            _targetRotation.eulerAngles = new Vector3(0, _targetRotation.eulerAngles.y, 0);
                            transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, rotationSpeedPreparingAttack);

                            if (closestSurroundPoint != null)
                            {
                                float i_distanceToPointRatio = (1 + (transform.position - closestSurroundPoint.position).magnitude / bezierDistanceToHeightRatio);  // widens the arc of surrounding the farther the surroundingPoint is

                                Vector3 i_p0 = transform.position;    // The starting point

                                Vector3 i_p2 = SwissArmyKnife.GetFlattedDownPosition(closestSurroundPoint.position, transform.position);  // The destination

                                float i_angle = Vector3.SignedAngle(i_p2 - i_p0, focusedPawnController.transform.position - i_p0, Vector3.up);

                                int i_moveSens = i_angle > 1 ? 1 : -1;

                                Vector3 i_p1 = i_p0 + (i_p2 - i_p0) / 0.5f + Vector3.Cross(i_p2 - i_p0, Vector3.up) * i_moveSens * bezierCurveHeight * i_distanceToPointRatio;  // "third point" of the bezier curve

                                // Calculating position on bezier curve, following start point, end point and avancement
                                // In this version, the avancement has been replaced by a constant because it's recalculated every frame
                                Vector3 i_positionOnBezierCurve = (Mathf.Pow(0.5f, 2) * i_p0) + (2 * 0.5f * 0.5f * i_p1) + (Mathf.Pow(0.5f, 2) * i_p2);
                                if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled && GetNavMesh().enabled) { navMeshAgent.SetDestination(SwissArmyKnife.GetFlattedDownPosition(i_positionOnBezierCurve, focusedPawnController.transform.position)); }
                            }
                            else
                            {
                                if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled && GetNavMesh().enabled)
                                {
                                    navMeshAgent.SetDestination(focusedPawnController.transform.position);
                                }
                            }
                        }
                    }
                }
                cooldownDuration -= Time.deltaTime;
                if (distanceWithFocusedPlayer <= distanceToAttack && cooldownDuration < 0)
                {
                    ChangeState(EnemyState.PreparingAttack);
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
                currentDeathWaitTime -= Time.deltaTime;
                if (currentDeathWaitTime < 0)
                {
                    Kill();
                }
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
                DestroySpawnedAttackUtilities();
                EnterBumpedState();
                break;
            case EnemyState.ChangingFocus:
                break;
            case EnemyState.PreparingAttack:
				EnterPreparingAttackState();
                //ChangeState("MeleeEnemyAnticipating",EnterPreparingAttackState(), ResetPreparingAttackState());
                break;
            case EnemyState.Attacking:
                EnterAttackingState();
                break;
            case EnemyState.PauseAfterAttack:
                timePauseAfterAttack = maxTimePauseAfterAttack;
                break;
            case EnemyState.Dying:
                selfCollider.enabled = false;
                currentDeathWaitTime = waitTimeBeforeDisappear;
                bool i_thereIsAnAnimation = false;
                foreach (AnimatorControllerParameter param in animator.parameters)
                {
                    if (param.name == "DeathTrigger")
                    {
                        animator.SetTrigger("DeathTrigger");
                        i_thereIsAnAnimation = true;
                        Freeze();
                        if (navMeshAgent != null && navMeshAgent.enabled == true) { navMeshAgent.isStopped = true; }
                    }
                }
                
                if(!i_thereIsAnAnimation)
                {
                    Kill();
                }
                
                break;
            case EnemyState.Spawning:
                break;
            case EnemyState.Deploying:
                foreach (AnimatorControllerParameter param in animator.parameters)
                {
                    if (param.name == "DeployTrigger") { animator.SetTrigger("DeployTrigger");}
                }
                break;
        }
    }

    public virtual void EnterBumpedState()
    {
        navMeshAgent.enabled = false;
        animator.SetTrigger("BumpTrigger");
    }

    public virtual void EnterPreparingAttackState()
    {
        navMeshAgent.enabled = false;
        anticipationTime = maxAnticipationTime;
        animator.SetTrigger("AnticipateAttackTrigger");
    }

    public virtual IEnumerator ResetPreparingAttackState()
    {
        ChangeState(EnemyState.Idle);
        yield return null;
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
        if (anticipationTime > portionOfAnticipationWithRotation * maxAnticipationTime)
        {
            Quaternion _targetRotation = Quaternion.LookRotation(focusedPawnController.transform.position - transform.position);
            _targetRotation.eulerAngles = new Vector3(0, _targetRotation.eulerAngles.y, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, rotationSpeedPreparingAttack);
        }
        anticipationTime -= Time.deltaTime;

        if (anticipationTime <= 0)
        {
            //if (attackPreviewPlane) { attackPreviewPlane.SetActive(false); } // making preview zone disappear
            ChangeState(EnemyState.Attacking);
        }
    }

    public virtual void AttackingState()
    {
        //ChangeState(EnemyState.PauseAfterAttack);
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
                DestroySpawnedAttackUtilities();
                break;
            case EnemyState.PauseAfterAttack:
                cooldownDuration = cooldownAfterAttackTime;
                break;
            case EnemyState.Dying:
                break;
        }
    }

    public virtual void DestroySpawnedAttackUtilities()
    {
        // Usually filled within the inherited scripts
    }

    public virtual void ExitBumpedState()
    {
        //Staggered(whatBumps);
    }

    void UpdateDistancesToPlayers()
    {
        distanceWithPlayerOne = Vector3.Distance(transform.position, playerOneTransform.position);
        distanceWithPlayerTwo = Vector3.Distance(transform.position, playerTwoTransform.position);
        if (focusedPawnController != null)
            distanceWithFocusedPlayer = Vector3.Distance(transform.position, focusedPawnController.transform.position);
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

    public virtual void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, float _damages, DamageSource _source, Vector3 _bumpModificators = default(Vector3))
    {
        Vector3 i_normalizedImpactVector;
        LockManager.UnlockTarget(this.transform);
        if (!CanDamage()) { return; }
        switch (_source)
        {
            case DamageSource.Dunk:
                
                if (isBumpable)
                {
                    if (enemyType == EnemyTypes.RedBarrel)
                    {
                        EnemyRedBarrel i_selfRef = GetComponent<EnemyRedBarrel>();
                        i_selfRef.willExplode = false;
                    }

					Analytics.CustomEvent("DamageWithDunk", new Dictionary<string, object> { { "Zone", GameManager.GetCurrentZoneName() }, });
					damageAfterBump = _damages;

                    i_normalizedImpactVector = new Vector3(_impactVector.x, 0, _impactVector.z);
                    if (_thrower.GetComponent<DunkController>() != null)
                    {
                        DunkController i_controller = _thrower.GetComponent<DunkController>();
                    }
                    BumpMe(i_normalizedImpactVector.normalized, BumpForce.Force2);
                    whatBumps = WhatBumps.Dunk;
                }
                else
                {
                    Damage(_damages);
                }
                break;

            case DamageSource.RedBarrelExplosion:
                if (isBumpable && enemyType != EnemyTypes.RedBarrel)
                {
                    damageAfterBump = _damages;
                    i_normalizedImpactVector = new Vector3(_impactVector.x, 0, _impactVector.z);
                    BumpMe(i_normalizedImpactVector.normalized, BumpForce.Force2);
                    whatBumps = WhatBumps.RedBarrel;
                }
                else
                {
                    Damage(_damages);
                }
                break;

            case DamageSource.Ball:
                Analytics.CustomEvent("DamageWithBall", new Dictionary<string, object> { { "Zone", GameManager.GetCurrentZoneName() }, });
				animator.SetTrigger("HitTrigger");
                FeedbackManager.SendFeedback("event.BallHittingEnemy", this, _ball.transform.position, _impactVector, _impactVector);
                damageAfterBump = 0;
                EnergyManager.IncreaseEnergy(energyGainedOnHit);
                whatBumps = WhatBumps.Pass;
                //Staggered(whatBumps);
                Damage(_damages);
                break;

            case DamageSource.PerfectReceptionExplosion:
                Damage(_damages);

				BallDatas bd = _ball.currentBallDatas;
				float ballChargePercent = (_ball.GetCurrentDamageModifier() - 1) / (bd.maxDamageModifierOnPerfectReception - 1);
				Debug.Log(ballChargePercent);
				if (ballChargePercent >= bd.minimalChargeForBump)
				{
					BumpMe(_impactVector.normalized, BumpForce.Force1);
				}
				else if (ballChargePercent >= bd.minimalChargeForHeavyPush)
				{
					Push(PushType.Heavy, _impactVector.normalized, PushForce.Force1);
				}
				else if (ballChargePercent >= bd.minimalChargeForLightPush)
				{
					Push(PushType.Light, _impactVector.normalized, PushForce.Force1);
				}
                FeedbackManager.SendFeedback("event.BallHittingEnemy", this, _ball.transform.position, _impactVector, _impactVector);
                break;

            case DamageSource.Laser:
                Damage(_damages);

                break;
            case DamageSource.ReviveExplosion:
                Push(PushType.Heavy, _impactVector, PushForce.Force2);
                break;
            case DamageSource.DeathExplosion:
                Push(PushType.Heavy, _impactVector, PushForce.Force2);
                break;
            case DamageSource.SpawnImpact:
                Push(PushType.Light, _impactVector, PushForce.Force1);
                break;
        }
    }

    public override void Damage(float _amount)
    {
        if (!CanDamage()) { return; }
        FeedbackManager.SendFeedback(eventOnBeingHit, this, transform.position, transform.up, transform.up);

        currentHealth -= _amount;

        if (currentHealth <= 0)
        {
            ChangeState(EnemyState.Dying);
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
        if (distanceWithPlayerOne < focusDistance && playerOnePawnController.IsTargetable() && transform.position.y > playerOneTransform.position.y - maxHeightOfDetection && transform.position.y < playerOneTransform.position.y + maxHeightOfDetection)
        {
            playerOneInRange = true;
        }
        else
        {
            playerOneInRange = false;
        }

        if (distanceWithPlayerTwo < focusDistance && playerTwoPawnController.IsTargetable() && transform.position.y > playerTwoTransform.position.y - maxHeightOfDetection && transform.position.y < playerTwoTransform.position.y + maxHeightOfDetection)
        {
            playerTwoInRange = true;
        }
        else
        {
            playerTwoInRange = false;
        }

        //Unfocus player because of distance
        if (focusedPawnController != null)
        {
            if (focusedPawnController.transform.position.y < transform.position.y - maxHeightOfDetection && focusedPawnController.transform.position.y >  transform.position.y + maxHeightOfDetection)
            {
                //Debug.Log("Changing to null due to height");
                ChangingFocus(null);
                playerOneInRange = false;
                playerTwoInRange = false;
                ExitState();
                EnterState(EnemyState.Idle);
                return;
            }
            
            if ((focusedPawnController.transform == playerOneTransform && (distanceWithPlayerOne > unfocusDistance || !playerOnePawnController.IsTargetable()))
                || ((focusedPawnController.transform == playerTwoTransform && (distanceWithPlayerTwo > unfocusDistance || !playerTwoPawnController.IsTargetable()))))
            {
                ChangingFocus(null);
            }
        }

        //Changing focus between the two
        if ((playerOneInRange && playerOnePawnController.IsTargetable())
            && (playerTwoInRange && playerTwoPawnController.IsTargetable())
            && focusedPawnController != null)
        {
            if (focusedPawnController.transform == playerOneTransform && distanceWithPlayerOne - distanceWithPlayerTwo > distanceBeforeChangingPriority)
            {
                ChangingFocus(playerTwoTransform);
            }
            else if (focusedPawnController.transform == playerTwoTransform && distanceWithPlayerTwo - distanceWithPlayerOne > distanceBeforeChangingPriority)
            {
                ChangingFocus(playerOneTransform);
            }
        }

        //no focused yet ? Choose one
        if (((playerOneInRange && playerOnePawnController.IsTargetable())
            || (playerTwoInRange && playerTwoPawnController.IsTargetable()))
            && focusedPawnController == null)
        {
            ChangingFocus(GetClosestAndAvailablePlayer());
        }
    }

    void ChangingFocus(Transform _newFocus)
    {
		if (_newFocus != null)
		{
			//Debug.Log(_newFocus);
			focusedPawnController = _newFocus.GetComponent<PawnController>();
		} else
		{
			focusedPawnController = null;
		}
        //AddSpeedCoef(new SpeedCoef(0.5f, 0.2f, SpeedMultiplierReason.ChangingFocus, false));
    }

    //public void Staggered(WhatBumps? cause = default)
    //{
    //    switch (cause)
    //    {
    //        case WhatBumps.Pass:
    //            AddSpeedCoef(new SpeedCoef(speedMultiplierFromPassHit, timeToRecoverSlowFromPass, SpeedMultiplierReason.Pass, false));
    //            break;
    //        case WhatBumps.Dunk:
    //            AddSpeedCoef(new SpeedCoef(speedMultiplierFromDunkHit, timeToRecoverSlowFromDunk, SpeedMultiplierReason.Dunk, false));
    //            break;
    //        case WhatBumps.Environment:
    //            AddSpeedCoef(new SpeedCoef(0.5f, 0.5f, SpeedMultiplierReason.Environment, false));
    //            break;
    //        default:
    //            AddSpeedCoef(new SpeedCoef(0.5f, 0.5f, SpeedMultiplierReason.Unknown, false));
    //            break;
    //    }
    //}

    IEnumerator WaitABit_C(float _duration)
    {
        yield return new WaitForSeconds(_duration);
        ChangeState(EnemyState.Following);
    }
}