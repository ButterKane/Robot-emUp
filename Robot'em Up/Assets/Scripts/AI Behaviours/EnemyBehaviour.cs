using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using UnityEngine.AI;


public enum EnemyState
{
    Idle,
    Following,
    Staggering,
    Bumped,
    ChangingFocus,
    PreparingAttack,
    Attacking,
    PauseAfterAttack,
    Dying,
}

public class EnemyBehaviour : MonoBehaviour, IHitable
{
    public EnemyState State = EnemyState.Idle;

    [Separator("References")]
    [SerializeField] private Transform _self;
    public Rigidbody Rb;
    public Animator Animator;
    public NavMeshAgent navMeshAgent;

    [Space(2)]
    [Separator("Auto-assigned References")]
    [SerializeField] private Transform _playerOne;
    private PawnController _playerOneController;
    [SerializeField] private Transform _playerTwo;
    private PawnController _playerTwoController;


    [Space(2)]
    [Separator("Tweakable variables")]
    bool playerOneInRange;
    bool playerTwoInRange;
    public int MaxHealth = 30;
    public int Health;
    float distanceWithPlayerOne;
    float distanceWithPlayerTwo;
    float distanceWithFocusedPlayer;
    public Transform focusedPlayer = null;
    public float energyAmount = 1;

    [Space(2)]
    [Header("Focus")]
    public float focusDistance = 18;
    public float unfocusDistance = 20;
    public float timeBetweenCheck = 0.25f;
    public float distanceBeforeChangingPriority = 3;

    [Space(2)]
    [Header("Attack")]
    public float distanceToAttack = 5;
    public float maxAnticipationTime = 0.5f;
    [Range(0, 1)] public float rotationSpeedPreparingAttack = 0.2f;
    float anticipationTime;
    public float attackMaxDistance = 8;
    public float maxAttackDuration = 0.5f;
    float attackDuration;
    float attackTimeProgression;
    public AnimationCurve attackSpeedCurve;
    Vector3 attackInitialPosition;
    Vector3 attackDestination;
    [Range(0, 1)] public float whenToTriggerEndOfAttackAnim;
    bool endOfAttackTriggerLaunched;
    public GameObject attackHitBoxPrefab;
    GameObject myAttackHitBox;
    public Vector3 hitBoxOffset;
    public float maxTimePauseAfterAttack = 1;
    float timePauseAfterAttack;
    public float attackRaycastDistance = 2;
    bool mustCancelAttack;

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
    public Transform ClosestSurroundPoint;
    [Range(0, 1)]
    public float BezierCurveHeight = 0.5f;
    public float BezierDistanceToHeightRatio = 100f;

    [Space(2)]
    [Header("Death")]
    public float coreDropChances = 1;
	public Vector2 minMaxCoreHealthValue = new Vector2(1, 3);

    void Start()
    {
        Health = MaxHealth;
        _self = transform;
        _playerOne = GameManager.i.playerOne.transform;
        _playerTwo = GameManager.i.playerTwo.transform;
        _playerOneController = _playerOne.GetComponent<PlayerController>();
        _playerTwoController = _playerTwo.GetComponent<PlayerController>();
        GameManager.i.enemyManager.enemies.Add(this);
        State = EnemyState.Following;
        StartCoroutine(CheckDistanceAndAdaptFocus());
    }

