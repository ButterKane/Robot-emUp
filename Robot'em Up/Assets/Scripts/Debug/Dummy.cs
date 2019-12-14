using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
public class Dummy : MonoBehaviour, IHitable
{
	[SerializeField] private bool _lockable; public bool lockable { get { return _lockable; } set { _lockable = value; } }
	[SerializeField] private float _lockHitboxSize; public float lockHitboxSize { get { return _lockHitboxSize; } set { _lockHitboxSize = value; } }

	public GameObject deathFX;
	private int _hitCount;
	private Vector3 initialScale;
	public int hitCount { 
		get { 
			return _hitCount; 
		}
		set { 
			_hitCount = value; 
		}
	}

	public int maxHealth;
	public void OnHit ( BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source, Vector3 _bumpModificators = default(Vector3))
	{
		transform.DOShakeScale(1f, 1f).OnComplete(ResetScale);
		hitCount++;
		if (_ball != null)
		{
			_ball.Explode(true);
		}

        if (_source != DamageSource.Dunk)
        {
            EnergyManager.IncreaseEnergy(0.2f);
        }
		if (_hitCount >= maxHealth) { FXManager.InstantiateFX(deathFX, transform.position, false, Vector3.forward, Vector3.one); Destroy(this.gameObject); }

		//Fonctions utile
		//_ball.Bounce();
		//_ball.Attract();
		//_ball.CancelMovement();   <- Stop completely the ball
		//_ball.ChangeState();
		//_ball.MultiplySpeed();   <- Multiply speed by a coef
		//_ball.ChangeMaxDistance();  <- Changing max distance of ball by X will stop it after travelling X meters
		//_ball.Explode();  <- Spawns an FX
	}

	private void Awake ()
	{
		initialScale = transform.localScale;
	}

	public void ResetScale()
	{
		transform.DOScale(initialScale, 0.1f);
	}
}
