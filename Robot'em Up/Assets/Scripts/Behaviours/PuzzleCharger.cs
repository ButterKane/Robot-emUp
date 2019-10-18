using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleCharger : MonoBehaviour, IHitable
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

    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PlayerController _thrower, int _damages, DamageSource _source )
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
