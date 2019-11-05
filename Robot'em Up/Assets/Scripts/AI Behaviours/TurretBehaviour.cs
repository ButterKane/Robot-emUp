using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public enum TurretState
{
    Hidden,
    ChangingMode,
    Attacking,
    Dying,
}

public class TurretBehaviour : MonoBehaviour
{
    [Separator("References")]
    [SerializeField] private Transform _self;
    public Rigidbody Rb;

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

    public int MaxHealth = 100;
    public int Health;
    bool playerOneInRange;
    bool playerTwoInRange;
    float distanceWithPlayerOne;
    float distanceWithPlayerTwo;
    Transform focusedPlayer = null;


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
                focusedPlayer = null;
            }
        }

        //no focused yet ? Choose one
        if((playerOneInRange || playerTwoInRange) && focusedPlayer == null)
        {
            focusedPlayer = GetClosestPlayer();
        }

        yield return new WaitForSeconds(timeBetweenCheck);
        StartCoroutine(CheckDistance());
    }
}
