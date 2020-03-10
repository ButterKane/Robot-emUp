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
    protected Quaternion wantedRotation;

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
        heightDeltaWithPlayerOne = Mathf.Abs(transform.position.y - playerOneTransform.position.y);

        distanceWithPlayerTwo = Vector3.Distance(transform.position, playerTwoTransform.position);
        heightDeltaWithPlayerTwo = Mathf.Abs(transform.position.y - playerTwoTransform.position.y);
    }

    Transform GetClosestAndAvailablePlayer()
    {
        if ((distanceWithPlayerOne >= distanceWithPlayerTwo &&
            (playerTwoPawnController.IsTargetable()) || !playerOnePawnController.IsTargetable()) &&
            heightDeltaWithPlayerTwo < maxHeightOfDetection)
        {
            return playerTwoTransform;
        }
        else if ((distanceWithPlayerTwo >= distanceWithPlayerOne && 
            (playerOnePawnController.IsTargetable()) || !playerTwoPawnController.IsTargetable()) &&
            heightDeltaWithPlayerOne < maxHeightOfDetection)
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

    protected virtual void RotateTowardsPlayerAndHisForward(float _rotationSpeedModRatio = 0)
    {
        wantedRotation = Quaternion.LookRotation(focusedPawnController.GetCenterPosition() + focusedPawnController.transform.forward*focusedPawnController.GetComponent<Rigidbody>().velocity.magnitude * forwardPredictionRatio - modelPivot.position);
      //  wantedRotation.eulerAngles = new Vector3(0, wantedRotation.eulerAngles.y, 0);
        modelPivot.rotation = Quaternion.Lerp(modelPivot.rotation, wantedRotation, Time.deltaTime * Mathf.Abs(maxRotationSpeed * (1-_rotationSpeedModRatio)));
    }

    protected virtual void RotateTowardsPlayerPosition(float _rotationSpeedModRatio = 0)
    {
        wantedRotation = Quaternion.LookRotation(focusedPawnController.GetCenterPosition() - modelPivot.position);
       // wantedRotation.eulerAngles = new Vector3(0, wantedRotation.eulerAngles.y, 0);
        modelPivot.rotation = Quaternion.Lerp(modelPivot.rotation, wantedRotation, Time.deltaTime * Mathf.Abs(maxRotationSpeed * (1-_rotationSpeedModRatio)));
    }

    public virtual void UpdateState()
    {
        //print("Global State : " + State);
        switch (turretState)
        {
            case TurretState.Attacking:
                CheckDistanceAndAdaptFocus();
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

                if(focusedPawnController != null)
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
                ChangeAimingRedDotState(AimingRedDotState.NotVisible);
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

                if (focusedPawnController != null)
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
        }
        else
        {
            aimingRedDotTransform.localScale = new Vector3(aimingRedDotTransform.localScale.x, aimingRedDotTransform.localScale.y, 50);
        }
    }

    public void TransitionFromAttackToRest() //called from animation event !
    {
        attackState = TurretAttackState.Rest;
    }

    public virtual void AbortAttack()
    {
        attackState = TurretAttackState.Rest;
        if (aimingRedDotState != AimingRedDotState.NotVisible)
        {
            ChangeAimingRedDotState(AimingRedDotState.NotVisible);
        }
        ChangingState(TurretState.Hiding);
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
        if(focusedPawnController == null && _newFocus!=null)
        {
            ChangingState(TurretState.GettingOutOfGround);
        }
        else if(focusedPawnController != null && _newFocus == null)
        {
            AbortAttack();
        }

        if (_newFocus != null) { focusedPawnController = _newFocus.GetComponent<PawnController>(); }
        else { focusedPawnController = null; }
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
        if (focusedPawnController != null)
        {
            if((focusedPawnController.transform == playerOneTransform && (distanceWithPlayerOne>unfocusDistance || !playerOnePawnController.IsTargetable())) 
                || ((focusedPawnController.transform == playerTwoTransform && (distanceWithPlayerTwo > unfocusDistance || !playerTwoPawnController.IsTargetable()))))
            {
                ChangingFocus(null);
            }
        }

        //Changing focus between the two
        if((playerOneInRange && playerOnePawnController.IsTargetable()) 
            && (playerTwoInRange && playerTwoPawnController.IsTargetable()) 
            && focusedPawnController != null)
        {
            if(focusedPawnController.transform == playerOneTransform && distanceWithPlayerOne-distanceWithPlayerTwo > distanceBeforeChangingPriority)
            {
                ChangingFocus(playerTwoTransform);
            }
            else if (focusedPawnController.transform == playerTwoTransform && distanceWithPlayerTwo - distanceWithPlayerOne > distanceBeforeChangingPriority)
            {
                ChangingFocus(playerOneTransform);
            }
        }

        //no focused yet ? Choose one
        if(((playerOneInRange && playerOnePawnController.IsTargetable()) 
            || (playerTwoInRange && playerTwoPawnController.IsTargetable())) 
            && focusedPawnController == null)
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
