using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleLink : PuzzleActivator, IHitable
{
    private GameObject FX_Activation;
    private GameObject FX_Linked;
    private GameObject FX_LinkEnd;
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


    public float chargingTime;


    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source )
    {
        if (MomentumManager.GetMomentum() >= puzzleData.nbMomentumNeededToLink)
        {

            if (FX_Linked != null)
            {
                Destroy(FX_Linked);
            }

            if (FX_LinkEnd != null)
            {
                Destroy(FX_LinkEnd);
            }

            FX_Linked = FXManager.InstantiateFX(puzzleData.Linked, Vector3.up * 1, true, _impactVector, Vector3.one, transform);
            

            if (FX_Activation == null)
            {
                FX_Activation = FXManager.InstantiateFX(puzzleData.Linking, Vector3.up * 1, true, Vector3.zero, Vector3.one, transform);
            }
			MomentumManager.DecreaseMomentum(puzzleData.nbMomentumLooseWhenLink);
            chargingTime = puzzleData.nbSecondsLinkMaintained;
            isActivated = true;

            ActivateLinkedObjects();



        }

    }

    void Awake()
    {
        chargingTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (chargingTime > 0 && isActivated)
        {
            chargingTime -= Time.deltaTime;
        }

        if (chargingTime <= 0 && isActivated)
        {
            isActivated = false;
            FX_LinkEnd = FXManager.InstantiateFX(puzzleData.LinkEnd, Vector3.up * 1, true, Vector3.forward, Vector3.one, transform);
            if (FX_Activation != null)
            {
                Destroy(FX_Activation);
            }
            if (FX_Linked != null)
            {
                Destroy(FX_Linked);
            }

            DesactiveLinkedObjects();


        }



    }
}
