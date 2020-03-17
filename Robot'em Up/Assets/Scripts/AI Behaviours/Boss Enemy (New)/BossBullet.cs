using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBullet : MonoBehaviour, IHitable
{
	private bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
	[SerializeField] private float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }

	private BossSettings bossDatas;
	Vector3 groundPosition = Vector3.zero;

	private Transform model;
	public void OnHit ( BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, float _damages, DamageSource _source, Vector3 _bumpModificators = default )
	{
		if (bossDatas.bulletStormSettings.hitableBullet)
		{
			Destroy(gameObject);
		}
	}

	public void Init ( BossSettings _bossSettings )
	{
		bossDatas = _bossSettings;
		model = transform.Find("Model");
		Destroy(this.gameObject, bossDatas.bulletStormSettings.bulletLifetime);
		lockable = bossDatas.bulletStormSettings.lockableBullet;
		StartCoroutine(Scale_C());
	}

	void Update ()
	{
		Vector3 newDirection = transform.forward;
		newDirection.y = 0f;
		Vector3 newPosition = transform.position + newDirection * Time.deltaTime * bossDatas.bulletStormSettings.bulletMoveSpeed;
		model.transform.Rotate(Vector3.up, Time.deltaTime * bossDatas.bulletStormSettings.bulletRotationSpeed);
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
			hitPlayer.Damage(bossDatas.bulletStormSettings.bulletDamages);
			Destroy(gameObject);
		}
		if (_other.gameObject.layer == LayerMask.NameToLayer("Environment"))
		{
			Destroy(gameObject);
		}
	}

	IEnumerator Scale_C()
	{
		for (float i = 0; i < bossDatas.bulletStormSettings.bulletScaleDuration; i+= Time.deltaTime)
		{
			transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * bossDatas.bulletStormSettings.bulletSize, i / bossDatas.bulletStormSettings.bulletScaleDuration);
			yield return null;
		}
		transform.localScale = Vector3.one * bossDatas.bulletStormSettings.bulletSize;
	}
}
