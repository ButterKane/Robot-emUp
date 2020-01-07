using System.Collections;
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
    Environment,
    Count
}


public class EnemyBehaviour : MonoBehaviour, IHitable
{
    [System.NonSerialized] public EnemyState EnemyState = EnemyState.Idle;

    [Separator("References")]
    [SerializeField] protected Transform self;
    public Rigidbody rb;
    public Animator animator;
    public NavMeshAgent navMeshAgent;
    public Transform healthBarRef;
    public GameObject healthBarPrefab;
    [System.NonSerialized] public string hitSound = "EnemyHit";

	[Space(2)]
    [Separator("Auto-assigned References")]
    [SerializeField] protected Transform playerOneTransform;
    protected PawnController playerOnePawnController;
    [SerializeField] protected Transform playerTwoTransform;
    protected PawnController playerTwoPawnController;


    [Space(2)]
    [Separator("Tweakable variables")]
    protected bool playerOneInRange;
    protected bool playerTwoInRange;
    public int maxHealth = 30;
    [System.NonSerialized] public int health;
    protected float distanceWithPlayerOne;
    protected float distanceWithPlayerTwo;
    protected float distanceWithFocusedPlayer;
    [System.NonSerialized] public Transform focusedPlayer = null;
    public float energyAmount = 1;
    public int damage = 10;
	public float powerLevel = 1;
    [SerializeField] protected bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
	[SerializeField] protected float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }
	public bool arenaRobot;

	[Space(2)]
    [Header("Focus")]
    public float focusDistance = 18;
    public float unfocusDistance = 20;
    public float timeBetweenCheck = 0;
    public float distanceBeforeChangingPriority = 3;
    public float maxTimeBetweenCheck = 0.25f;

    [Space(2)]
    [Header("Movement")]
    public float normalSpeed = 7; // This value is the one in the inspector, but in practice it is modified by the Random speed mod
    [System.NonSerialized] public float actualSpeed;
    public float normalAcceleration = 30;
    public float randomSpeedMod;
    private float moveMultiplicator = 1;
    private int normalMoveMultiplicator = 1;
    public float slowFromPass;
    public float timeToRecoverSlowFromPass;
    public float slowFromDunk;
    public float timeToRecoverSlowFromDunk;
    public AnimationCurve speedRecoverCurve;
    private float staggerAvancement;
    private WhatBumps whatBumps;

    [Space(2)]
    [Header("Attack")]
    public float distanceToAttack = 5;
    public float maxAnticipationTime = 0.5f;
    [Range(0, 1)] public float rotationSpeedPreparingAttack = 0.2f;
    protected float anticipationTime;
    public float attackMaxDistance = 8;
    public float maxAttackDuration = 0.5f;
    protected float attackDuration;
    protected float attackTimeProgression;
    public AnimationCurve attackSpeedCurve;
    protected Vector3 attackInitialPosition;
    protected Vector3 attackDestination;
    [Range(0, 1)] public float whenToTriggerEndOfAttackAnim;
    protected bool endOfAttackTriggerLaunched;
    public GameObject attackHitBoxPrefab;
    GameObject myAttackHitBox;
    public Vector3 hitBoxOffset;
    public float maxTimePauseAfterAttack = 1;
    float timePauseAfterAttack;
    public float attackRaycastDistance = 2;
    protected bool mustCancelAttack;
    
    [Space(2)]
    [Header("Bump")]
    public bool isBumpable = true;
    public float maxGettingUpDuration = 0.6f;
    public AnimationCurve bumpDistanceCurve;
    float bumpDistance;
    float bumpDuration;
    float restDuration;
    float gettingUpDuration;
    Vector3 bumpInitialPosition;
    Vector3 bumpDestinationPosition;
    Vector3 bumpDirection;
    float bumpTimeProgression;
    [Range(0, 1)] public float whenToTriggerFallingAnim;
    bool fallingTriggerLaunched;
    public float bumpRaycastDistance = 1;
    bool mustCancelBump;
    public int damageAfterBump;

    [Space(2)]
    [Header("FX References")]
    public GameObject deathParticlePrefab;
    public float deathParticleScale = 2;
    public GameObject hitParticlePrefab;
    public float hitParticleScale = 3;

    [Space(2)]
    [Header("Surrounding")]
    [System.NonSerialized] public Transform closestSurroundPoint;
    [Range(0, 1)]
    public float bezierCurveHeight = 0.5f;
    public float bezierDistanceToHeightRatio = 100f;

    [Space(2)]
    [Header("Death")]
    public float coreDropChances = 1;
    public Vector2 minMaxDropForce;
	public Vector2 minMaxCoreHealthValue = new Vector2(1, 3);
    [System.NonSerialized] public UnityEvent onDeath = new UnityEvent();

    

    protected void Start()
    {
        health = maxHealth;
        self = transform;
        actualSpeed = normalSpeed + Random.Range(-randomSpeedMod, randomSpeedMod);
        timeBetweenCheck = maxTimeBetweenCheck;
        playerOneTransform = GameManager.playerOne.transform;
        playerTwoTransform = GameManager.playerTwo.transform;
        playerOnePawnController = playerOneTransform.GetComponent<PlayerController>();
        playerTwoPawnController = playerTwoTransform.GetComponent<PlayerController>();
        GameManager.i.enemyManager.enemies.Add(this);
        GameObject healthBar = Instantiate(healthBarPrefab, CanvasManager.i.mainCanvas.transform);
        healthBar.GetComponent<EnemyHealthBar>().enemy = this;

        if (arenaRobot)
        {
            ChangingState(EnemyState.WaitForCombatStart);
        }
        else
        {
            ChangingState(EnemyState.Idle);
        }
    }

    void Update()
    {
        UpdateDistancesToPlayers();
        UpdateState();
        UpdateAnimatorBlendTree();
    }

    private void UpdateAnimatorBlendTree()
    {
        if (animator != null)
        {
            animator.SetFloat("IdleRunBlend", navMeshAgent.velocity.magnitude / navMeshAgent.speed);
        }
    }

    void UpdateState()
    {
        switch (EnemyState)
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
                    ChangingState(EnemyState.Following);
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
                        float internal_distanceToPointRatio = (1 + (self.position - closestSurroundPoint.position).magnitude / bezierDistanceToHeightRatio);  // widens the arc of surrounding the farther the surroundingPoint is

                        Vector3 internal_p0 = self.position;    // The starting point

                        Vector3 internal_p2 = SwissArmyKnife.GetFlattedDownPosition(closestSurroundPoint.position, self.position);  // The destination

                        float internal_angle = Vector3.SignedAngle(internal_p2 - internal_p0, focusedPlayer.transform.position - internal_p0, Vector3.up);

                        int internal_moveSens = internal_angle > 1 ? 1 : -1;

                        Vector3 internal_p1 = internal_p0 + (internal_p2 - internal_p0) / 0.5f + Vector3.Cross(internal_p2 - internal_p0, Vector3.up) * internal_moveSens * bezierCurveHeight * internal_distanceToPointRatio;  // "third point" of the bezier curve

                        // Calculating position on bezier curve, following start point, end point and avancement
                        // In this version, the avancement has been replaced by a constant because it's recalculated every frame
                        Vector3 internal_positionOnBezierCurve = (Mathf.Pow(0.5f, 2) * internal_p0) + (2 * 0.5f * 0.5f * internal_p1) + (Mathf.Pow(0.5f, 2) * internal_p2);
                        navMeshAgent.SetDestination(SwissArmyKnife.GetFlattedDownPosition(internal_positionOnBezierCurve, focusedPlayer.position));
                    }
                    else
                    {
                        navMeshAgent.SetDestination(focusedPlayer.position);
                    }

                    if (distanceWithFocusedPlayer <= distanceToAttack)
                    {
                        ChangingState(EnemyState.PreparingAttack);
                    }
                }
                break;

            case EnemyState.Bumped:
                if (bumpTimeProgression < 1)
                {
                    bumpTimeProgression += Time.deltaTime / bumpDuration;

                    //must stop ?
                    int bumpRaycastMask = 1 << LayerMask.NameToLayer("Environment");
                    if (Physics.Raycast(self.position, bumpDirection, bumpRaycastDistance, bumpRaycastMask) && !mustCancelBump)
                    {
                        mustCancelBump = true;
                        bumpTimeProgression = whenToTriggerFallingAnim;
                    }

                    //move !
                    if (!mustCancelBump)
                    {
                        rb.MovePosition(Vector3.Lerp(bumpInitialPosition, bumpDestinationPosition, bumpDistanceCurve.Evaluate(bumpTimeProgression)));
                    }

                    //trigger end anim
                    if (bumpTimeProgression >= whenToTriggerFallingAnim && !fallingTriggerLaunched)
                    {
                        fallingTriggerLaunched = true;
                        animator.SetTrigger("FallingTrigger");

                        if (damageAfterBump > 0)
                        {
                            health -= damageAfterBump;
                        }
                    }
                }

                //when arrived on ground
                else if (restDuration > 0)
                {
                    if (health <= 0)
                    {
                        ChangingState(EnemyState.Dying);
                    }

                    restDuration -= Time.deltaTime;
                    if (restDuration <= 0)
                    {
                        animator.SetTrigger("StandingUpTrigger");
                    }
                }

                //time to get up
                else if (gettingUpDuration > 0)
                {
                    gettingUpDuration -= Time.deltaTime;
                    if (gettingUpDuration <= 0)
                        ChangingState(EnemyState.Following);
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
                    ChangingState(EnemyState.Idle);
                }
                break;

            case EnemyState.Dying:
                Die();
                break;
        }
    }

    public void ChangingState(EnemyState _newState)
    {
        ExitState();
        EnterState(_newState);
        EnemyState = _newState;
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
				onDeath.Invoke();
                break;
        }
    }

	public NavMeshAgent GetNavMeshAgent()
	{
		return navMeshAgent;
	}

    public virtual void EnterBumpedState()
    {
        transform.rotation = Quaternion.LookRotation(-bumpDirection);
        gettingUpDuration = maxGettingUpDuration;
        fallingTriggerLaunched = false;
        navMeshAgent.enabled = false;
        bumpTimeProgression = 0;
        bumpInitialPosition = transform.position;
        bumpDestinationPosition = transform.position + bumpDirection * bumpDistance;
        animator.SetTrigger("BumpTrigger");
        mustCancelBump = false;
    }

    public virtual void EnterPreparingAttackState()
    {
        navMeshAgent.enabled = false;
        anticipationTime = maxAnticipationTime;
        animator.SetTrigger("AttackTrigger");
    }

    public virtual void EnterAttackingState(string attackSound = "EnemyAttack")
    {
        SoundManager.PlaySound(attackSound, transform.position, transform);
        //attackDuration = maxAttackDuration;
        endOfAttackTriggerLaunched = false;
        attackInitialPosition = transform.position;
        attackDestination = attackInitialPosition + attackMaxDistance * transform.forward;
        attackTimeProgression = 0;
        myAttackHitBox = Instantiate(attackHitBoxPrefab, transform.position + hitBoxOffset.x * transform.right + hitBoxOffset.y * transform.up + hitBoxOffset.z * transform.forward, Quaternion.identity, transform);
        mustCancelAttack = false;
    }

    public virtual void PreparingAttackState()
    {
        Quaternion _targetRotation = Quaternion.LookRotation(focusedPlayer.position - transform.position);
        _targetRotation.eulerAngles = new Vector3(0, _targetRotation.eulerAngles.y, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, rotationSpeedPreparingAttack);
        anticipationTime -= Time.deltaTime;
        if (anticipationTime <= 0)
        {
            ChangingState(EnemyState.Attacking);
        }
    }

    public virtual void AttackingState()
    {
        attackTimeProgression += Time.deltaTime / maxAttackDuration;
        //attackDuration -= Time.deltaTime;

        //must stop ?
        int attackRaycastMask = 1 << LayerMask.NameToLayer("Environment");
        if (Physics.Raycast(self.position, self.forward, attackRaycastDistance, attackRaycastMask) && !mustCancelAttack)
        {
            attackTimeProgression = whenToTriggerEndOfAttackAnim;
            mustCancelAttack = true;
        }

        if (!mustCancelAttack)
        {
            rb.MovePosition(Vector3.Lerp(attackInitialPosition, attackDestination, attackSpeedCurve.Evaluate(attackTimeProgression)));
        }

        if (attackTimeProgression >= 1)
        {
            ChangingState(EnemyState.PauseAfterAttack);
        }
        else if (attackTimeProgression >= whenToTriggerEndOfAttackAnim && !endOfAttackTriggerLaunched)
        {
            endOfAttackTriggerLaunched = true;
            animator.SetTrigger("EndOfAttackTrigger");
        }
    }

    void ExitState()
    {
        switch (EnemyState)
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
        StartCoroutine(Staggered_C(whatBumps));
    }

    void UpdateDistancesToPlayers()
    {
        distanceWithPlayerOne = Vector3.Distance(self.position, playerOneTransform.position);
        distanceWithPlayerTwo = Vector3.Distance(self.position, playerTwoTransform.position);
        if (focusedPlayer != null)
            distanceWithFocusedPlayer = Vector3.Distance(self.position, focusedPlayer.position);
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
		SoundManager.PlaySound(hitSound, transform.position, transform);
		Vector3 internal_normalizedImpactVector;
		LockManager.UnlockTarget(this.transform);
        float internal_BumpDistanceMod = 0.5f;
        float internal_BumpDurationMod = 0.5f;
        float internal_BumpRestDurationMod = 0.5f;

        switch (_source)
        {
            case DamageSource.Dunk:
                if (isBumpable)
                {
                    damageAfterBump = _damages;
                    internal_normalizedImpactVector = new Vector3(_impactVector.x, 0, _impactVector.z);
                    if (_thrower.GetComponent<DunkController>() != null)
                    {
                        DunkController internal_controller = _thrower.GetComponent<DunkController>();
                        internal_BumpDistanceMod = internal_controller.bumpDistanceMod;
                        internal_BumpDurationMod = internal_controller.bumpDurationMod;
                        internal_BumpRestDurationMod = internal_controller.bumpRestDurationMod;
                    }
                    BumpMe(10, 1, 1, internal_normalizedImpactVector.normalized, internal_BumpDistanceMod, internal_BumpDurationMod, internal_BumpRestDurationMod);
                    whatBumps = WhatBumps.Dunk;
                }
                else
                {
                    health -= _damages;
                }
                break;

            case DamageSource.RedBarrelExplosion:
                if (isBumpable)
                {
                    damageAfterBump = _damages;
                    EnergyManager.IncreaseEnergy(energyAmount);
                    internal_normalizedImpactVector = new Vector3(_impactVector.x, 0, _impactVector.z);
                    if (_bumpModificators != default(Vector3))
                    {
                        internal_BumpDistanceMod = _bumpModificators.x;
                        internal_BumpDurationMod = _bumpModificators.y;
                        internal_BumpRestDurationMod = _bumpModificators.z;
                    }
                    BumpMe(10, 1, 1, internal_normalizedImpactVector.normalized, internal_BumpDistanceMod, internal_BumpDurationMod, internal_BumpRestDurationMod); 
                    whatBumps = WhatBumps.RedBarrel;
                }
                else
                {
                    health -= _damages;
                }
                break;

            case DamageSource.Ball:
                damageAfterBump = 0;
                FeedbackManager.SendFeedback("event.EnemyHitByBall", this);
				FeedbackManager.SendFeedback("event.BallTouchingEnemy", _ball);
				EnergyManager.IncreaseEnergy(energyAmount);
				whatBumps = WhatBumps.Pass;
                StartCoroutine(Staggered_C(whatBumps));
                health -= _damages;
                if (health <= 0)
                {
                    ChangingState(EnemyState.Dying);
                }
                break;
        }

        if (_ball)
            _ball.Explode(true);

            animator.SetTrigger("HitTrigger");
            GameObject internal_hitParticle = Instantiate(hitParticlePrefab, transform.position, Quaternion.identity);
            internal_hitParticle.transform.localScale *= hitParticleScale;
            Destroy(internal_hitParticle, 1f);
    }

    public virtual void Die(string deathSound = "EnemyDeath")
    {
        GameManager.i.enemyManager.enemies.Remove(this);

        if (deathSound != null)
        {
            SoundManager.PlaySound(deathSound, transform.position, transform);
        }
		LockManager.UnlockTarget(this.transform);
        GameObject internal_deathParticle = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        internal_deathParticle.transform.localScale *= deathParticleScale;
        Destroy(internal_deathParticle, 1.5f);
		if (Random.Range(0f, 1f) <= coreDropChances)
		{
			DropCore();
		}

        Destroy(gameObject);
    }

	protected void DropCore()
	{
		GameObject internal_newCore = Instantiate(Resources.Load<GameObject>("EnemyResource/EnemyCore"));
		internal_newCore.name = "Core of " + gameObject.name;
		internal_newCore.transform.position = transform.position;
		Vector3 internal_wantedDirectionAngle = SwissArmyKnife.RotatePointAroundPivot(Vector3.forward, Vector3.up, new Vector3(0, Random.Range(0,360), 0));
		float internal_throwForce = Random.Range(minMaxDropForce.x, minMaxDropForce.y);
		internal_wantedDirectionAngle.y = internal_throwForce * 0.035f;
		internal_newCore.GetComponent<CorePart>().Init(null, internal_wantedDirectionAngle.normalized * internal_throwForce, 1, (int)Random.Range(minMaxCoreHealthValue.x, minMaxCoreHealthValue.y));
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
    }

    public IEnumerator Staggered_C(WhatBumps? cause = default)
    {
        float internal_timeToRecover = 0.5f;

        switch (cause)
        {
            case WhatBumps.Pass:
                moveMultiplicator -= slowFromPass;
                internal_timeToRecover = timeToRecoverSlowFromPass;
                // Fetch ball datas => speed reduction on pass
                break;
            case WhatBumps.Dunk:
                moveMultiplicator -= slowFromDunk;
                internal_timeToRecover = timeToRecoverSlowFromDunk;
                // Fetch ball datas => speed reduction on dunk
                break;
            case WhatBumps.Environment:
                moveMultiplicator -= 0.5f;
                internal_timeToRecover = 0.5f;
                // Fetch environment datas => speed reduction
                break;
            default:
                moveMultiplicator -= 0.5f;
                internal_timeToRecover = 0.5f;
                Debug.Log("Default case: New speed multiplicator = 0.5");
                break;
        }

        float internal_time = 0;
        float internal_initialMoveMultiplicator = moveMultiplicator;

        while (moveMultiplicator < normalMoveMultiplicator)
        {
            moveMultiplicator = internal_initialMoveMultiplicator + (normalMoveMultiplicator - internal_initialMoveMultiplicator) * speedRecoverCurve.Evaluate(internal_time);
            internal_time += Time.deltaTime / internal_timeToRecover;
            print(moveMultiplicator);
            yield return null;
        }

        moveMultiplicator = normalMoveMultiplicator;
    }

    public virtual void BumpMe(float _bumpDistance, float _bumpDuration, float _restDuration,  Vector3 _bumpDirection,  float randomDistanceMod, float randomDurationMod, float randomRestDurationMod)
    {
		FeedbackManager.SendFeedback("event.EnemyBumpedAway", this);
		SoundManager.PlaySound("EnemiesBumpAway", transform.position, transform);
		bumpDistance = _bumpDistance + Random.Range(-randomDistanceMod, randomDistanceMod);
        bumpDuration = _bumpDuration + Random.Range(-randomDurationMod, randomDurationMod);
        restDuration = _restDuration + Random.Range(-randomRestDurationMod, randomRestDurationMod);
        bumpDirection = _bumpDirection;
        ChangingState(EnemyState.Bumped);
    }

    IEnumerator WaitABit_C(float _duration)
    {
        yield return new WaitForSeconds(_duration);
        ChangingState(EnemyState.Following);
    }
}
