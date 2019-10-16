using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    public GameObject head;

    private Transform playerOne;
    private Transform playerTwo;

    private float distanceToOne;
    private float distanceToTwo;

    private Transform target;
    
    void Start()
    {
        playerOne = GameManager.i.playerOne.transform;
        playerTwo = GameManager.i.playerTwo.transform;
    }

    // Update is called once per frame
    void Update()
    {
        distanceToOne = (playerOne.position - transform.position).magnitude;
        distanceToTwo = (playerTwo.position - transform.position).magnitude;

        if (distanceToOne < distanceToTwo)
        {
            target = playerOne;
        }
        else
        {
            target = playerTwo;
        }

        head.transform.LookAt(target);
    }
}
