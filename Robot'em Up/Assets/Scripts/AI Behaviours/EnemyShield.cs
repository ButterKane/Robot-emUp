﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShield : MonoBehaviour, IHitable
{
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

    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source)
    {
        //Stop the ball
        _ball.ChangeSpeed(0);
       
    }

}
