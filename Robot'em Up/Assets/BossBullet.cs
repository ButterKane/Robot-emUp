using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBullet : MonoBehaviour, IHitable
{
	[SerializeField] private bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
	[SerializeField] private float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }

	public bool hitable;
	public float moveSpeed;
	public float damages;
	public float rotationSpeed;

	private Transform model;
	public void OnHit ( BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, float _damages, DamageSource _source, Vector3 _bumpModificators = default )
	{
		if (hitable)
		{
			Destroy(gameObject);
		}
	}

	void Awake()
	{
		model = transform.Find("Model");
	}

	void Update ()
	{
		transform.position += transform.forward * Time.deltaTime * moveSpeed;
		model.transform.Rotate(Vector3.up, Time.deltaTime * rotationSpeed);
	}

	void OnTriggerEnter ( Collider _other )
	{
		if (_other.tag == "Player")
		{
			PlayerController hitPlayer = _other.GetComponent<PlayerController>();
			hitPlayer.Damage(damages);
			Destroy(gameObject);
		}
		if (_other.gameObject.layer == LayerMask.NameToLayer("Environment"))
		{
			Destroy(gameObject);
		}
	}
}
