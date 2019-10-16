using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
public class Dummy : MonoBehaviour, IHitable
{
	private int _hitCount;
	public int hitCount { 
		get { 
			return _hitCount; 
		}
		set { 
			_hitCount = value; 
		}
	}

	public int maxHealth;
	public void OnHit ( BallBehaviour _ball, Vector3 _impactVector, PlayerController _thrower )
	{
		transform.DOShakeScale(1f, 1f);
		hitCount++;
		_ball.Explode(true);
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
}
