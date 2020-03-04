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
	public float size = 3f;
	public float scaleSpeed = 0.2f;
	public float lifeTime = 30f;
	Vector3 groundPosition = Vector3.zero;

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
		Destroy(this.gameObject, lifeTime);
		StartCoroutine(Scale_C());
	}

	void Update ()
	{
		Vector3 newDirection = transform.forward;
		newDirection.y = 0f;
		Vector3 newPosition = transform.position + newDirection * Time.deltaTime * moveSpeed;
		model.transform.Rotate(Vector3.up, Time.deltaTime * rotationSpeed);
		RaycastHit hit;
		if (Physics.Raycast(transform.position, Vector3.down, out hit, 20f, LayerMask.GetMask("Environment")))
		{
			groundPosition = hit.point + Vector3.up * 3f;
		}
		{
			if (newPosition.y > groundPosition.y + 1f)
			{
				newPosition.y -= Time.deltaTime * 5f;
			}
			if (newPosition.y < groundPosition.y - 1f)
			{
				newPosition.y += Time.deltaTime * 5f;
			}
		}
		transform.position = newPosition;
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

	IEnumerator Scale_C()
	{
		for (float i = 0; i < scaleSpeed; i+= Time.deltaTime)
		{
			transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * size, i / scaleSpeed);
			yield return null;
		}
		transform.localScale = Vector3.one * size;
	}
}
