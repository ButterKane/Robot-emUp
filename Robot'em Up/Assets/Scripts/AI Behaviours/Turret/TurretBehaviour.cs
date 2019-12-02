using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
#pragma warning disable 0649

public enum TurretState
{
    WaitForCombatStart,
    Hidden,
    Hiding,
    GettingOutOfGround,
    Idle,
    Attacking,
    Dying,
}

public enum TurretAttackState
{
    Anticipation,
    Attack,
    Rest,
}

public enum AimingCubeState
{
    NotVisible,
    Following,
    Locking,
}

public class TurretBehaviour : MonoBehaviour, IHitable
{
    [Separator("References")]
    [SerializeField] private Transform _self;
    public Rigidbody Rb;
    public Animator Animator;

    [Space(2)]
    [Separator("Auto-assigned References")]
    public Transform Target;
    private Transform _playerOneTransform;
    private Transform _playerTwoTransform;
    private PawnController _playerOnePawnController;
    private PawnController _playerTwoPawnController;

    [Space(2)]
    [Separator("Variables")]
    [ReadOnly] public TurretState State;
    [ReadOnly] public TurretAttackState attackState;
    [ReadOnly] public AimingCubeState aimingCubeState;

    [Space(2)]
    [Header("Focus")]
    public float focusDistance;
    public float unfocusDistance;
    public float maxTimeBetweenCheck;
    float timeBetweenCheck;
    public float distanceBeforeChangingPriority;

    [Space(2)]
    [Header("Global")]
    public int MaxHealth = 100;
    public int Health;
    public float energyAmount;
    bool playerOneInRange;
    bool playerTwoInRange;
    float distanceWithPlayerOne;
    float distanceWithPlayerTwo;
    Transform focusedPlayerTransform = null;
    PawnController focusedPlayerPawnController;
    public bool arenaTurret;

    [Space(2)]
    [Header("Attack")]
    public float defaultForwardPredictionRatio;
    float forwardPredictionRatio;
    public Vector2 minMaxRandomRangePredictionRatio;
    public float maxRotationSpeed;
    public float maxAnticipationTime;
    float anticipationTime;
    public float maxRestTime;
    float restTime;
    public float restTimeBeforeAimingCubeUnlocked;

	[SerializeField] private bool _lockable; public bool lockable { get { return _lockable; } set { _lockable = value; } }
	[SerializeField] private float _lockHitboxSize; public float lockHitboxSize { get { return _lockHitboxSize; } set { _lockHitboxSize = value; } }

	[Space(2)]
    [Header("Aiming Cube")]
    //CUBE
    public Transform aimingCubeTransform;
    public Renderer aimingCubeRenderer;
    public Vector3 aimingCubeDefaultScale;
    public Vector3 aimingCubeLockedScale;
    public Color lockingAimingColor;
    public float lockingAimingColorIntensity;
    public Color followingAimingColor;
    public float followingAimingColorIntensity;
    public LayerMask layersToCheckToScale;
    //MISC
    Vector3 wantedAimingPosition;
    Quaternion wantedRotation;

    [Space(2)]
    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform bulletSpawn;

    [Space(2)]
    [Header("FXReferences")]
    public GameObject deathParticlePrefab;
    public float deathParticleScale;
    public GameObject hitParticlePrefab;
    public float hitParticleScale;

