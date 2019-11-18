using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPart : MonoBehaviour
{
	public PlayerController linkedPlayer;
	private Rigidbody rb;
	private Animator animator;
	float lifeTime = 0;
	bool inited = false;
	public bool grounded = false;
	public int totalPartCount;
	PlayerController picker = null;
	public void Init(PlayerController _linkedPlayer, Vector3 _throwVector, int _totalPartCount)
	{
		linkedPlayer = _linkedPlayer;
		picker = null;
		totalPartCount = _totalPartCount;
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
			foreach (Collider collider in GetComponents<Collider>())
			{
				if (!collider.isTrigger)
				{
					Destroy(collider);
				}
			}
			animator.SetTrigger("showArrow");
			rb.isKinematic = true;
		}
	}

	public void Pick(PlayerController _player)
	{
		animator.SetTrigger("pick");
		picker = _player;
	}
}
