using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleLink : PuzzleActivator, IHitable
{
    private GameObject fX_Activation;
    private GameObject fX_Linked;
    private GameObject fX_LinkEnd;
	[SerializeField] private bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
	[SerializeField] private float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }


	public float chargingTime;


    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source, Vector3 _bumpModificators = default(Vector3))
    {
        if (_source == DamageSource.Ball | _source == DamageSource.Dunk)
        {
            if (MomentumManager.GetMomentum() >= puzzleData.nbMomentumNeededToLink)
            {

                if (fX_Linked != null)
                {
                    Destroy(fX_Linked);
                }

                if (fX_LinkEnd != null)
                {
                    Destroy(fX_LinkEnd);
                }

                fX_Linked = FXManager.InstantiateFX(puzzleData.linked, Vector3.up * 2f, true, _impactVector, Vector3.one * 2f, transform);
            
                if (fX_Activation == null)
                {
                    fX_Activation = FXManager.InstantiateFX(puzzleData.linking, Vector3.up * 1.4f, true, Vector3.zero, Vector3.one * 1.4f, transform);
                }
			    MomentumManager.DecreaseMomentum(puzzleData.nbMomentumLooseWhenLink);
                chargingTime = puzzleData.nbSecondsLinkMaintained;
                isActivated = true;

                SoundManager.PlaySound("PuzzleLinkActivate", transform.position, transform);

                ActivateLinkedObjects();
            }
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
            fX_LinkEnd = FXManager.InstantiateFX(puzzleData.linkEnd, Vector3.up * 1, true, Vector3.forward, Vector3.one, transform);
            if (fX_Activation != null)
            {
                Destroy(fX_Activation);
            }
            if (fX_Linked != null)
            {
                Destroy(fX_Linked);
            }

            SoundManager.PlaySound("PuzzleLinkDesactivate", transform.position, transform);
            DesactiveLinkedObjects();
        }
    }
}
