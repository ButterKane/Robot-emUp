using UnityEngine;

interface IHitable
{
	int hitCount { get; set; }
	void OnHit ( BallBehaviour _ball, Vector3 _impactVector, PlayerController _thrower, int _damages, DamageSource _source );
}
