using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleLink : MonoBehaviour, IHitable
{
    public PuzzleDatas puzzleData;
    private int _hitCount;
    public int hitCount
    {
        get
        {
            return _hitCount;
        }
        set
        {
            _hitCount = value;
        }
    }


    public bool isActivated;
    public float chargingTime;


    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PlayerController _thrower)
    {
        if (MomentumManager.GetMomentum() > puzzleData.nbMomentumNeededToLink)
        {
            FXManager.InstantiateFX(puzzleData.Linked, Vector3.up * 1, true, transform);
            chargingTime = puzzleData.nbSecondsLinkMaintained;
            isActivated = true;

            //When a link is activate we need to check if a door would open
            PuzzleDoor[] doors = FindObjectsOfType<PuzzleDoor>();
            foreach (var item in doors)
            {
                item.checkIfValid();
            }
        }

    }

    void Start()
    {
        chargingTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (chargingTime>0 && isActivated)
        {
            chargingTime -= Time.deltaTime;
        }

        if (chargingTime <= 0 && isActivated)
        {
            isActivated = false;
            FXManager.InstantiateFX(puzzleData.LinkEnd, Vector3.up * 1, true, transform);

        }



    }
}
