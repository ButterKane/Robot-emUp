using System.Collections;
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

    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PlayerController _thrower, int _damages, DamageSource _source)
    {
        Debug.Log((_impactVector.normalized + transform.forward.normalized).magnitude);
        //Stop the ball 
        if ((_impactVector.normalized + transform.forward.normalized).magnitude > 0.8f  )
        {
            _ball.ChangeSpeed(0);

        }
        Debug.Log("SHielded");
    }

}
