using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
#pragma warning disable 0649

public enum TurretState
{
    Hidden,
    Hiding,
    PrepareToAttack,
    Attacking,
    Dying,
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
    public TurretState State;

    [Space(2)]
    [Header("Focus")]
    public float focusDistance;
    public float unfocusDistance;
    public float timeBetweenCheck;
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
    public float forwardPredictionRatio;
    public float maxRotationSpeed;
    public float rotationSpeedAcceleration;
    float rotationSpeed;

	[SerializeField] private bool _lockable; public bool lockable { get { return _lockable; } set { _lockable = value; } }

	[Space(2)]
    [Header("Aiming Cube & Sphere")]
    //CUBE
    public Transform aimingCubeTransform;
    public Renderer aimingCubeRenderer;
    public Vector3 aimingCubeDefaultScale;
    public Vector3 aimingCubeLockedScale;
    bool shouldRotateTowardsPlayer;
    public Color lockingAimingColor;
    public float lockingAimingColorIntensity;
    public Color followingAimingColor;
    public float followingAimingColorIntensity;
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
        _playerOneTransform = GameManager.i.playerOne.transform;
        _playerTwoTransform = GameManager.i.playerTwo.transform;
        _playerOnePawnController = GameManager.i.playerOne.GetComponent<PawnController>();
        _playerTwoPawnController = GameManager.i.playerTwo.GetComponent<PawnController>();


        Health = MaxHealth;

        StartCoroutine(CheckDistanceAndAdaptFocus());
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

    Transform GetClosestPlayer()
    {
        if (distanceWithPlayerOne >= distanceWithPlayerTwo)
        {
            return _playerTwoTransform;
        }
        else
        {
            return _playerOneTransform;
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
        //print(State);
        switch (State)
        {
            case TurretState.Attacking:
				if (focusedPlayerTransform != null)
				{
                    if(shouldRotateTowardsPlayer)
                        RotateTowardsPlayerAndHisForward();
                }
                if(focusedPlayerTransform != null)
                {
                    wantedAimingPosition = focusedPlayerTransform.position + focusedPlayerTransform.forward * focusedPlayerTransform.GetComponent<Rigidbody>().velocity.magnitude * forwardPredictionRatio;
                }
                break;
            case TurretState.PrepareToAttack:
                break;
            case TurretState.Hiding:
                break;
            case TurretState.Hidden:
                break;
            case TurretState.Dying:
                break;
        }
    }

    void ExitState()
    {
        switch (State)
        {
            case TurretState.Hiding:
                break;
            case TurretState.PrepareToAttack:
                break;
            case TurretState.Hidden:
                break;
            case TurretState.Dying:
                break;
            case TurretState.Attacking:
                aimingCubeTransform.localScale = Vector3.zero;
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
            case TurretState.PrepareToAttack:
                Animator.SetTrigger("PrepareToAttackTrigger");
                break;
            case TurretState.Hidden:
                break;
            case TurretState.Dying:
                break;
            case TurretState.Attacking:
                aimingCubeTransform.localScale = aimingCubeDefaultScale;
                break;
        }
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
            ChangingState(TurretState.PrepareToAttack);
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

    IEnumerator CheckDistanceAndAdaptFocus()
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
            if((focusedPlayerTransform == _playerOneTransform && distanceWithPlayerOne>unfocusDistance) 
                || (focusedPlayerTransform == _playerTwoTransform && distanceWithPlayerTwo > unfocusDistance))
            {
                ChangingFocus(null);
            }
        }

        //Changing focus between the two
        if(playerOneInRange && playerTwoInRange && focusedPlayerTransform != null)
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
        if((playerOneInRange || playerTwoInRange) && focusedPlayerTransform == null)
        {
            ChangingFocus(GetClosestPlayer());
        }

        //Restart coroutine in X seconds
        yield return new WaitForSeconds(timeBetweenCheck);
        StartCoroutine(CheckDistanceAndAdaptFocus());
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

    public void AimingCubeRotate(bool _true)
    {
        if (_true)
        {
            shouldRotateTowardsPlayer = true;
            aimingCubeRenderer.material.color = followingAimingColor;
            aimingCubeRenderer.material.SetColor("_EmissionColor", followingAimingColor * followingAimingColorIntensity);
            aimingCubeTransform.localScale = aimingCubeDefaultScale;
        }
        else
        {
            shouldRotateTowardsPlayer = false;
            aimingCubeRenderer.material.color = lockingAimingColor;
            aimingCubeRenderer.material.SetColor("_EmissionColor", lockingAimingColor * lockingAimingColorIntensity);
            aimingCubeTransform.localScale = aimingCubeLockedScale;
        }
    }
}
