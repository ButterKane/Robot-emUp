using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using UnityEngine.Events;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.Analytics;
using System.Linq;

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
    Sniper,
    Laser
}

public class EnemyBehaviour : PawnController, IHitable
{
    [ReadOnly] public EnemyState enemyState = EnemyState.Idle;

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
    public EnemyFocusValues focusValues;
    [HideInInspector] protected float timeBetweenCheck = 0;

    [Space(3)]
    [Header("Movement")]
    public EnemyMoveValues moveValues;

    [Space(3)]
    [Header("Common attack variables")]
    public EnemyAttackvalues attackValues;
    
    protected float currentAnticipationTime;
    protected float attackDuration;
    protected float cooldownDuration;
    protected bool endOfAttackTriggerLaunched;

    float currentTimePausedAfterAttack;
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
    public EnemyDeathValues deathValues;
    [System.NonSerialized] public UnityEvent onDeath = new UnityEvent();
    private float currentDeathWaitTime;
    private HealthBar healthBar;

    protected virtual void Start()
    {
        InitializePlayersRefs();

        foreach (AnimatorControllerParameter  t in animator.parameters)
        {
            if (t.name == "isFastDeployment") 
            {
                animator.SetBool("isFastDeployment", isDeploymentFast);
            }
        }
        timeBetweenCheck = focusValues.maxTimeBetweenCheck;
        
        EnemyManager.i.enemies.Add(this);
        if (canSurroundPlayer) { EnemyManager.i.enemiesThatSurround.Add(this); }
        healthBar = Instantiate(healthBarPrefab, CanvasManager.i.mainCanvas.transform).GetComponent<HealthBar>();
        healthBar.target = this;
        selfCollider = GetComponent<Collider>();

        ChangeState(EnemyState.Idle);
    }

    protected void Update()
    {
        UpdateDistancesToPlayers();
        UpdateState();
        UpdateSpeed();
        UpdateHealthBar();
    }

    #region EnemyState Changing
    public void ChangeState(EnemyState _newState)
    {
        if (enemyState == EnemyState.Dying) { return; }
        ExitState();
        EnterState(_newState);
        enemyState = _newState;
    }

