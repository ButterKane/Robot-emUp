using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour, IHitable
{
    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source)
    {
        Debug.Log("touched shield");
        if ((_impactVector.normalized + transform.forward.normalized).magnitude < 0.8f)
        {
            //_ball.Bounce();
        }
    }

}