    public int hitCount { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    void Start()
    {
        _self = transform;
        _playerOneTransform = GameManager.playerOne.transform;
        _playerTwoTransform = GameManager.playerTwo.transform;
        _playerOnePawnController = GameManager.playerOne.GetComponent<PawnController>();
        _playerTwoPawnController = GameManager.playerTwo.GetComponent<PawnController>();

        Health = MaxHealth;

        if (arenaTurret)
        {
            ChangingState(TurretState.WaitForCombatStart);
        }
        else
        {
            ChangingState(TurretState.Idle);
        }
    }

    void Update()
    {
        UpdateDistancesToPlayers();
        UpdateState();
    }

    void UpdateDistancesToPlayers()
    {
        distanceWithPlayerOne = Vector3.Distance(_self.position, _playerOneTransform.position);
        distanceWithPlayerTwo = Vector3.Distance(_self.position, _playerTwoTransform.position);
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

    public void ChangingState(TurretState _newState)
    {
        ExitState();
        State = _newState;
        EnterState();
    }

    void RotateTowardsPlayerAndHisForward()
    {
        wantedRotation = Quaternion.LookRotation(focusedPlayerTransform.position + focusedPlayerTransform.forward*focusedPlayerTransform.GetComponent<Rigidbody>().velocity.magnitude * forwardPredictionRatio - _self.position);
        wantedRotation.eulerAngles = new Vector3(0, wantedRotation.eulerAngles.y, 0);
        _self.rotation = Quaternion.Lerp(_self.rotation, wantedRotation, Time.deltaTime * Mathf.Abs(maxRotationSpeed));
    }

    void UpdateState()
    {
        //print(attackState);
        switch (State)
        {
            case TurretState.Attacking:
                AttackingUpdateState();		
                break;
            case TurretState.GettingOutOfGround:
                break;
            case TurretState.Hiding:
                break;
            case TurretState.Hidden:
                timeBetweenCheck -= Time.deltaTime;
                if (timeBetweenCheck <= 0)
                {
                    CheckDistanceAndAdaptFocus();
                }
                break;
            case TurretState.Dying:
                break;
            case TurretState.Idle:
                timeBetweenCheck -= Time.deltaTime;
                if (timeBetweenCheck <= 0)
                {
                    CheckDistanceAndAdaptFocus();
                    timeBetweenCheck = maxTimeBetweenCheck;
                }
                if(focusedPlayerTransform != null)
                {
                    ChangingState(TurretState.Attacking);
                }
                break;
        }
    }

    void ExitState()
    {
        switch (State)
        {
            case TurretState.Hiding:
                break;
            case TurretState.GettingOutOfGround:
                break;
            case TurretState.Hidden:
                break;
            case TurretState.Dying:
                break;
            case TurretState.Attacking:
                ChangeAimingCubeState(AimingCubeState.NotVisible);
                break;
            case TurretState.Idle:
                break;
        }
    }

    void EnterState()
    {
        //print(State);
        switch (State)
        {
            case TurretState.Hiding:
                Animator.SetTrigger("HidingTrigger");
                break;
            case TurretState.GettingOutOfGround:
                Animator.SetTrigger("GettingOutOfGroundTrigger");
                break;
            case TurretState.Hidden:
                break;
            case TurretState.Dying:
                break;
            case TurretState.Attacking:
                attackState = TurretAttackState.Anticipation;
                Animator.SetTrigger("AnticipationTrigger");
                anticipationTime = maxAnticipationTime;
                restTime = maxRestTime;
                break;
            case TurretState.Idle:
                timeBetweenCheck = 0;
                break;
        }
    }

    void AttackingUpdateState()
    {
        switch (attackState)
        {
            case TurretAttackState.Anticipation:
                if (focusedPlayerTransform != null)
                {
                    RotateTowardsPlayerAndHisForward();
                }
                anticipationTime -= Time.deltaTime;
                if (anticipationTime <= 0)
                {
                    attackState = TurretAttackState.Attack;
                    Animator.SetTrigger("AttackTrigger");
                }
                break;
            case TurretAttackState.Attack:
                break;
            case TurretAttackState.Rest:
                restTime -= Time.deltaTime;
                if (restTime <= 0)
                {
                    ChangeAimingCubeState(AimingCubeState.NotVisible);
                    Animator.SetTrigger("FromRestToIdleTrigger");
                    ChangingState(TurretState.Idle);
                }
                else if(maxRestTime - restTime > restTimeBeforeAimingCubeUnlocked && aimingCubeState != AimingCubeState.Following)
                {
                    ChangeAimingCubeState(AimingCubeState.Following);
                }
                break;
        }

        //Adapt aimCube Scale and Position
        RaycastHit hit;
        if (Physics.Raycast(_self.position, _self.forward, out hit, 50, layersToCheckToScale))
        {
            aimingCubeTransform.localScale = new Vector3(aimingCubeTransform.localScale.x, aimingCubeTransform.localScale.y, Vector3.Distance(_self.position, hit.point));
            aimingCubeTransform.position = _self.position + _self.up * .5f + (aimingCubeTransform.localScale.z / 2 * _self.forward);
        }
    }

    public void TransitionFromAttackToRest() //called from animation event !
    {
        attackState = TurretAttackState.Rest;
    }

    public void LaunchProjectile()
    {
        Vector3 spawnPosition;
        spawnPosition = bulletSpawn.position;
        GameObject spawnedBullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.LookRotation(transform.forward));
    }

    void ChangingFocus(Transform _newFocus)
    {
        if(focusedPlayerTransform == null && _newFocus!=null)
        {
            ChangingState(TurretState.GettingOutOfGround);
        }
        else if(focusedPlayerTransform != null && _newFocus == null)
        {
            ChangingState(TurretState.Hiding);
        }

        focusedPlayerTransform = _newFocus;
        if(_newFocus != null)
        {
            focusedPlayerPawnController = _newFocus.gameObject.GetComponent<PlayerController>();
        }
        else
        {
            focusedPlayerPawnController = null;
        }
    }

    void CheckDistanceAndAdaptFocus()
    {
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
        if (focusedPlayerTransform != null)
        {
            if((focusedPlayerTransform == _playerOneTransform && (distanceWithPlayerOne>unfocusDistance || !_playerOnePawnController.IsTargetable())) 
                || ((focusedPlayerTransform == _playerTwoTransform && (distanceWithPlayerTwo > unfocusDistance || !_playerTwoPawnController.IsTargetable()))))
            {
                ChangingFocus(null);
            }
        }

        //Changing focus between the two
        if((playerOneInRange && _playerOnePawnController.IsTargetable()) 
            && (playerTwoInRange && _playerTwoPawnController.IsTargetable()) 
            && focusedPlayerTransform != null)
        {
            if(focusedPlayerTransform == _playerOneTransform && distanceWithPlayerOne-distanceWithPlayerTwo > distanceBeforeChangingPriority)
            {
                ChangingFocus(_playerTwoTransform);
            }
            else if (focusedPlayerTransform == _playerTwoTransform && distanceWithPlayerTwo - distanceWithPlayerOne > distanceBeforeChangingPriority)
            {
                ChangingFocus(_playerOneTransform);
            }
        }

        //no focused yet ? Choose one
        if(((playerOneInRange && _playerOnePawnController.IsTargetable()) 
            || (playerTwoInRange && _playerTwoPawnController.IsTargetable())) 
            && focusedPlayerTransform == null)
        {
            ChangingFocus(GetClosestAndAvailablePlayer());
        }
    }

    void Die()
    {
        GameObject deathParticle = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        deathParticle.transform.localScale *= deathParticleScale;
        Destroy(deathParticle, 1.5f);
        Destroy(gameObject);
    }

    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source)
    {
        Health -= _damages;
		if (_ball != null)
		{
			_ball.Explode(true);
		}
		if (Health <= 0)
        {
            Die();
        }
        else
        {
            GameObject hitParticle = Instantiate(hitParticlePrefab, transform.position, Quaternion.identity);
            hitParticle.transform.localScale *= hitParticleScale;
            Destroy(hitParticle, 1.5f);
        }
        EnergyManager.IncreaseEnergy(energyAmount);
    }

    public void ChangeAimingCubeState(AimingCubeState _NewState)
    {
        if (_NewState == AimingCubeState.Following)
        {
            aimingCubeRenderer.material.color = followingAimingColor;
            aimingCubeRenderer.material.SetColor("_EmissionColor", followingAimingColor * followingAimingColorIntensity);
            aimingCubeTransform.localScale = aimingCubeDefaultScale;
            forwardPredictionRatio = defaultForwardPredictionRatio + Random.Range(minMaxRandomRangePredictionRatio.x, minMaxRandomRangePredictionRatio.y);
        }
        else if(_NewState == AimingCubeState.Locking)
        {
            aimingCubeRenderer.material.color = lockingAimingColor;
            aimingCubeRenderer.material.SetColor("_EmissionColor", lockingAimingColor * lockingAimingColorIntensity);
            aimingCubeTransform.localScale = aimingCubeLockedScale;
        }
        else if(_NewState == AimingCubeState.NotVisible)
        {
            aimingCubeTransform.localScale = Vector3.zero;
        }
    }
}
