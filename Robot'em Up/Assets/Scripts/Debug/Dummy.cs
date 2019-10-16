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
		if (_hitCount >= maxHealth) { Destroy(this.gameObject); }

		//Fonctions utile
		//_ball.Bounce();
		//_ball.Attract();
		//_ball.CancelMovement();
		//_ball.ChangeState();
	}
}
