using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimLock : MonoBehaviour
{
	public EnemyBehaviour linkedEnemy;
	private Animator animator;
	private SphereCollider extendedCollider;

	public void Init(EnemyBehaviour _linkedEnemy, float _radius)
	{
		linkedEnemy = _linkedEnemy;
		animator = GetComponent<Animator>();
		extendedCollider = _linkedEnemy.gameObject.AddComponent<SphereCollider>();
		extendedCollider.isTrigger = true;
		extendedCollider.radius = _radius;
	}

	private void Update ()
	{
		if (linkedEnemy == null) { return; }
		transform.position = Camera.main.WorldToScreenPoint(linkedEnemy.transform.position + Vector3.up * 1);
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
