using UnityEngine;

interface IHitable
{
	void OnHit ( BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source );
}
