using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
public class Dummy : MonoBehaviour, IHitable
{
	[SerializeField] private bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
	[SerializeField] private float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }

    public string hitEvent;
    public string deathEvent;
	public GameObject deathFX;
	private int hitCount;
	private Vector3 initialScale;
	public int hitCount_access { 
		get { 
			return hitCount; 
		}
		set { 
			hitCount = value; 
		}
	}

	public int maxHealth;

	public virtual void OnHit ( BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source, Vector3 _bumpModificators = default(Vector3))
	{
		transform.DOShakeScale(1f, 1f).OnComplete(ResetScale);
		hitCount_access++;

        if (_source != DamageSource.Dunk)
        {
            EnergyManager.IncreaseEnergy(0.2f);
        }

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
