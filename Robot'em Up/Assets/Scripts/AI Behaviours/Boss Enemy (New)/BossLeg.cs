using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLeg : MonoBehaviour, IHitable
{
	public BossBehaviour boss;
	private ProceduralSpiderLegAnimator legAnimator;
	[SerializeField] private bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
	[SerializeField] private float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }

	public void OnHit ( BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, float _damages, DamageSource _source, Vector3 _bumpModificators = default )
	{
		FeedbackManager.SendFeedback("event.BossLegHit", this);
		boss.Damage(_damages);
		legAnimator.StartCoroutine(legAnimator.BumpLeg_C());
	}


	void Awake()
	{
		legAnimator = GetComponentInParent<ProceduralSpiderLegAnimator>();
	}
}
