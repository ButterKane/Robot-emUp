﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour, IHitable
{
    [SerializeField] private bool _lockable = false; public bool lockable { get { return _lockable; } set { _lockable = value; } }
    public float angleRangeForRebound = 45f;

    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source)
    {
        Debug.Log("touched shield");
        
        if (Vector3.Dot(_impactVector.normalized, transform.forward.normalized) < (angleRangeForRebound /180)) // Check if dot product is somewhere inside right angle
        {
            Debug.Log("Rebound");
            Vector3 newDirection = Vector3.Reflect(_impactVector, transform.forward);
            Debug.DrawRay(transform.position, newDirection, Color.magenta, 10f);
            newDirection.y = _impactVector.y;
            _ball.Bounce(newDirection, 1f) ;
        }
    }

}
