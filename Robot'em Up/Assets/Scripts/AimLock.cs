using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AimLock : MonoBehaviour
{
	public Transform linkedTarget;
	private Animator animator;
	private SphereCollider extendedCollider;

	public void Init(Transform _linkedTarget, float _radius, Color _color, Color _iconColor)
	{
		linkedTarget = _linkedTarget;
		animator = GetComponent<Animator>();
		extendedCollider = _linkedTarget.gameObject.AddComponent<SphereCollider>();
		extendedCollider.isTrigger = true;
		extendedCollider.radius = _radius;
		SetColor(_color, _iconColor) ;
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

	public void SetColor(Color _newColor, Color _iconColor)
	{
		foreach (Image image in GetComponentsInChildren<Image>())
		{
			if (image.gameObject.name == "Icon")
			{
				image.color = _iconColor;
			}
			else
			{
				image.color = _newColor;
			}
		}
	}
}
