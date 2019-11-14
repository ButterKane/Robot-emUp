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
    
    public float focusDistance;
    public float unfocusDistance;
    public float timeBetweenCheck;
    public float distanceBeforeChangingPriority;

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
    [Range(0, 1)] public float whenToTriggerNextAnim;
    public GameObject attackHitBoxPrefab;
    GameObject myAttackHitBox;
    public Vector3 hitBoxOffset;

    public float maxTimePauseAfterAttack;
    float timePauseAfterAttack;


    void Start()
    {
        Health = MaxHealth;
        _self = transform;
        _playerOne = GameManager.i.playerOne.transform;
        _playerTwo = GameManager.i.playerTwo.transform;
        State = VeryBasicEnemyState.Following;
        StartCoroutine(CheckDistance());
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
                Rb.MovePosition(Vector3.Lerp(attackInitialPosition, attackDestination, attackSpeedCurve.Evaluate(attackTimeProgression)));
                attackDuration -= Time.deltaTime;
                if(attackDuration <= 0)
                {
                    ChangingState(VeryBasicEnemyState.PauseAfterAttack);
                }
                else if(attackDuration <= whenToTriggerNextAnim)
                {
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
        State = _newState;
        EnterState();
    }

    void EnterState()
    {
        //print(State);
        switch (State)
        {
            case VeryBasicEnemyState.Idle:
                break;
            case VeryBasicEnemyState.Following:
                navMeshAgent.enabled = true;
                break;
            case VeryBasicEnemyState.Staggering:
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
                attackInitialPosition = transform.position;
                attackDestination = attackInitialPosition + attackMaxDistance*transform.forward;
                attackTimeProgression = 0;
                myAttackHitBox = Instantiate(attackHitBoxPrefab, transform.position + hitBoxOffset.x * transform.right + hitBoxOffset.y*transform.up + hitBoxOffset.z*transform.forward, Quaternion.identity, transform);
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
        Health -= _damages;
        if(Health <= 0)
        {
            Die();
        }
        else
        {
            Animator.SetTrigger("HitTrigger");
        }
        throw new System.NotImplementedException();
    }

    void Die()
    {
        Destroy(gameObject);
    }

    IEnumerator CheckDistance()
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
        StartCoroutine(CheckDistance());
    }

    void ChangingFocus(Transform _newFocus)
    {
        /*if(_newFocus != null)
        {

        }*/
        focusedPlayer = _newFocus;
    }

}
