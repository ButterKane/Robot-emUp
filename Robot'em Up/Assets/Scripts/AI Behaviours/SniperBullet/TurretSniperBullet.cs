using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretSniperBullet : MonoBehaviour
{
	public Rigidbody rb;
	public float speed;
	public int damageDealt;
	public float maxLifeTime;
	Vector3 initialPosition;
	[HideInInspector] public Transform target;
	[HideInInspector] public Transform spawnParent;
	public GameObject impactFX;
	public Vector3 impactFXScale;
	public LayerMask layerToCheck;
	public float distanceToRaycast;

	[HideInInspector] public bool isAimingPlayer;
	public float distanceAoEDamage;

	private void Start ()
	{
		initialPosition = transform.position;
	}

	void Update ()
	{
		rb.velocity = (target.position - transform.position).normalized * speed;
		maxLifeTime -= Time.deltaTime;
		if (maxLifeTime <= 0 || !target.GetComponent<PawnController>().IsTargetable())
		{
			Destroy(gameObject);
		}
	}

	private void OnTriggerEnter ( Collider _other )
	{
		if (_other.tag == "Player")
		{
			Debug.Log("Damaging player");
			_other.GetComponent<PawnController>().Damage(damageDealt);
			GameObject i_impactFX = Instantiate(impactFX, transform.position, Quaternion.identity);
			i_impactFX.transform.localScale = impactFXScale;
			Destroy(i_impactFX, 1);
			Destroy(gameObject);
		}
		if (_other.gameObject.layer == LayerMask.NameToLayer("Environment"))
		{
			Vector3 hitPoint = _other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

			GameObject i_impactFX = Instantiate(impactFX, hitPoint, Quaternion.identity);
			i_impactFX.transform.localScale = impactFXScale;
			Destroy(i_impactFX, .25f);
			if (isAimingPlayer && Vector3.Distance(hitPoint, target.position) < distanceAoEDamage)
			{
				target.GetComponent<PawnController>().Damage(damageDealt);
			}
			Destroy(gameObject);
		}
	}
}