    void EnterState(EnemyState _newState)
    {
        switch (_newState)
        {
            case EnemyState.Idle:
                if (focusValues != null)
                {
                    timeBetweenCheck = focusValues.maxTimeBetweenCheck;
                }
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
                break;
            case EnemyState.Attacking:
                EnterAttackingState();
                break;
            case EnemyState.PauseAfterAttack:
                if (attackValues != null)
                {
                    currentTimePausedAfterAttack = attackValues.maxTimePauseAfterAttack;
                }
                break;
            case EnemyState.Dying:
                selfCollider.enabled = false;
                animator.SetTrigger("DeathTrigger");
                currentDeathWaitTime = deathValues.waitTimeBeforeDisappear;
                Freeze();
                if (navMeshAgent != null && navMeshAgent.enabled == true) { navMeshAgent.isStopped = true; }
                break;
            case EnemyState.Spawning:
                break;
            case EnemyState.Deploying:
                // is played automatically in the animator. See "isFastDeployment" bool to manage aniamtions
                break;
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
                DestroySpawnedAttackUtilities();
                break;
            case EnemyState.PauseAfterAttack:
                cooldownDuration = attackValues.cooldownAfterAttackTime;
                break;
            case EnemyState.Dying:
                break;
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
                    timeBetweenCheck = focusValues.maxTimeBetweenCheck;
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
                    timeBetweenCheck = focusValues.maxTimeBetweenCheck;
                }

                if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
                {
                    if (distanceWithFocusedPlayer < focusValues.closestDistanceToplayer)
                    {
                        navMeshAgent.isStopped = true;
                    }
                    else
                    {
                        navMeshAgent.isStopped = false;
                        if (focusedPawnController != null)
                        {
                            // Rotate to face the focused player
                            Quaternion i_targetRotation = Quaternion.LookRotation(focusedPawnController.GetCenterPosition() - transform.position);
                            i_targetRotation.eulerAngles = new Vector3(0, i_targetRotation.eulerAngles.y, 0);
                            transform.rotation = Quaternion.Lerp(transform.rotation, i_targetRotation, attackValues.rotationSpeedPreparingAttack);

                            if (closestSurroundPoint != null)
                            {
                                if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled && GetNavMesh().enabled)
                                { navMeshAgent.SetDestination(UpdateNewDestinationAlongBezierCurve(closestSurroundPoint.position)); }
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
                if (distanceWithFocusedPlayer <= attackValues.distanceToAttack && cooldownDuration < 0)
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
                currentTimePausedAfterAttack -= Time.deltaTime;
                if (currentTimePausedAfterAttack <= 0)
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
    #endregion

    #region Public Methods
    public override void UpdateAnimatorBlendTree()
    {
        base.UpdateAnimatorBlendTree(); // Don't do anything if there is no animator
        if (canMove)
        {
            animator.SetFloat("IdleRunBlend", navMeshAgent.velocity.magnitude / navMeshAgent.speed);
        }
    }

    public virtual IEnumerator ResetPreparingAttackState()
    {
        ChangeState(EnemyState.Idle);
        yield return null;
    }

    public virtual void EnterPreparingAttackState()
    {
        navMeshAgent.enabled = false;
        currentAnticipationTime = attackValues.maxAnticipationTime;
        animator.SetTrigger("AnticipateAttackTrigger");
    }

    public virtual void PreparingAttackState()
    {
        if (currentAnticipationTime > attackValues.portionOfAnticipationWithRotation * attackValues.maxAnticipationTime)
        {
            Quaternion i_targetRotation = Quaternion.LookRotation(focusedPawnController.transform.position - transform.position);
            i_targetRotation.eulerAngles = new Vector3(0, i_targetRotation.eulerAngles.y, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, i_targetRotation, attackValues.rotationSpeedPreparingAttack);
        }
        currentAnticipationTime -= Time.deltaTime;

        if (currentAnticipationTime <= 0)
        {
            ChangeState(EnemyState.Attacking);
        }
    }

    public virtual void EnterAttackingState(string attackSound = "EnemyAttack")
    {
        endOfAttackTriggerLaunched = false;
        animator.SetTrigger("AttackTrigger");
        mustCancelAttack = false;
    }

    public virtual void AttackingState()
    {
        //ChangeState(EnemyState.PauseAfterAttack);
    }

    public virtual void EnterBumpedState()
    {
        navMeshAgent.enabled = false;
        animator.SetTrigger("BumpTrigger");
    }

    public virtual void ExitBumpedState()
    {
        //Staggered(whatBumps);
    }

    public virtual void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, float _damages, DamageSource _source, Vector3 _bumpModificators = default(Vector3))
    {
        damageAfterBump = 0;
        Vector3 i_normalizedImpactVector;
        LockManager.UnlockTarget(this.transform);

        switch (_source)
        {
            case DamageSource.Dunk:
                Analytics.CustomEvent("DamageWithDunk", new Dictionary<string, object> { { "Zone", GameManager.GetCurrentZoneName() }, });

                if (isBumpable)
                {
                    if (enemyType == EnemyTypes.RedBarrel)
                    {
                        EnemyRedBarrel i_selfRef = GetComponent<EnemyRedBarrel>();
                        i_selfRef.willExplode = false;
                    }

                    damageAfterBump = _damages;
                    i_normalizedImpactVector = new Vector3(_impactVector.x, 0, _impactVector.z);

                    BumpMe(i_normalizedImpactVector.normalized, BumpForce.Force2);
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
                EnergyManager.IncreaseEnergy(energyGainedOnHit);

                Damage(_damages);
                break;

            case DamageSource.PerfectReceptionExplosion:
                Damage(_damages);
                FeedbackManager.SendFeedback("event.BallHittingEnemy", this, _ball.transform.position, _impactVector, _impactVector);

                BallDatas bd = _ball.GetCurrentBallDatas();
                float ballChargePercent = (_ball.GetCurrentDamageModifier() - 1) / (bd.maxDamageModifierOnPerfectReception - 1);

                // Bump or push depending on Ball charge value
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
    #endregion

    #region Private and protected methods
    protected void CheckDistanceAndAdaptFocus()
    {
        if (focusValues == null) { return; }
        //Checking who is in range
        if (distanceWithPlayerOne < focusValues.focusDistance && playerOnePawnController.IsTargetable() && transform.position.y > playerOneTransform.position.y - focusValues.maxHeightOfDetection && transform.position.y < playerOneTransform.position.y + focusValues.maxHeightOfDetection)
        {
            playerOneInRange = true;
        }
        else
        {
            playerOneInRange = false;
        }

        if (distanceWithPlayerTwo < focusValues.focusDistance && playerTwoPawnController.IsTargetable() && transform.position.y > playerTwoTransform.position.y - focusValues.maxHeightOfDetection && transform.position.y < playerTwoTransform.position.y + focusValues.maxHeightOfDetection)
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
            if (focusedPawnController.transform.position.y < transform.position.y - focusValues.maxHeightOfDetection && focusedPawnController.transform.position.y > transform.position.y + focusValues.maxHeightOfDetection)
            {
                //Debug.Log("Changing to null due to height");
                ChangingFocus(null);
                playerOneInRange = false;
                playerTwoInRange = false;
                ChangeState(EnemyState.Idle);
                return;
            }

            if ((focusedPawnController.transform == playerOneTransform && (distanceWithPlayerOne > focusValues.unfocusDistance || !playerOnePawnController.IsTargetable()))
                || ((focusedPawnController.transform == playerTwoTransform && (distanceWithPlayerTwo > focusValues.unfocusDistance || !playerTwoPawnController.IsTargetable()))))
            {
                ChangingFocus(null);
            }
        }

        //Changing focus between the two
        if ((playerOneInRange && playerOnePawnController.IsTargetable())
            && (playerTwoInRange && playerTwoPawnController.IsTargetable())
            && focusedPawnController != null)
        {
            if (focusedPawnController.transform == playerOneTransform && distanceWithPlayerOne - distanceWithPlayerTwo > focusValues.distanceBeforeChangingPriority)
            {
                ChangingFocus(playerTwoTransform);
            }
            else if (focusedPawnController.transform == playerTwoTransform && distanceWithPlayerTwo - distanceWithPlayerOne > focusValues.distanceBeforeChangingPriority)
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

    protected Transform GetClosestAndAvailablePlayer()
    {
        if ((distanceWithPlayerOne >= distanceWithPlayerTwo &&
            (playerTwoPawnController.IsTargetable()) || !playerOnePawnController.IsTargetable()) &&
            heightDeltaWithPlayerTwo < focusValues.maxHeightOfDetection)
        {
            return playerTwoTransform;
        }
        else if ((distanceWithPlayerTwo >= distanceWithPlayerOne &&
            (playerOnePawnController.IsTargetable()) || !playerTwoPawnController.IsTargetable()) &&
            heightDeltaWithPlayerOne < focusValues.maxHeightOfDetection)
        {
            return playerOneTransform;
        }
        else
        {
            return null;
        }
    }

    void ChangingFocus(Transform _newFocus)
    {
        if (_newFocus != null) { focusedPawnController = _newFocus.GetComponent<PawnController>(); }
        else { focusedPawnController = null; }
    }

    void UpdateDistancesToPlayers()
    {
        distanceWithPlayerOne = Vector3.Distance(transform.position, playerOneTransform.position);
        distanceWithPlayerTwo = Vector3.Distance(transform.position, playerTwoTransform.position);
        if (focusedPawnController != null)
            distanceWithFocusedPlayer = Vector3.Distance(transform.position, focusedPawnController.transform.position);
    }

    private void InitializePlayersRefs()
    {
        playerOneTransform = GameManager.playerOne.transform;
        playerTwoTransform = GameManager.playerTwo.transform;
        playerOnePawnController = playerOneTransform.GetComponent<PlayerController>();
        playerTwoPawnController = playerTwoTransform.GetComponent<PlayerController>();
    }

    protected void UpdateHealthBar()
    {
        if (GetHealth() < GetMaxHealth() && healthBar != null)
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
            navMeshAgent.speed = pawnMovementValues.moveSpeed * GetSpeedCoef();
        }
    }

    protected Vector3 UpdateNewDestinationAlongBezierCurve(Vector3 _endPosition)
    {
        float i_distanceToPointRatio = (1 + (transform.position - _endPosition).magnitude / bezierDistanceToHeightRatio);  // widens the arc of surrounding the farther the surroundingPoint is

        Vector3 i_p0 = transform.position;    // The starting point

        Vector3 i_p2 = SwissArmyKnife.GetFlattedDownPosition(_endPosition, transform.position);  // The destination

        float i_angle = Vector3.SignedAngle(i_p2 - i_p0, focusedPawnController.transform.position - i_p0, Vector3.up);

        int i_moveSens = i_angle > 1 ? 1 : -1;

        Vector3 i_p1 = i_p0 + (i_p2 - i_p0) / 0.5f + Vector3.Cross(i_p2 - i_p0, Vector3.up) * i_moveSens * bezierCurveHeight * i_distanceToPointRatio;  // "third point" of the bezier curve

        // Calculating position on bezier curve, following start point, end point and avancement
        // In this version, the avancement has been replaced by a constant because it's recalculated every frame
        Vector3 i_positionOnBezierCurve = (Mathf.Pow(0.5f, 2) * i_p0) + (2 * 0.5f * 0.5f * i_p1) + (Mathf.Pow(0.5f, 2) * i_p2);

        return SwissArmyKnife.GetFlattedDownPosition(i_positionOnBezierCurve, focusedPawnController.transform.position);
    }

    /// <summary>
    /// Need cleaning => Create core with enemy and drop it afterward
    /// </summary>
    protected void DropCore()
    {
        GameObject i_newCore = Instantiate(Resources.Load<GameObject>("EnemyResource/EnemyCore"));
        i_newCore.name = "Core of " + gameObject.name;
        i_newCore.transform.position = transform.position;
        Vector3 i_wantedDirectionAngle = SwissArmyKnife.RotatePointAroundPivot(Vector3.forward, Vector3.up, new Vector3(0, Random.Range(0, 360), 0));
        float i_throwForce = Random.Range(deathValues.minMaxDropForce.x, deathValues.minMaxDropForce.y);
        i_wantedDirectionAngle.y = i_throwForce * 0.035f;
        i_newCore.GetComponent<CorePart>().Init(null, i_wantedDirectionAngle.normalized * i_throwForce, 1, (int)Random.Range(deathValues.minMaxCoreHealthValue.x, deathValues.minMaxCoreHealthValue.y));
    }
    #endregion

    #region Overridden methods
    public override void Damage(float _amount, bool _enableInvincibilityFrame = false)
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
        EnemyManager.i.RemoveEnemy(this);
        if (Random.Range(0f, 1f) <= deathValues.coreDropChances)
        {
            DropCore();
        }
        base.Kill();
    }
    #endregion

    #region Coroutines
    IEnumerator WaitABit_C(float _duration)
    {
        yield return new WaitForSeconds(_duration);
        ChangeState(EnemyState.Following);
    }
    #endregion
}