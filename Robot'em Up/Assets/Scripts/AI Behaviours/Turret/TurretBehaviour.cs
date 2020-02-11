using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System;
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

public enum AimingRedDotState
{
    NotVisible,
    Following,
    Locking,
}

public class TurretBehaviour : EnemyBehaviour, IHitable
{
    [Space(2)]
    [Separator("Turret Variables")]
    public Animator baseAnimator;
    public TurretState turretState;
    [NonSerialized] public TurretAttackState attackState;
    [NonSerialized] public AimingRedDotState aimingRedDotState;
    public Transform modelPivot;

    //[Space(2)]
    //[Header("Global")]
    PawnController focusedPlayerPawnController;
    public bool arenaTurret;

    [Space(2)]
    [Header("Attack")]
    public float defaultForwardPredictionRatio;
    float forwardPredictionRatio;
    public Vector2 minMaxRandomRangePredictionRatio;
    public float maxRotationSpeed;
    public float maxRestTime;
    public float randomRangeRestTime;
    protected float restTime;
    protected GameObject spawnedBullet;
    //public float restTimeBeforeAimingCubeUnlocked;

	[Space(2)]
    [Header("Aiming Cube")]
    //CUBE
    public Transform aimingRedDotTransform;
    public Renderer aimingRedDotRenderer;
    public Vector3 aimingRedDotDefaultScale;
    public Vector3 aimingRedDotLockedScale;
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
    public bool canBulletTouchEnemies = false;
    [Range(0,1)] public float bulletDamageRatioToEnemies = 0.5f;

    new void Start()
    {
        base.Start();
        isBumpable = false;
        if (arenaTurret)
        {
            ChangingState(TurretState.WaitForCombatStart);
        }
        else
        {
            ChangingState(TurretState.Idle);
        }
    }

    new protected virtual void Update()
    {
        UpdateDistancesToPlayers();
        UpdateState();
		UpdateHealthBar();
	}

    void UpdateDistancesToPlayers()
    {
        distanceWithPlayerOne = Vector3.Distance(transform.position, playerOneTransform.position);
        distanceWithPlayerTwo = Vector3.Distance(transform.position, playerTwoTransform.position);
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

    public void ChangingState(TurretState _newState)
    {
        ExitState();
        turretState = _newState;
        EnterState();
    }

    protected virtual void RotateTowardsPlayerAndHisForward()
    {
        wantedRotation = Quaternion.LookRotation(focusedPlayer.position + focusedPlayer.forward*focusedPlayer.GetComponent<Rigidbody>().velocity.magnitude * forwardPredictionRatio - modelPivot.position);
        wantedRotation.eulerAngles = new Vector3(0, wantedRotation.eulerAngles.y, 0);
        modelPivot.rotation = Quaternion.Lerp(modelPivot.rotation, wantedRotation, Time.deltaTime * Mathf.Abs(maxRotationSpeed));
    }

    protected virtual void RotateTowardsPlayerPosition()
    {
        wantedRotation = Quaternion.LookRotation(focusedPlayer.position - modelPivot.position);
        wantedRotation.eulerAngles = new Vector3(0, wantedRotation.eulerAngles.y, 0);
        modelPivot.rotation = Quaternion.Lerp(modelPivot.rotation, wantedRotation, Time.deltaTime * Mathf.Abs(maxRotationSpeed));
    }

    public virtual void UpdateState()
    {
        //print("Global State : " + State);
        switch (turretState)
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
                    timeBetweenCheck = maxTimeBetweenCheck;
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
                if(focusedPlayer != null)
                {
                    ChangingState(TurretState.Attacking);
                }
                break;
        }
    }

