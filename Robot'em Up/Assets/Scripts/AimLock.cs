using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimLock : MonoBehaviour
{
	public Transform linkedTarget;
	private Animator animator;
	private SphereCollider extendedCollider;

	public void Init(Transform _linkedTarget, float _radius)
	{
		linkedTarget = _linkedTarget;
		animator = GetComponent<Animator>();
		extendedCollider = _linkedTarget.gameObject.AddComponent<SphereCollider>();
		extendedCollider.isTrigger = true;
		extendedCollider.radius = _radius;
	}

	private void Update ()
	{
		if (linkedTarget == null) { return; }
		transform.position = Camera.main.WorldToScreenPoint(linkedTarget.position + Vector3.up * 1);
	}

	public void DestroyObject()
	{
		LockManager.lockedTargets.Remove(this);
		Destroy(extendedCollider);
		Destroy(this.gameObject);
	}

	public void Unlock()
	{
		if (animator != null)
		{
			animator.SetTrigger("Unlock");
		}
	}
}
