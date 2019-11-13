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

public class TurretBehaviour : MonoBehaviour
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
    public float focusDistance;
    public float unfocusDistance;
    public float timeBetweenCheck;
    public float distanceBeforeChangingPriority;

    public int MaxHealth = 100;
    public int Health;
    bool playerOneInRange;
    bool playerTwoInRange;
    float distanceWithPlayerOne;
    float distanceWithPlayerTwo;
    Transform focusedPlayer = null;

    public GameObject bulletPrefab;
    public Transform leftBulletSpawn;
    public Transform rightBulletSpawn;


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

    void UpdateState()
    {
        print(State);
        switch (State)
        {
            case TurretState.Attacking:
                Quaternion wantedRotation = Quaternion.LookRotation(focusedPlayer.position - _self.position);
                wantedRotation.eulerAngles = new Vector3(0, wantedRotation.eulerAngles.y, 0);
                _self.rotation = Quaternion.Lerp(_self.rotation, wantedRotation, 0.2f);
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
                break;
        }
    }

    public void LaunchProjectile(bool _fromLeft)
    {
        Vector3 spawnPosition;
        if (_fromLeft)
            spawnPosition = leftBulletSpawn.position;
        else
            spawnPosition = rightBulletSpawn.position;

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

        yield return new WaitForSeconds(timeBetweenCheck);
        StartCoroutine(CheckDistance());
    }
}
