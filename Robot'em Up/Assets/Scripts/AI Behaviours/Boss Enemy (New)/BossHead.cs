using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHead : MonoBehaviour, IHitable
{
	[SerializeField] private bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
	[SerializeField] private float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }
	[SerializeField] private Vector3 lockSize3DModifier = Vector3.one; public Vector3 lockSize3DModifier_access { get { return lockSize3DModifier; } set { lockSize3DModifier = value; } }

	public void OnHit ( BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, float _damages, DamageSource _source, Vector3 _bumpModificators = default )
	{
		if (_source == DamageSource.Ball)
		{
			EnergyManager.IncreaseEnergy(BossSettings.GetDatas().staggerSettings.energyGainedOnHit);
		}
	}
}
