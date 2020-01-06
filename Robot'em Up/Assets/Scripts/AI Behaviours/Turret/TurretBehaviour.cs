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

public enum AimingCubeState
{
    NotVisible,
    Following,
    Locking,
}

public class TurretBehaviour : EnemyBehaviour, IHitable
{
    [Space(2)]
    [Separator("Auto-assigned References")]
    public Transform target;

    [Space(2)]
    [Separator("Variables")]
    public TurretState turretState;
    [NonSerialized] public TurretAttackState attackState;
    [NonSerialized] public AimingCubeState aimingCubeState;

    //[Space(2)]
    //[Header("Global")]
    bool playerOneInRange;
    bool playerTwoInRange;
    float distanceWithPlayerOne;
    float distanceWithPlayerTwo;
    protected Transform focusedPlayerTransform = null;
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

    new void Start()
    {
        base.Start();

        hitSound = "TurretHit";
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

    public virtual void Update()
    {
        UpdateDistancesToPlayers();
        UpdateState();
    }

    void UpdateDistancesToPlayers()
    {
        distanceWithPlayerOne = Vector3.Distance(self.position, playerOneTransform.position);
        distanceWithPlayerTwo = Vector3.Distance(self.position, playerTwoTransform.position);
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
        wantedRotation = Quaternion.LookRotation(focusedPlayerTransform.position + focusedPlayerTransform.forward*focusedPlayerTransform.GetComponent<Rigidbody>().velocity.magnitude * forwardPredictionRatio - self.position);
        wantedRotation.eulerAngles = new Vector3(0, wantedRotation.eulerAngles.y, 0);
        self.rotation = Quaternion.Lerp(self.rotation, wantedRotation, Time.deltaTime * Mathf.Abs(maxRotationSpeed));
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
                break;
            case TurretState.GettingOutOfGround:
                animator.SetTrigger("GettingOutOfGroundTrigger");
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
                if (focusedPlayerTransform != null)
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
                if (restTime <= 0)
                {
                    animator.SetTrigger("FromRestToIdleTrigger");
                    ChangingState(TurretState.Idle);
                }
                if(aimingCubeState != AimingCubeState.NotVisible)
                {
                    ChangeAimingCubeState(AimingCubeState.NotVisible);
                }
                break;
        }

        //Adapt aimCube Scale and Position
        RaycastHit hit;
        if (Physics.Raycast(self.position, self.forward, out hit, 50, layersToCheckToScale))
        {
            aimingCubeTransform.localScale = new Vector3(aimingCubeTransform.localScale.x, aimingCubeTransform.localScale.y, Vector3.Distance(self.position, hit.point));
            aimingCubeTransform.position = self.position + self.up * .5f + (aimingCubeTransform.localScale.z / 2 * self.forward);
        }
    }

    public void TransitionFromAttackToRest() //called from animation event !
    {
        attackState = TurretAttackState.Rest;
    }

    public virtual void LaunchProjectile()
    {
        Vector3 internal_spawnPosition;
        internal_spawnPosition = bulletSpawn.position;
        spawnedBullet = Instantiate(bulletPrefab, internal_spawnPosition, Quaternion.LookRotation(transform.forward));
        FeedbackManager.SendFeedback("event.BasicTurretAttack", this);
        SoundManager.PlaySound("BasicTurretAttack", transform.position);
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
        if (focusedPlayerTransform != null)
        {
            if((focusedPlayerTransform == playerOneTransform && (distanceWithPlayerOne>unfocusDistance || !playerOnePawnController.IsTargetable())) 
                || ((focusedPlayerTransform == playerTwoTransform && (distanceWithPlayerTwo > unfocusDistance || !playerTwoPawnController.IsTargetable()))))
            {
                ChangingFocus(null);
            }
        }

        //Changing focus between the two
        if((playerOneInRange && playerOnePawnController.IsTargetable()) 
            && (playerTwoInRange && playerTwoPawnController.IsTargetable()) 
            && focusedPlayerTransform != null)
        {
            if(focusedPlayerTransform == playerOneTransform && distanceWithPlayerOne-distanceWithPlayerTwo > distanceBeforeChangingPriority)
            {
                ChangingFocus(playerTwoTransform);
            }
            else if (focusedPlayerTransform == playerTwoTransform && distanceWithPlayerTwo - distanceWithPlayerOne > distanceBeforeChangingPriority)
            {
                ChangingFocus(playerOneTransform);
            }
        }

        //no focused yet ? Choose one
        if(((playerOneInRange && playerOnePawnController.IsTargetable()) 
            || (playerTwoInRange && playerTwoPawnController.IsTargetable())) 
            && focusedPlayerTransform == null)
        {
            ChangingFocus(GetClosestAndAvailablePlayer());
        }
    }

    public virtual void Die()
    {
        GameObject internal_deathParticle = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        internal_deathParticle.transform.localScale *= deathParticleScale;
        Destroy(internal_deathParticle, 1.5f);

        if (UnityEngine.Random.Range(0f, 1f) <= coreDropChances)
        {
            DropCore();
        }

        FeedbackManager.SendFeedback("event.BasicTurretDeath", this);
        SoundManager.PlaySound("BasicTurretDeath", transform.position);

        Destroy(gameObject);
    }

    new public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source, Vector3 _bumpModificators = default(Vector3))
    {
        base.OnHit(_ball,_impactVector,  _thrower, _damages, _source, _bumpModificators);
		if (health <= 0)
        {
            Die();
        }
    }

    public virtual void ChangeAimingCubeState(AimingCubeState _newState)
    {
        if (_newState == AimingCubeState.Following)
        {
            aimingCubeRenderer.material.color = followingAimingColor;
            aimingCubeRenderer.material.SetColor("_EmissionColor", followingAimingColor * followingAimingColorIntensity);
            aimingCubeTransform.localScale = aimingCubeDefaultScale;
            forwardPredictionRatio = defaultForwardPredictionRatio + UnityEngine.Random.Range(minMaxRandomRangePredictionRatio.x, minMaxRandomRangePredictionRatio.y);
        }
        else if(_newState == AimingCubeState.Locking)
        {
            aimingCubeRenderer.material.color = lockingAimingColor;
            aimingCubeRenderer.material.SetColor("_EmissionColor", lockingAimingColor * lockingAimingColorIntensity);
            aimingCubeTransform.localScale = aimingCubeLockedScale;
        }
        else if(_newState == AimingCubeState.NotVisible)
        {
            aimingCubeTransform.localScale = Vector3.zero;
        }
        aimingCubeState = _newState;
    }
}
