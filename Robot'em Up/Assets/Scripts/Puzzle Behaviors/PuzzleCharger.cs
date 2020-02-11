using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleCharger : MonoBehaviour, IHitable
{
    public PuzzleDatas puzzleData;
	[SerializeField] private bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
	[SerializeField] private float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }

    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source,Vector3 _bumpModificators = default(Vector3))
    {
        FXManager.InstantiateFX(puzzleData.charging, Vector3.up * 2, true, Vector3.forward, Vector3.one * 3, transform);
        if (MomentumManager.GetMomentum() < 1)
        {
            MomentumManager.IncreaseMomentum(puzzleData.nbMomentumChargedByCharger);
        }

        EnergyManager.IncreaseEnergy(0.5f);

        // if momentum < 1
        // Will charge  puuzzledata.nbMomentumChargedByCharger Momentum

    }
    
}
