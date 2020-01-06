using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleCharger : MonoBehaviour, IHitable
{
    public PuzzleDatas puzzleData;
	[SerializeField] private bool _lockable; public bool lockable { get { return _lockable; } set { _lockable = value; } }
	[SerializeField] private float _lockHitboxSize; public float lockHitboxSize { get { return _lockHitboxSize; } set { _lockHitboxSize = value; } }
    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source,Vector3 _bumpModificators = default(Vector3))
    {
        FXManager.InstantiateFX(puzzleData.Charging, Vector3.up * 2, true, Vector3.forward, Vector3.one * 3, transform);
        if (MomentumManager.GetMomentum() < 1)
        {
            MomentumManager.IncreaseMomentum(puzzleData.nbMomentumChargedByCharger);
        }
        // if momentum < 1
        // Will charge  puuzzledata.nbMomentumChargedByCharger Momentum
      
    }
    
}
