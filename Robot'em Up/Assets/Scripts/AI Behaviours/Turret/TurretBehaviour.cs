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
    [SerializeField] private Transform _playerOne;
    [SerializeField] private Transform _playerTwo;
    
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
    bool playerOneInRange;
    bool playerTwoInRange;
    float distanceWithPlayerOne;
    float distanceWithPlayerTwo;
    Transform focusedPlayer = null;
    public float forwardPredictionRatio;

    [Space(2)]
    [Header("AimingCube")]
    public Transform aimingCubeTransform;
    public Renderer aimingCubeRenderer;
    public Vector3 aimingCubeDefaultScale;
    public Vector3 aimingCubeLockedScale;
    bool shouldRotateTowardsPlayer;
    public Color lockingAimingColor;
    public float lockingAimingColorIntensity;
    public Color followingAimingColor;
    public float followingAimingColorIntensity;

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
        _playerOne = GameManager.i.playerOne.transform;
        _playerTwo = GameManager.i.playerTwo.transform;


        Health = MaxHealth;

        StartCoroutine(CheckDistance());
    }

    void Update()
    {
        UpdateDistancesToPlayers();
        UpdateState();
    }

    void UpdateDistancesToPlayers()
    {
        distanceWithPlayerOne = Vector3.Distance(_self.position, _playerOne.position);
        distanceWithPlayerTwo = Vector3.Distance(_self.position, _playerTwo.position);
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

    public void ChangingState(TurretState _newState)
    {
        ExitState();
        State = _newState;
        EnterState();
    }

    void RotateTowardsPlayerAndHisForward()
    {
        Quaternion wantedRotation = Quaternion.LookRotation(focusedPlayer.position + focusedPlayer.forward*focusedPlayer.GetComponent<Rigidbody>().velocity.magnitude * forwardPredictionRatio - _self.position);
        wantedRotation.eulerAngles = new Vector3(0, wantedRotation.eulerAngles.y, 0);
        _self.rotation = Quaternion.Lerp(_self.rotation, wantedRotation, 0.2f);
    }

    void UpdateState()
    {
        //print(State);
        switch (State)
        {
            case TurretState.Attacking:
				if (focusedPlayer != null)
				{
                    if(shouldRotateTowardsPlayer)
                        RotateTowardsPlayerAndHisForward();
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
        if(focusedPlayer == null && _newFocus!=null)
        {
            ChangingState(TurretState.PrepareToAttack);
        }
        else if(focusedPlayer != null && _newFocus == null)
        {
            ChangingState(TurretState.Hiding);
        }

        focusedPlayer = _newFocus;
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
            if((focusedPlayer == _playerOne && distanceWithPlayerOne>unfocusDistance) || (focusedPlayer == _playerTwo && distanceWithPlayerTwo > unfocusDistance))
            {
                ChangingFocus(null);
            }
        }

        //Changing focus between the two
        if(playerOneInRange && playerTwoInRange && focusedPlayer != null)
        {
            if(focusedPlayer == _playerOne && distanceWithPlayerOne-distanceWithPlayerTwo > distanceBeforeChangingPriority)
            {
                ChangingFocus(_playerTwo);
            }
            else if (focusedPlayer == _playerTwo && distanceWithPlayerTwo - distanceWithPlayerOne > distanceBeforeChangingPriority)
            {
                ChangingFocus(_playerOne);
            }
        }

        //no focused yet ? Choose one
        if((playerOneInRange || playerTwoInRange) && focusedPlayer == null)
        {
            ChangingFocus(GetClosestPlayer());
        }

        //Restart coroutine in X seconds
        yield return new WaitForSeconds(timeBetweenCheck);
        StartCoroutine(CheckDistance());
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