    public virtual void ExitState()
    {
        switch (turretState)
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
                break;
            case TurretState.Idle:
                break;
        }
    }

    public virtual void EnterState()
    {
        //print(State);
        switch (turretState)
        {
            case TurretState.Hiding:
                animator.SetTrigger("HidingTrigger");
                if (baseAnimator != null) { baseAnimator.SetTrigger("HidingTrigger"); }
                break;
            case TurretState.GettingOutOfGround:
                animator.SetTrigger("GettingOutOfGroundTrigger");
                if (baseAnimator != null) { baseAnimator.SetTrigger("GettingOutOfGroundTrigger"); }
                break;
            case TurretState.Hidden:
                break;
            case TurretState.Dying:
                break;
            case TurretState.Attacking:
                attackState = TurretAttackState.Anticipation;
                animator.SetTrigger("AnticipationTrigger");
                anticipationTime = maxAnticipationTime;
                restTime = maxRestTime + UnityEngine.Random.Range(-randomRangeRestTime, randomRangeRestTime);
                break;
            case TurretState.Idle:
                timeBetweenCheck = 0;
                break;
        }
    }

    public virtual void AttackingUpdateState()
    {
        switch (attackState)
        {
            case TurretAttackState.Anticipation:
                ChangeAimingRedDotState(AimingRedDotState.Following);

                if (focusedPlayer != null)
                {
                    RotateTowardsPlayerAndHisForward();
                }
                anticipationTime -= Time.deltaTime;
                if (anticipationTime <= 0)
                {
                    attackState = TurretAttackState.Attack;
                    animator.SetTrigger("AttackTrigger");
                }
                break;

            case TurretAttackState.Attack:
                
                break;

            case TurretAttackState.Rest:
                restTime -= Time.deltaTime;
                if (aimingRedDotState != AimingRedDotState.NotVisible)
                {
                    ChangeAimingRedDotState(AimingRedDotState.NotVisible);
                }
                if (restTime <= 0)
                {
                    animator.SetTrigger("FromRestToIdleTrigger");
                    ChangingState(TurretState.Idle);
                }
                break;
        }

        //Adapt aimCube Scale and Position
        RaycastHit hit;
        if (Physics.Raycast(transform.position, modelPivot.forward, out hit, 50, layersToCheckToScale))
        {
            aimingRedDotTransform.localScale = new Vector3(aimingRedDotTransform.localScale.x, aimingRedDotTransform.localScale.y, Vector3.Distance(modelPivot.position, hit.point));
            aimingRedDotTransform.position = modelPivot.position + (aimingRedDotTransform.localScale.z / 2 * modelPivot.forward);
        }
        else
        {
            aimingRedDotTransform.localScale = new Vector3(aimingRedDotTransform.localScale.x, aimingRedDotTransform.localScale.y, 50);
            aimingRedDotTransform.position = modelPivot.position + (aimingRedDotTransform.localScale.z / 2 * modelPivot.forward);
        }
    }

    public void TransitionFromAttackToRest() //called from animation event !
    {
        attackState = TurretAttackState.Rest;
    }

    public virtual void Shoot()
    {
        Vector3 i_spawnPosition;
        i_spawnPosition = bulletSpawn.position;
        spawnedBullet = Instantiate(bulletPrefab, i_spawnPosition, Quaternion.LookRotation(modelPivot.forward));
        spawnedBullet.GetComponent<TurretBasicBullet>().launcher = transform;
        spawnedBullet.GetComponent<TurretBasicBullet>().canHitEnemies = canBulletTouchEnemies;
        spawnedBullet.GetComponent<TurretBasicBullet>().damageModificator = bulletDamageRatioToEnemies;
    }

    void ChangingFocus(Transform _newFocus)
    {
        if(focusedPlayer == null && _newFocus!=null)
        {
            ChangingState(TurretState.GettingOutOfGround);
        }
        else if(focusedPlayer != null && _newFocus == null)
        {
            ChangingState(TurretState.Hiding);
        }

        focusedPlayer = _newFocus;
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
            if((focusedPlayer == playerOneTransform && (distanceWithPlayerOne>unfocusDistance || !playerOnePawnController.IsTargetable())) 
                || ((focusedPlayer == playerTwoTransform && (distanceWithPlayerTwo > unfocusDistance || !playerTwoPawnController.IsTargetable()))))
            {
                ChangingFocus(null);
            }
        }

        //Changing focus between the two
        if((playerOneInRange && playerOnePawnController.IsTargetable()) 
            && (playerTwoInRange && playerTwoPawnController.IsTargetable()) 
            && focusedPlayer != null)
        {
            if(focusedPlayer == playerOneTransform && distanceWithPlayerOne-distanceWithPlayerTwo > distanceBeforeChangingPriority)
            {
                ChangingFocus(playerTwoTransform);
            }
            else if (focusedPlayer == playerTwoTransform && distanceWithPlayerTwo - distanceWithPlayerOne > distanceBeforeChangingPriority)
            {
                ChangingFocus(playerOneTransform);
            }
        }

        //no focused yet ? Choose one
        if(((playerOneInRange && playerOnePawnController.IsTargetable()) 
            || (playerTwoInRange && playerTwoPawnController.IsTargetable())) 
            && focusedPlayer == null)
        {
            ChangingFocus(GetClosestAndAvailablePlayer());
        }
    }

    public virtual void Die()
    {

        if (UnityEngine.Random.Range(0f, 1f) <= coreDropChances)
        {
            DropCore();
        }

        Destroy(gameObject);
    }

    new public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source, Vector3 _bumpModificators = default(Vector3))
    {
        base.OnHit(_ball,_impactVector,  _thrower, _damages, _source, _bumpModificators);
		if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void ChangeAimingRedDotState(AimingRedDotState _newState)
    {
        if (_newState == AimingRedDotState.Following)
        {
            aimingRedDotRenderer.material.color = followingAimingColor;
            aimingRedDotRenderer.material.SetColor("_EmissionColor", followingAimingColor * followingAimingColorIntensity);
            aimingRedDotTransform.localScale = aimingRedDotDefaultScale;
            forwardPredictionRatio = defaultForwardPredictionRatio + UnityEngine.Random.Range(minMaxRandomRangePredictionRatio.x, minMaxRandomRangePredictionRatio.y);
        }
        else if(_newState == AimingRedDotState.Locking)
        {
            aimingRedDotRenderer.material.color = lockingAimingColor;
            aimingRedDotRenderer.material.SetColor("_EmissionColor", lockingAimingColor * lockingAimingColorIntensity);
            aimingRedDotTransform.localScale = aimingRedDotLockedScale;
        }
        else if(_newState == AimingRedDotState.NotVisible)
        {
            aimingRedDotTransform.localScale = Vector3.zero;
        }
        aimingRedDotState = _newState;
    }
}
