using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldBossShield : MonoBehaviour, IHitable
{
    public OldEnemyBoss enemy;
    [SerializeField] private bool lockable = false; public bool lockable_access { get { return lockable; } set { lockable = value; } }
	[SerializeField] private float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }

    [SerializeField] private Vector3 lockSize3DModifier = Vector3.one; public Vector3 lockSize3DModifier_access { get { return lockSize3DModifier; } set { lockSize3DModifier = value; } }

    private void Update()
    {
        transform.position = enemy.transform.position + enemy.transform.forward * 2  + Vector3.up;
        transform.LookAt(transform.position + enemy.transform.forward );
    }
    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, float _damages, DamageSource _source, Vector3 _bumpModificators = default(Vector3))
    {
        if (_ball != null) // Check if it's the ball that touched
        {
            if ((_impactVector.normalized + transform.forward.normalized).magnitude < (20 / 63.5)) // This division makes it usable as a dot product
            {
                FeedbackManager.SendFeedback("event.ShieldHitByBall", this);
                Vector3 i_newDirection = Vector3.Reflect(_impactVector, transform.forward);
                //Debug.DrawRay(transform.position, i_newDirection, Color.magenta, 10f);
                i_newDirection.y = _impactVector.y;
                _ball.Bounce(i_newDirection, 1f);
            }
        }
    }

}
