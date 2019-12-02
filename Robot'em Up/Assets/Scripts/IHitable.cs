using UnityEngine;

public interface IHitable
{
	bool lockable { get; set; }
	float lockHitboxSize { get; set; }

	void OnHit ( BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source );
}
