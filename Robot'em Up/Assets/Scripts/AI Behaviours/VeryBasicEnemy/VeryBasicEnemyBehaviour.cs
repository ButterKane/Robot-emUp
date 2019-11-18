using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using UnityEngine.AI;

public enum VeryBasicEnemyState
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

public class VeryBasicEnemyBehaviour : MonoBehaviour,IHitable
{
    VeryBasicEnemyState State = VeryBasicEnemyState.Idle;

    [Separator("References")]
    [SerializeField] private Transform _self;
    public Rigidbody Rb;
    public Animator Animator;
    public NavMeshAgent navMeshAgent;

    [Space(2)]
    [Separator("Auto-assigned References")]
    public Transform Target;
    [SerializeField] private Transform _playerOne;
    [SerializeField] private Transform _playerTwo;


    [Space(2)]
    [Separator("Tweakable variables")]
    bool playerOneInRange;
    bool playerTwoInRange;
    public int MaxHealth = 100;
    public int Health;
    float distanceWithPlayerOne;
    float distanceWithPlayerTwo;
    float distanceWithFocusedPlayer;
    Transform focusedPlayer = null;
	public float energyAmount;

	[Space(2)]
    [Header("Focus")]
    public float focusDistance;
    public float unfocusDistance;
    public float timeBetweenCheck;
    public float distanceBeforeChangingPriority;

    [Space(2)]
    [Header("Attack")]
    public float distanceToAttack;
    public float maxAnticipationTime;
    [Range(0, 1)]public float rotationSpeedPreparingAttack;
    float anticipationTime;
    public float attackMaxDistance;
    public float maxAttackDuration;
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
    public float maxTimePauseAfterAttack;
    float timePauseAfterAttack;
    public float attackRaycastDistance;
    bool mustCancelAttack;

    [Space(2)]
    [Header("Bump")]
    public float maxGettingUpDuration;
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
    public float bumpRaycastDistance;
    bool mustCancelBump;


    [Space(2)]
    [Header("FX References")]
    public GameObject deathParticlePrefab;
    public float deathParticleScale;
    public GameObject hitParticlePrefab;
    public float hitParticleScale;

