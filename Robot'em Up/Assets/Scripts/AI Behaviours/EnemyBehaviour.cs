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
    [System.NonSerialized] public EnemyState State = EnemyState.Idle;

    [Separator("References")]
    [SerializeField] protected Transform _self;
    public Rigidbody Rb;
    public Animator Animator;
    public NavMeshAgent navMeshAgent;
    public Transform HealthBarRef;
    public GameObject HealthBarPrefab;

	[Space(2)]
    [Separator("Auto-assigned References")]
    [SerializeField] private Transform _playerOneTransform;
    private PawnController _playerOnePawnController;
    [SerializeField] private Transform _playerTwoTransform;
    private PawnController _playerTwoPawnController;


    [Space(2)]
    [Separator("Tweakable variables")]
    bool playerOneInRange;
    bool playerTwoInRange;
    public int MaxHealth = 30;
    [System.NonSerialized] public int Health;
    float distanceWithPlayerOne;
    float distanceWithPlayerTwo;
    float distanceWithFocusedPlayer;
    [System.NonSerialized] public Transform focusedPlayer = null;
    public float energyAmount = 1;
    public int damage = 10;
	public float powerLevel = 1;
    [SerializeField] private bool _lockable; public bool lockable { get { return _lockable; } set { _lockable = value; } }
	[SerializeField] private float _lockHitboxSize; public float lockHitboxSize { get { return _lockHitboxSize; } set { _lockHitboxSize = value; } }
	public bool arenaRobot;

	[Space(2)]
    [Header("Focus")]
    public float focusDistance = 18;
    public float unfocusDistance = 20;
    float timeBetweenCheck = 0;
    public float distanceBeforeChangingPriority = 3;
    public float maxTimeBetweenCheck = 0.25f;

    [Space(2)]
    [Header("Movement")]
    public float NormalSpeed = 7; // This value is the one in the inspector, but in practice it is modified by the Random speed mod
    [System.NonSerialized] public float ActualSpeed;
    public float NormalAcceleration = 30;
    public float RandomSpeedMod;
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

    [Space(2)]
    [Header("FX References")]
    public GameObject deathParticlePrefab;
    public float deathParticleScale = 2;
    public GameObject hitParticlePrefab;
    public float hitParticleScale = 3;

    [Space(2)]
    [Header("Surrounding")]
    [System.NonSerialized] public Transform ClosestSurroundPoint;
    [Range(0, 1)]
    public float BezierCurveHeight = 0.5f;
    public float BezierDistanceToHeightRatio = 100f;

    [Space(2)]
    [Header("Death")]
    public float coreDropChances = 1;
    public Vector2 minMaxDropForce;
	public Vector2 minMaxCoreHealthValue = new Vector2(1, 3);
    [System.NonSerialized] public UnityEvent onDeath = new UnityEvent();

    

    protected void Start()
    {
        Health = MaxHealth;
        _self = transform;
        ActualSpeed = NormalSpeed + Random.Range(-RandomSpeedMod, RandomSpeedMod);
        timeBetweenCheck = maxTimeBetweenCheck;
        _playerOneTransform = GameManager.playerOne.transform;
        _playerTwoTransform = GameManager.playerTwo.transform;
        _playerOnePawnController = _playerOneTransform.GetComponent<PlayerController>();
        _playerTwoPawnController = _playerTwoTransform.GetComponent<PlayerController>();
        GameManager.i.enemyManager.enemies.Add(this);
        GameObject healthBar = Instantiate(HealthBarPrefab, CanvasManager.i.MainCanvas.transform);
        healthBar.GetComponent<EnemyHealthBar>().Enemy = this;

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
        if (Animator != null)
        {
            Animator.SetFloat("IdleRunBlend", navMeshAgent.velocity.magnitude / navMeshAgent.speed);
        }
    }

    void UpdateState()
    {
        switch (State)
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

                    if (ClosestSurroundPoint != null)
                    {
                        float distanceToPointRatio = (1 + (_self.position - ClosestSurroundPoint.position).magnitude / BezierDistanceToHeightRatio);  // widens the arc of surrounding the farther the surroundingPoint is

                        Vector3 p0 = _self.position;    // The starting point

                        Vector3 p2 = SwissArmyKnife.GetFlattedDownPosition(ClosestSurroundPoint.position, _self.position);  // The destination

                        float angle = Vector3.SignedAngle(p2 - p0, focusedPlayer.transform.position - p0, Vector3.up);

                        int moveSens = angle > 1 ? 1 : -1;

                        Vector3 p1 = p0 + (p2 - p0) / 0.5f + Vector3.Cross(p2 - p0, Vector3.up) * moveSens * BezierCurveHeight * distanceToPointRatio;  // "third point" of the bezier curve

                        // Calculating position on bezier curve, following start point, end point and avancement
                        // In this version, the avancement has been replaced by a constant because it's recalculated every frame
                        Vector3 positionOnBezierCurve = (Mathf.Pow(0.5f, 2) * p0) + (2 * 0.5f * 0.5f * p1) + (Mathf.Pow(0.5f, 2) * p2);
                        navMeshAgent.SetDestination(SwissArmyKnife.GetFlattedDownPosition(positionOnBezierCurve, focusedPlayer.position));
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

                //isBeingBumped !
                if (bumpTimeProgression < 1)
                {
                    bumpTimeProgression += Time.deltaTime / bumpDuration;

                    //must stop ?
                    int bumpRaycastMask = 1 << LayerMask.NameToLayer("Environment");
                    if (Physics.Raycast(_self.position, bumpDirection, bumpRaycastDistance, bumpRaycastMask) && !mustCancelBump)
                    {
                        mustCancelBump = true;
                        bumpTimeProgression = whenToTriggerFallingAnim;
                    }

                    //move !
                    if (!mustCancelBump)
                    {
                        Rb.MovePosition(Vector3.Lerp(bumpInitialPosition, bumpDestinationPosition, bumpDistanceCurve.Evaluate(bumpTimeProgression)));
                    }

                    //trigger end anim
                    if (bumpTimeProgression >= whenToTriggerFallingAnim && !fallingTriggerLaunched)
                    {
                        fallingTriggerLaunched = true;
                        Animator.SetTrigger("FallingTrigger");
                    }
                }

                //when arrived on ground
                else if (restDuration > 0)
                {
                    restDuration -= Time.deltaTime;
                    if (restDuration <= 0)
                    {
                        Animator.SetTrigger("StandingUpTrigger");
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
        State = _newState;
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
        Animator.SetTrigger("BumpTrigger");
        mustCancelBump = false;
    }

    public virtual void EnterPreparingAttackState()
    {
        navMeshAgent.enabled = false;
        anticipationTime = maxAnticipationTime;
        Animator.SetTrigger("AttackTrigger");
    }

    public virtual void EnterAttackingState()
    {
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
        if (Physics.Raycast(_self.position, _self.forward, attackRaycastDistance, attackRaycastMask) && !mustCancelAttack)
        {
            attackTimeProgression = whenToTriggerEndOfAttackAnim;
            mustCancelAttack = true;
        }

        if (!mustCancelAttack)
        {
            Rb.MovePosition(Vector3.Lerp(attackInitialPosition, attackDestination, attackSpeedCurve.Evaluate(attackTimeProgression)));
        }

        if (attackTimeProgression >= 1)
        {
            ChangingState(EnemyState.PauseAfterAttack);
        }
        else if (attackTimeProgression >= whenToTriggerEndOfAttackAnim && !endOfAttackTriggerLaunched)
        {
            endOfAttackTriggerLaunched = true;
            Animator.SetTrigger("EndOfAttackTrigger");
        }
    }

    void ExitState()
    {
        switch (State)
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
        StartCoroutine(StaggeredCo(whatBumps));
    }

    void UpdateDistancesToPlayers()
    {
        distanceWithPlayerOne = Vector3.Distance(_self.position, _playerOneTransform.position);
        distanceWithPlayerTwo = Vector3.Distance(_self.position, _playerTwoTransform.position);
        if (focusedPlayer != null)
            distanceWithFocusedPlayer = Vector3.Distance(_self.position, focusedPlayer.position);
    }

    Transform GetClosestAndAvailablePlayer()
    {
        if ((distanceWithPlayerOne >= distanceWithPlayerTwo && _playerTwoPawnController.IsTargetable())
            || !_playerOnePawnController.IsTargetable())
        {
            return _playerTwoTransform;
        }
        else if ((distanceWithPlayerTwo >= distanceWithPlayerOne && _playerOnePawnController.IsTargetable())
            || !_playerTwoPawnController.IsTargetable())
        {
            return _playerOneTransform;
        }
        else
        {
            return null;
        }
    }

    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source, Vector3 _bumpModificators = default(Vector3))
    {
		SoundManager.PlaySound("EnemyHit", transform.position, transform);
		Vector3 normalizedImpactVector;
		LockManager.UnlockTarget(this.transform);
        float BumpDistanceMod = 0.5f;
        float BumpDurationMod = 0.5f;
        float BumpRestDurationMod = 0.5f;
        switch (_source)
        {
            case DamageSource.Dunk:
                normalizedImpactVector = new Vector3(_impactVector.x, 0, _impactVector.z);

                if (_thrower.GetComponent<DunkController>() != null)
                {
                    DunkController controller = _thrower.GetComponent<DunkController>();
                    BumpDistanceMod = controller.BumpDistanceMod;
                    BumpDurationMod = controller.BumpDurationMod;
                    BumpRestDurationMod = controller.BumpRestDurationMod;
                }

                BumpMe(10, 1, 1, normalizedImpactVector.normalized, BumpDistanceMod, BumpDurationMod, BumpRestDurationMod); 
                whatBumps = WhatBumps.Dunk;
                break;
            case DamageSource.RedBarrelExplosion:
				EnergyManager.IncreaseEnergy(energyAmount);
				normalizedImpactVector = new Vector3(_impactVector.x, 0, _impactVector.z);
                if (_bumpModificators != default(Vector3))
                {
                    BumpDistanceMod = _bumpModificators.x;
                    BumpDurationMod = _bumpModificators.y;
                    BumpRestDurationMod = _bumpModificators.z;
                }
                BumpMe(10, 1, 1, normalizedImpactVector.normalized, BumpDistanceMod, BumpDurationMod, BumpRestDurationMod);    // Need Explosion Data
                whatBumps = WhatBumps.RedBarrel;
                break;
            case DamageSource.Ball:
				FeedbackManager.SendFeedback("event.EnemyHitByBall", this);
				FeedbackManager.SendFeedback("event.BallTouchingEnemy", _ball);
				EnergyManager.IncreaseEnergy(energyAmount);
				whatBumps = WhatBumps.Pass;
                StartCoroutine(StaggeredCo(whatBumps));
				break;
        }
        Health -= _damages;

        if (_ball)
            _ball.Explode(true);

        if (Health <= 0)
        {
            ChangingState(EnemyState.Dying);
        }
        else
        {
            Animator.SetTrigger("HitTrigger");
            GameObject hitParticle = Instantiate(hitParticlePrefab, transform.position, Quaternion.identity);
            hitParticle.transform.localScale *= hitParticleScale;
            Destroy(hitParticle, 1f);
        }
    }

    protected virtual void Die()
    {
		LockManager.UnlockTarget(this.transform);
        GameObject deathParticle = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        deathParticle.transform.localScale *= deathParticleScale;
        Destroy(deathParticle, 1.5f);
		if (Random.Range(0f, 1f) <= coreDropChances)
		{
			DropCore();
		}
        GameManager.i.enemyManager.enemies.Remove(this);
		//onDeath.Invoke();
        Destroy(gameObject);
    }

	void DropCore()
	{
		GameObject newCore = Instantiate(Resources.Load<GameObject>("EnemyResource/EnemyCore"));
		newCore.name = "Core of " + gameObject.name;
		newCore.transform.position = transform.position;
		Vector3 wantedDirectionAngle = SwissArmyKnife.RotatePointAroundPivot(Vector3.forward, Vector3.up, new Vector3(0, Random.Range(0,360), 0));
		float throwForce = Random.Range(minMaxDropForce.x, minMaxDropForce.y);
		wantedDirectionAngle.y = throwForce * 0.035f;
		newCore.GetComponent<CorePart>().Init(null, wantedDirectionAngle.normalized * throwForce, 1, (int)Random.Range(minMaxCoreHealthValue.x, minMaxCoreHealthValue.y));
	}

    void CheckDistanceAndAdaptFocus()
    {
        //print(focusedPlayer);
        //Checking who is in range
        if (distanceWithPlayerOne < focusDistance && _playerOnePawnController.IsTargetable())
        {
            playerOneInRange = true;
        }
        else
        {
            playerOneInRange = false;
        }

        if (distanceWithPlayerTwo < focusDistance && _playerTwoPawnController.IsTargetable())
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
            if ((focusedPlayer == _playerOneTransform && (distanceWithPlayerOne > unfocusDistance || !_playerOnePawnController.IsTargetable()))
                || ((focusedPlayer == _playerTwoTransform && (distanceWithPlayerTwo > unfocusDistance || !_playerTwoPawnController.IsTargetable()))))
            {
                ChangingFocus(null);
            }
        }

        //Changing focus between the two
        if ((playerOneInRange && _playerOnePawnController.IsTargetable())
            && (playerTwoInRange && _playerTwoPawnController.IsTargetable())
            && focusedPlayer != null)
        {
            if (focusedPlayer == _playerOneTransform && distanceWithPlayerOne - distanceWithPlayerTwo > distanceBeforeChangingPriority)
            {
                ChangingFocus(_playerTwoTransform);
            }
            else if (focusedPlayer == _playerTwoTransform && distanceWithPlayerTwo - distanceWithPlayerOne > distanceBeforeChangingPriority)
            {
                ChangingFocus(_playerOneTransform);
            }
        }

        //no focused yet ? Choose one
        if (((playerOneInRange && _playerOnePawnController.IsTargetable())
            || (playerTwoInRange && _playerTwoPawnController.IsTargetable()))
            && focusedPlayer == null)
        {
            ChangingFocus(GetClosestAndAvailablePlayer());
        }
    }

    void ChangingFocus(Transform _newFocus)
    {
        focusedPlayer = _newFocus;
    }

    public IEnumerator StaggeredCo(WhatBumps? cause = default)
    {
        float _timeToRecover = 0.5f;
        switch (cause)
        {
            case WhatBumps.Pass:
                moveMultiplicator -= slowFromPass;
                _timeToRecover = timeToRecoverSlowFromPass;
                // Fetch ball datas => speed reduction on pass
                break;
            case WhatBumps.Dunk:
                moveMultiplicator -= slowFromDunk;
                _timeToRecover = timeToRecoverSlowFromDunk;
                // Fetch ball datas => speed reduction on dunk
                break;
            case WhatBumps.Environment:
                moveMultiplicator -= 0.5f;
                _timeToRecover = 0.5f;
                // Fetch environment datas => speed reduction
                break;
            default:
                moveMultiplicator -= 0.5f;
                _timeToRecover = 0.5f;
                Debug.Log("Default case: New speed multiplicator = 0.5");
                break;
        }

        float _t = 0;
        float _initialMoveMultiplicator = moveMultiplicator;

        while (moveMultiplicator < normalMoveMultiplicator)
        {
            moveMultiplicator = _initialMoveMultiplicator + (normalMoveMultiplicator - _initialMoveMultiplicator) * speedRecoverCurve.Evaluate(_t);
            _t += Time.deltaTime / _timeToRecover;
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

    IEnumerator WaitABit(float _duration)
    {
        yield return new WaitForSeconds(_duration);
        ChangingState(EnemyState.Following);
    }
}
