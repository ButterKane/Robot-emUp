using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System;
#pragma warning disable 0649

public enum TurretState
{
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
    NotAttacking
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
        eventOnDeath = "event.TurretBasicDeath";
        isBumpable = false;

        ChangingTurretState(TurretState.Hidden);
    }

    new protected virtual void Update()
    {
        UpdateDistancesToPlayers();
        UpdateTurretState();
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

    public void ChangingTurretState(TurretState _newState)
    {
        ExitTurretState();
        turretState = _newState;
        EnterTurretState();
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

    public virtual void UpdateTurretState()
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
                    timeBetweenCheck = focusValues.maxTimeBetweenCheck;
                }
                if (focusedPawnController != null)
                {
                    Deploy();
                }
                break;

            case TurretState.Dying:
                break;

            case TurretState.Idle:
                timeBetweenCheck -= Time.deltaTime;

                if (timeBetweenCheck <= 0)
                {
                    CheckDistanceAndAdaptFocus();
                    timeBetweenCheck = focusValues.maxTimeBetweenCheck;
                }

                if(focusedPawnController != null)
                {
                    ChangingTurretState(TurretState.Attacking);
                }
                break;
        }
    }

    public void Deploy()
    {
        ChangingTurretState(TurretState.Idle);
    }

    public virtual void ExitTurretState()
    {
        switch (turretState)
        {
            case TurretState.Hiding:
                break;
            case TurretState.GettingOutOfGround:
                break;
            case TurretState.Hidden:
                animator.SetTrigger("GettingOutOfGroundTrigger");
                break;
            case TurretState.Dying:
                break;
            case TurretState.Attacking:
                break;
            case TurretState.Idle:
                break;
        }
    }

    public virtual void EnterTurretState()
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
                ChangingTurretAttackState(TurretAttackState.Anticipation);
                break;
            case TurretState.Idle:
                timeBetweenCheck = 0;
                break;
        }
    }

    public void ChangingTurretAttackState(TurretAttackState _newState)
    {
        ExitTurretAttackState();
        attackState = _newState;
        EnterTurretAttackState();
    }

    public virtual void EnterTurretAttackState()
    {
        switch (attackState)
        {
            case TurretAttackState.Anticipation:
                ChangeAimingRedDotState(AimingRedDotState.Following);
                animator.SetTrigger("AnticipationTrigger");
                currentAnticipationTime = attackValues.maxAnticipationTime;
                restTime = maxRestTime + UnityEngine.Random.Range(-randomRangeRestTime, randomRangeRestTime);
                break;
            case TurretAttackState.Attack:
                attackState = TurretAttackState.Attack;
                ChangeAimingRedDotState(AimingRedDotState.NotVisible);
                animator.SetTrigger("AttackTrigger");
                break;
            case TurretAttackState.Rest:
                break;
            case TurretAttackState.NotAttacking:
                break;
        }
    }

    public virtual void ExitTurretAttackState()
    {
        switch (attackState)
        {
            case TurretAttackState.Anticipation:
                break;
            case TurretAttackState.Attack:
                break;
            case TurretAttackState.Rest:
                animator.SetTrigger("FromRestToIdleTrigger");
                
                break;
            case TurretAttackState.NotAttacking:
                break;
        }
    }

    public virtual void AttackingUpdateState()
    {
        switch (attackState)
        {
            case TurretAttackState.Anticipation:
                
                if (focusedPawnController != null)
                {
                    RotateTowardsPlayerAndHisForward();
                }
                currentAnticipationTime -= Time.deltaTime;
                if (currentAnticipationTime <= 0)
                {
                    ChangingTurretAttackState(TurretAttackState.Attack);
                }
                break;

            case TurretAttackState.Attack:
                ChangingTurretAttackState(TurretAttackState.Rest);
                break;

            case TurretAttackState.Rest:
                restTime -= Time.deltaTime;
                if (restTime <= 0)
                {
                    ChangingTurretAttackState(TurretAttackState.NotAttacking);
                    
                    ChangingTurretState(TurretState.Idle);
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

    public virtual void ResetValuesAtEndOfAttack()
    {

    }

    public virtual void AbortAttack()
    {
        attackState = TurretAttackState.Rest;
        if (aimingRedDotState != AimingRedDotState.NotVisible)
        {
            ChangeAimingRedDotState(AimingRedDotState.NotVisible);
        }
        ChangingTurretState(TurretState.Idle);
    }

    public virtual void Shoot()
    {
        FeedbackManager.SendFeedback("event.TurretBasicAttack", this);
        Vector3 i_spawnPosition;
        i_spawnPosition = bulletSpawn.position;
        spawnedBullet = Instantiate(bulletPrefab, i_spawnPosition, Quaternion.LookRotation(modelPivot.forward));
        spawnedBullet.GetComponent<TurretBasicBullet>().launcher = transform;
        spawnedBullet.GetComponent<TurretBasicBullet>().canHitEnemies = canBulletTouchEnemies;
        spawnedBullet.GetComponent<TurretBasicBullet>().damageModificator = bulletDamageRatioToEnemies;

        if (aimingRedDotState != AimingRedDotState.NotVisible)
        {
            ChangeAimingRedDotState(AimingRedDotState.NotVisible);
        }
    }

    void ChangingFocus(Transform _newFocus)
    {
        if(focusedPawnController == null && _newFocus!=null)
        {
            if (turretState == TurretState.Hidden) { ChangingTurretState(TurretState.GettingOutOfGround); }
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
        if (distanceWithPlayerOne < focusValues.focusDistance && playerOnePawnController.IsTargetable())
        {
            playerOneInRange = true;
        }
        else
        {
            playerOneInRange = false;
        }

        if (distanceWithPlayerTwo < focusValues.focusDistance && playerTwoPawnController.IsTargetable())
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
            if((focusedPawnController.transform == playerOneTransform && (distanceWithPlayerOne> focusValues.unfocusDistance || !playerOnePawnController.IsTargetable())) 
                || ((focusedPawnController.transform == playerTwoTransform && (distanceWithPlayerTwo > focusValues.unfocusDistance || !playerTwoPawnController.IsTargetable()))))
            {
                ChangingFocus(null);
            }
        }

        //Changing focus between the two
        if((playerOneInRange && playerOnePawnController.IsTargetable()) 
            && (playerTwoInRange && playerTwoPawnController.IsTargetable()) 
            && focusedPawnController != null)
        {
            if(focusedPawnController.transform == playerOneTransform && distanceWithPlayerOne-distanceWithPlayerTwo > focusValues.distanceBeforeChangingPriority)
            {
                ChangingFocus(playerTwoTransform);
            }
            else if (focusedPawnController.transform == playerTwoTransform && distanceWithPlayerTwo - distanceWithPlayerOne > focusValues.distanceBeforeChangingPriority)
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

        if (UnityEngine.Random.Range(0f, 1f) <= deathValues.coreDropChances)
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