    //-----------------------------------------
    public int hitCount { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //-----------------------------------------

    void Start()
    {
        Health = MaxHealth;
        _self = transform;
        _playerOne = GameManager.i.playerOne.transform;
        _playerTwo = GameManager.i.playerTwo.transform;
        State = VeryBasicEnemyState.Following;
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
            case VeryBasicEnemyState.Idle:
                break;
            case VeryBasicEnemyState.Following:
                if(focusedPlayer != null)
                {
                    navMeshAgent.SetDestination(focusedPlayer.position);
                    if (distanceWithFocusedPlayer <= distanceToAttack)
                    {
                        ChangingState(VeryBasicEnemyState.PreparingAttack);
                    }
                }
                break;
            case VeryBasicEnemyState.Staggering:
                break;
            case VeryBasicEnemyState.Bumped:

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
                else if(gettingUpDuration>0)
                {
                    gettingUpDuration -= Time.deltaTime;
                    if(gettingUpDuration<=0)
                        ChangingState(VeryBasicEnemyState.Following);
                }
                break;
            case VeryBasicEnemyState.ChangingFocus:
                break;
            case VeryBasicEnemyState.PreparingAttack:
                Quaternion _targetRotation = Quaternion.LookRotation(focusedPlayer.position - transform.position);
                _targetRotation.eulerAngles = new Vector3(0, _targetRotation.eulerAngles.y, 0);
                transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, rotationSpeedPreparingAttack);
                anticipationTime -= Time.deltaTime;
                if (anticipationTime <= 0)
                {
                    ChangingState(VeryBasicEnemyState.Attacking);
                }
                break;
            case VeryBasicEnemyState.Attacking:

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

                if (attackTimeProgression >=1)
                {
                    ChangingState(VeryBasicEnemyState.PauseAfterAttack);
                }
                else if(attackTimeProgression >= whenToTriggerEndOfAttackAnim && !endOfAttackTriggerLaunched)
                {
                    endOfAttackTriggerLaunched = true;
                    Animator.SetTrigger("EndOfAttackTrigger");
                }
                break;
            case VeryBasicEnemyState.PauseAfterAttack:
                timePauseAfterAttack -= Time.deltaTime;
                if(timePauseAfterAttack <= 0)
                {
                    ChangingState(VeryBasicEnemyState.Following);
                }
                break;
            case VeryBasicEnemyState.Dying:
                break;
        }
    }

    public void ChangingState(VeryBasicEnemyState _newState)
    {
        ExitState();
        EnterState(_newState);
        State = _newState;
    }

    void EnterState(VeryBasicEnemyState _newState)
    {
        print(_newState);
        switch (_newState)
        {
            case VeryBasicEnemyState.Idle:
                break;
            case VeryBasicEnemyState.Following:
                navMeshAgent.enabled = true;
                break;
            case VeryBasicEnemyState.Staggering:
                break;
            case VeryBasicEnemyState.Bumped:
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
            case VeryBasicEnemyState.ChangingFocus:
                break;
            case VeryBasicEnemyState.PreparingAttack:
                navMeshAgent.enabled = false;
                anticipationTime = maxAnticipationTime;
                Animator.SetTrigger("AttackTrigger");
                break;
            case VeryBasicEnemyState.Attacking:
                attackDuration = maxAttackDuration;
                endOfAttackTriggerLaunched = false;
                attackInitialPosition = transform.position;
                attackDestination = attackInitialPosition + attackMaxDistance*transform.forward;
                attackTimeProgression = 0;
                myAttackHitBox = Instantiate(attackHitBoxPrefab, transform.position + hitBoxOffset.x * transform.right + hitBoxOffset.y*transform.up + hitBoxOffset.z*transform.forward, Quaternion.identity, transform);
                mustCancelAttack = false;
                break;
            case VeryBasicEnemyState.PauseAfterAttack:
                timePauseAfterAttack = maxTimePauseAfterAttack;
                break;
            case VeryBasicEnemyState.Dying:
                break;
        }
    }

    void ExitState()
    {
        switch (State)
        {
            case VeryBasicEnemyState.Idle:
                break;
            case VeryBasicEnemyState.Following:
                break;
            case VeryBasicEnemyState.Staggering:
                break;
            case VeryBasicEnemyState.Bumped:
                break;
            case VeryBasicEnemyState.ChangingFocus:
                break;
            case VeryBasicEnemyState.PreparingAttack:
                break;
            case VeryBasicEnemyState.Attacking:
                Destroy(myAttackHitBox);
                break;
            case VeryBasicEnemyState.PauseAfterAttack:
                break;
            case VeryBasicEnemyState.Dying:
                break;
        }
    }

    void UpdateDistancesToPlayers()
    {
        distanceWithPlayerOne = Vector3.Distance(_self.position, _playerOne.position);
        distanceWithPlayerTwo = Vector3.Distance(_self.position, _playerTwo.position);
        if(focusedPlayer != null)
            distanceWithFocusedPlayer = Vector3.Distance(_self.position, focusedPlayer.position);
    }

    Transform GetClosestPlayer()
    {
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
		switch (_source)
		{
			case DamageSource.Dunk:
				Vector3 normalizedImpactVector = new Vector3(_impactVector.x, 0, _impactVector.z);
				BumpMe(10, 1, 1, normalizedImpactVector.normalized);
				break;
		}
        Health -= _damages;
        _ball.Explode(true);
        if (Health <= 0)
        {
            Die();
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

    void Die()
    {
        GameObject deathParticle = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        deathParticle.transform.localScale *= deathParticleScale;
        Destroy(deathParticle, 1.5f);
        Destroy(gameObject);
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
            if (focusedPlayer == _playerOne && distanceWithPlayerOne - distanceWithPlayerTwo > distanceBeforeChangingPriority)
            {
                ChangingFocus(_playerTwo);
            }
            else if (focusedPlayer == _playerTwo && distanceWithPlayerTwo - distanceWithPlayerOne > distanceBeforeChangingPriority)
            {
                ChangingFocus(_playerOne);
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
        ChangingState(VeryBasicEnemyState.Bumped);
    }

}
