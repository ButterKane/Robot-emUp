using UnityEngine;

public interface IHitable
{
	bool lockable_access { get; set; }
	float lockHitboxSize_access { get; set; }

	void OnHit ( BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, float _damages, DamageSource _source, Vector3 _bumpModificators = default(Vector3) );
}
