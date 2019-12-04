using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour, IHitable
{
    public EnemyShield Enemy;
    [SerializeField] private bool _lockable = false; public bool lockable { get { return _lockable; } set { _lockable = value; } }
	[SerializeField] private float _lockHitboxSize; public float lockHitboxSize { get { return _lockHitboxSize; } set { _lockHitboxSize = value; } }
	public float angleRangeForRebound;

    private void Update()
    {
        transform.position = Enemy.transform.position + Enemy.transform.forward * 3f + Vector3.up;
        transform.LookAt(transform.position + Enemy.transform.forward);
    }
    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source, Vector3 _bumpModificators = default(Vector3))
    {
        if ((_impactVector.normalized + transform.forward.normalized).magnitude < 1.5f)
        {
            Vector3 newDirection = Vector3.Reflect(_impactVector, transform.forward);
            Debug.DrawRay(transform.position, newDirection, Color.magenta, 10f);
            newDirection.y = _impactVector.y;
            _ball.Bounce(newDirection, 1f) ;
        }
    }

}
