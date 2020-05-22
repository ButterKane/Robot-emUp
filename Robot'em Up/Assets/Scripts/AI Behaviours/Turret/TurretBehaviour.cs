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
    private TurretBasicBullet spawnedBulletScript;
    //public float restTimeBeforeAimingCubeUnlocked;

	[Space(2)]
    [Header("Aiming Red Dot")]
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
        CreateProjectile();
        ChangingTurretState(TurretState.Hidden);
    }

    new protected virtual void Update()
    {
        UpdateDistancesToPlayers();
        UpdateTurretState();
		UpdateHealthBar();
	}

    #region Changing Turret State
    public void ChangingTurretState(TurretState _newState)
    {
        ExitTurretState();
        turretState = _newState;
        EnterTurretState();
    }

    private void UpdateTurretState()
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

                if (focusedPawnController != null)
                {
                    ChangingTurretState(TurretState.Attacking);
                }
                break;
        }
    }

    protected virtual void ExitTurretState()
    {
        switch (turretState)
        {
            case TurretState.Hiding:
                break;
            case TurretState.GettingOutOfGround:
                break;
            case TurretState.Hidden:
                animator.SetTrigger("DeployTrigger");
                if (baseAnimator != null) { baseAnimator.SetTrigger("DeployTrigger"); }
                break;
            case TurretState.Dying:
                break;
            case TurretState.Attacking:
                break;
            case TurretState.Idle:
                break;
        }
    }

    protected virtual void EnterTurretState()
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
                animator.SetTrigger("DeployTrigger");
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
    #endregion

    #region Changing Attack State
    protected void ChangingTurretAttackState(TurretAttackState _newState)
    {
        ExitTurretAttackState();
        attackState = _newState;
        EnterTurretAttackState();
    }

    protected virtual void EnterTurretAttackState()
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

    protected virtual void ExitTurretAttackState()
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

    protected virtual void AttackingUpdateState()
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
    #endregion

    #region Public methods
    public void Deploy()
    {
        ChangingTurretState(TurretState.Idle);
    }

    public virtual void Shoot()
    {
        FeedbackManager.SendFeedback("event.TurretBasicAttack", this);

        spawnedBulletScript.ActivateProjectile(bulletSpawn.position, Quaternion.LookRotation(modelPivot.forward));

        if (aimingRedDotState != AimingRedDotState.NotVisible)
        {
            ChangeAimingRedDotState(AimingRedDotState.NotVisible);
        }
    }

    public virtual void Die()
    {

    }

    public override void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, float _damages, DamageSource _source, Vector3 _bumpModificators = default)
    {
        base.OnHit(_ball, _impactVector, _thrower, _damages, _source, _bumpModificators);
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
        else if (_newState == AimingRedDotState.Locking)
        {
            aimingRedDotRenderer.material.color = lockingAimingColor;
            aimingRedDotRenderer.material.SetColor("_EmissionColor", lockingAimingColor * lockingAimingColorIntensity);
            aimingRedDotTransform.localScale = aimingRedDotLockedScale;
        }
        else if (_newState == AimingRedDotState.NotVisible)
        {
            aimingRedDotTransform.localScale = Vector3.zero;
        }
        aimingRedDotState = _newState;
    }

    public virtual void ResetValuesAtEndOfAttack()
    {

    }
    #endregion

    #region Private and protected methods
    private void UpdateDistancesToPlayers()
    {
        distanceWithPlayerOne = Vector3.Distance(transform.position, playerOneTransform.position);
        heightDeltaWithPlayerOne = Mathf.Abs(transform.position.y - playerOneTransform.position.y);

        distanceWithPlayerTwo = Vector3.Distance(transform.position, playerTwoTransform.position);
        heightDeltaWithPlayerTwo = Mathf.Abs(transform.position.y - playerTwoTransform.position.y);
    }

    protected virtual void RotateTowardsPlayerAndHisForward(float _rotationSpeedModRatio = 0)
    {
        wantedRotation = Quaternion.LookRotation(focusedPawnController.GetCenterPosition() + focusedPawnController.transform.forward * focusedPawnController.GetComponent<Rigidbody>().velocity.magnitude * forwardPredictionRatio - modelPivot.position);
        //  wantedRotation.eulerAngles = new Vector3(0, wantedRotation.eulerAngles.y, 0);
        modelPivot.rotation = Quaternion.Lerp(modelPivot.rotation, wantedRotation, Time.deltaTime * Mathf.Abs(maxRotationSpeed * (1 - _rotationSpeedModRatio)));
    }

    protected virtual void RotateTowardsPlayerPosition(float _rotationSpeedModRatio = 0)
    {
        wantedRotation = Quaternion.LookRotation(focusedPawnController.GetCenterPosition() - modelPivot.position);
        // wantedRotation.eulerAngles = new Vector3(0, wantedRotation.eulerAngles.y, 0);
        modelPivot.rotation = Quaternion.Lerp(modelPivot.rotation, wantedRotation, Time.deltaTime * Mathf.Abs(maxRotationSpeed * (1 - _rotationSpeedModRatio)));
    }

    protected virtual void AbortAttack()
    {
        ChangingTurretAttackState(TurretAttackState.Rest);
        if (aimingRedDotState != AimingRedDotState.NotVisible)
        {
            ChangeAimingRedDotState(AimingRedDotState.NotVisible);
        }
        ChangingTurretState(TurretState.Idle);
    }

    protected virtual void CreateProjectile()
    {
        if (spawnedBullet != null)
        {
            Destroy(spawnedBullet);
            spawnedBullet = null;
        }
        spawnedBullet = Instantiate(bulletPrefab, transform.position, Quaternion.LookRotation(modelPivot.forward));
        spawnedBulletScript = spawnedBullet.GetComponent<TurretBasicBullet>();
        spawnedBulletScript.launcher = transform;
        spawnedBulletScript.canHitEnemies = canBulletTouchEnemies;
        spawnedBulletScript.damageModificator = bulletDamageRatioToEnemies;
        spawnedBullet.gameObject.SetActive(false);
    }
    #endregion

    

    

    
}
