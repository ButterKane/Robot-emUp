using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
public class Dummy : MonoBehaviour, IHitable
{
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
	public void OnHit ( BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source )
	{
		transform.DOShakeScale(1f, 1f).OnComplete(ResetScale);
		hitCount++;
		_ball.Explode(true);
		MomentumManager.IncreaseMomentum(0.1f);
		if (_hitCount >= maxHealth) { Destroy(this.gameObject); }

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