    void Update()
    {
        UpdateDistancesToPlayers();
        UpdateState();
        UpdateAnimatorBlendTree();
        if (Input.GetKeyDown(KeyCode.P))
        {
            BumpMe(8, .35f, 1, -transform.forward);
        }
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
                break;
            case EnemyState.Following:
                if (focusedPlayer != null)
                {
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
            case EnemyState.Staggering:
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
                Quaternion _targetRotation = Quaternion.LookRotation(focusedPlayer.position - transform.position);
                _targetRotation.eulerAngles = new Vector3(0, _targetRotation.eulerAngles.y, 0);
                transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, rotationSpeedPreparingAttack);
                anticipationTime -= Time.deltaTime;
                if (anticipationTime <= 0)
                {
                    ChangingState(EnemyState.Attacking);
                }
                break;
            case EnemyState.Attacking:
                AttackingState();
                
                break;
            case EnemyState.PauseAfterAttack:
                timePauseAfterAttack -= Time.deltaTime;
                if (timePauseAfterAttack <= 0)
                {
                    ChangingState(EnemyState.Following);
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
        print(_newState);
        switch (_newState)
        {
            case EnemyState.Idle:
                break;
            case EnemyState.Following:
                navMeshAgent.enabled = true;
                break;
            case EnemyState.Staggering:
                break;
            case EnemyState.Bumped:
                transform.rotation = Quaternion.LookRotation(-bumpDirection);
                gettingUpDuration = maxGettingUpDuration;
                fallingTriggerLaunched = false;
                navMeshAgent.enabled = false;
                bumpTimeProgression = 0;
                bumpInitialPosition = transform.position;
                bumpDestinationPosition = transform.position + bumpDirection * bumpDistance;
                Animator.SetTrigger("BumpTrigger");
                mustCancelBump = false;
                break;
            case EnemyState.ChangingFocus:
                break;
            case EnemyState.PreparingAttack:
                navMeshAgent.enabled = false;
                anticipationTime = maxAnticipationTime;
                Animator.SetTrigger("AttackTrigger");
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

    public virtual void EnterAttackingState()
    {
        attackDuration = maxAttackDuration;
        endOfAttackTriggerLaunched = false;
        attackInitialPosition = transform.position;
        attackDestination = attackInitialPosition + attackMaxDistance * transform.forward;
        attackTimeProgression = 0;
        myAttackHitBox = Instantiate(attackHitBoxPrefab, transform.position + hitBoxOffset.x * transform.right + hitBoxOffset.y * transform.up + hitBoxOffset.z * transform.forward, Quaternion.identity, transform);
        mustCancelAttack = false;
    }

    public virtual void AttackingState()
    {
        attackTimeProgression += Time.deltaTime / attackDuration;
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
            case EnemyState.Staggering:
                break;
            case EnemyState.Bumped:
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

    void UpdateDistancesToPlayers()
    {
        distanceWithPlayerOne = Vector3.Distance(_self.position, _playerOne.position);
        distanceWithPlayerTwo = Vector3.Distance(_self.position, _playerTwo.position);
        if (focusedPlayer != null)
            distanceWithFocusedPlayer = Vector3.Distance(_self.position, focusedPlayer.position);
    }

    Transform GetClosestPlayer()
    {
        if (!_playerOneController.IsTargetable())
        {
            return _playerTwo;
        }

        if (!_playerTwoController.IsTargetable())
        {
            return _playerOne;
        }

        if (distanceWithPlayerOne >= distanceWithPlayerTwo)
        {
            return _playerTwo;
        }
        else
        {
            return _playerOne;
        }
    }

    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source)
    {
        Vector3 normalizedImpactVector;
        switch (_source)
        {
            case DamageSource.Dunk:
                normalizedImpactVector = new Vector3(_impactVector.x, 0, _impactVector.z);
                BumpMe(10, 1, 1, normalizedImpactVector.normalized);
                break;
            case DamageSource.RedBarrelExplosion:
                normalizedImpactVector = new Vector3(_impactVector.x, 0, _impactVector.z);
                BumpMe(10, 1, 1, normalizedImpactVector.normalized);
                break;

        }
        Health -= _damages;

        if (_ball)
            _ball.Explode(true);

        if (Health <= 0)
        {
            State = EnemyState.Dying;
        }
        else
        {
            Animator.SetTrigger("HitTrigger");
            GameObject hitParticle = Instantiate(hitParticlePrefab, transform.position, Quaternion.identity);
            hitParticle.transform.localScale *= hitParticleScale;
            Destroy(hitParticle, 1f);
        }
        EnergyManager.IncreaseEnergy(energyAmount);
    }

    protected virtual void Die()
    {
        GameObject deathParticle = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        deathParticle.transform.localScale *= deathParticleScale;
        Destroy(deathParticle, 1.5f);
		if (Random.Range(0f, 1f) <= coreDropChances)
		{
			DropCore();
		}	
		Destroy(gameObject);
    }

	void DropCore()
	{
		GameObject newCore = Instantiate(Resources.Load<GameObject>("EnemyResource/EnemyCore"));
		newCore.name = "Core of " + gameObject.name;
		newCore.transform.position = transform.position;
		Vector3 wantedDirectionAngle = SwissArmyKnife.RotatePointAroundPivot(Vector3.forward, Vector3.up, new Vector3(0, Random.Range(0,360), 0));
		float throwForce = Random.Range(10, 17);
		wantedDirectionAngle.y = throwForce * 0.035f;
		newCore.GetComponent<CorePart>().Init(null, wantedDirectionAngle.normalized * throwForce, 1, (int)Random.Range(minMaxCoreHealthValue.x, minMaxCoreHealthValue.y));
	}

    IEnumerator CheckDistanceAndAdaptFocus()
    {
        //Checking who is in range
        if (distanceWithPlayerOne < focusDistance)
        {
            playerOneInRange = true;
        }
        else
        {
            playerOneInRange = false;
        }

        if (distanceWithPlayerTwo < focusDistance)
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
            if ((focusedPlayer == _playerOne && distanceWithPlayerOne > unfocusDistance) || (focusedPlayer == _playerTwo && distanceWithPlayerTwo > unfocusDistance))
            {
                ChangingFocus(null);
            }
        }

        //Changing focus between the two
        if (playerOneInRange && playerTwoInRange && focusedPlayer != null)
        {
            if (_playerTwoController.IsTargetable())
            {
                if (focusedPlayer == _playerOne && distanceWithPlayerOne - distanceWithPlayerTwo > distanceBeforeChangingPriority)
                {
                    ChangingFocus(_playerTwo);
                }
            }
            else if (_playerOneController.IsTargetable())
            {
                if (focusedPlayer == _playerTwo && distanceWithPlayerTwo - distanceWithPlayerOne > distanceBeforeChangingPriority)
                {
                    ChangingFocus(_playerOne);
                }
            }
        }

        //no focused yet ? Choose one
        if ((playerOneInRange || playerTwoInRange) && focusedPlayer == null)
        {
            ChangingFocus(GetClosestPlayer());
        }

        //Restart coroutine in X seconds
        yield return new WaitForSeconds(timeBetweenCheck);
        StartCoroutine(CheckDistanceAndAdaptFocus());
    }

    void ChangingFocus(Transform _newFocus)
    {
        focusedPlayer = _newFocus;
    }

    public void BumpMe(float _bumpDistance, float _bumpDuration, float _restDuration, Vector3 _bumpDirection)
    {
        bumpDistance = _bumpDistance;
        bumpDuration = _bumpDuration;
        restDuration = _restDuration;
        bumpDirection = _bumpDirection;
        ChangingState(EnemyState.Bumped);
    }

}
