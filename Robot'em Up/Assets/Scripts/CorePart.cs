using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorePart : MonoBehaviour
{
	public PawnController linkedPawn;
	private Rigidbody rb;
	private Animator animator;
	float lifeTime = 0;
	bool inited = false;
	public int healthValue;
	public bool grounded = false;
	public int totalPartCount;
	PlayerController picker = null;
	private Collider col;
	public void Init(PawnController _linkedPawn, Vector3 _throwVector, int _totalPartCount, int _healthValue)
	{
		linkedPawn = _linkedPawn;
		picker = null;
		totalPartCount = _totalPartCount;
		healthValue = _healthValue;
		rb = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
		inited = true;
		rb.AddForce(_throwVector, ForceMode.Impulse);
	}

	private void Update ()
	{
		if (inited)
		{
			lifeTime += Time.deltaTime;
			if (!grounded && lifeTime >= 1f) { TryGround(); } else
			{
				Quaternion flattedRotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);
				transform.localRotation = Quaternion.Lerp(transform.rotation, flattedRotation, Time.deltaTime);
			}
			if (picker != null)
			{
				transform.position = Vector3.Lerp(transform.position, picker.transform.position, Time.deltaTime * Vector3.Distance(transform.position, picker.transform.position) * 1.75f);
				if (Vector3.Distance(transform.position, picker.transform.position) < 1.85f)
				{
					Destroy(this.gameObject);
				}
			}
		}
	}

	void TryGround()
	{
		RaycastHit hit;
		if (Physics.Raycast(transform.position, Vector3.down, out hit, 1f, LayerMask.GetMask("Environment"))) {
			grounded = true;
			foreach (Collider collider in GetComponentsInChildren<Collider>())
			{
				if (!collider.isTrigger)
				{
					Destroy(collider);
				}
			}
			col = GetComponent<Collider>();
			ExtendingArmsController.grabableObjects.Add(col);
			if (animator != null) //if not an enemy core
			    animator.SetTrigger("showArrow");
			rb.isKinematic = true;
		}
	}

	public bool CanBePicked()
	{
		if (picker != null) { return false; }
		return true;
	}

	public void Pick(PlayerController _player)
	{
		if (animator != null)
			animator.SetTrigger("pick");
		picker = _player;
	}

	private void OnDestroy ()
	{
		if (col != null)
		{
			ExtendingArmsController.grabableObjects.Remove(col);
		}
	}
}
