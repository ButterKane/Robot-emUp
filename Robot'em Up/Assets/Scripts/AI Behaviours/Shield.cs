using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour, IHitable
{
    public EnemyShield enemy;
    [SerializeField] private bool lockable_access = false; public bool lockable { get { return lockable_access; } set { lockable_access = value; } }
	[SerializeField] private float lockHitboxSize_access; public float lockHitboxSize { get { return lockHitboxSize_access; } set { lockHitboxSize_access = value; } }

    private void Update()
    {
        transform.position = enemy.transform.position + enemy.transform.forward * enemy.spawningShieldFrontDistance + Vector3.up;
        transform.LookAt(transform.position + enemy.transform.forward);
    }
    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source, Vector3 _bumpModificators = default(Vector3))
    {
        if ((_impactVector.normalized + transform.forward.normalized).magnitude < (enemy.angleRangeForRebound / 63.5)) // This division makes it usable as a dot product
        {
            Vector3 internal_newDirection = Vector3.Reflect(_impactVector, transform.forward);
            Debug.DrawRay(transform.position, internal_newDirection, Color.magenta, 10f);
            internal_newDirection.y = _impactVector.y;
            _ball.Bounce(internal_newDirection, 1f) ;
        }
    }

}
