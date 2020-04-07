﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleCharger : MonoBehaviour, IHitable
{
    public PuzzleDatas puzzleData;
	[SerializeField] private bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
	[SerializeField] private float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }
    [SerializeField] private Vector3 lockSize3DModifier = Vector3.one; public Vector3 lockSize3DModifier_access { get { return lockSize3DModifier; } set { lockSize3DModifier = value; } }

    [Range(0f, 1f)]
    public float chargeAmount = 0.5f;

    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, float _damages, DamageSource _source,Vector3 _bumpModificators = default(Vector3))
    {
        FXManager.InstantiateFX(puzzleData.charging, Vector3.up * 2, true, Vector3.forward, Vector3.one * 3, transform);
        MomentumManager.IncreaseMomentum(puzzleData.nbMomentumChargedByCharger);
        EnergyManager.IncreaseEnergy(chargeAmount);

    }
    
}
