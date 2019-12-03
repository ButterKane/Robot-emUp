using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleLink : PuzzleActivator, IHitable
{
    private GameObject FX_Activation;
    private GameObject FX_Linked;
    private GameObject FX_LinkEnd;
	[SerializeField] private bool _lockable; public bool lockable { get { return _lockable; } set { _lockable = value; } }
	[SerializeField] private float _lockHitboxSize; public float lockHitboxSize { get { return _lockHitboxSize; } set { _lockHitboxSize = value; } }


	public float chargingTime;


    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source, Vector3 _bumpModificators = default(Vector3))
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

            FX_Linked = FXManager.InstantiateFX(puzzleData.Linked, Vector3.up * 2f, true, _impactVector, Vector3.one * 2f, transform);
            
            if (FX_Activation == null)
            {
                FX_Activation = FXManager.InstantiateFX(puzzleData.Linking, Vector3.up * 1.4f, true, Vector3.zero, Vector3.one * 1.4f, transform);
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
