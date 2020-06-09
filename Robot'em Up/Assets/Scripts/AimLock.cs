using UnityEngine;
using UnityEngine.UI;

public class AimLock : MonoBehaviour
{
	public Transform linkedTarget;
	private Animator animator;
	private Collider extendedCollider;

	public void Init ( Transform _linkedTarget, float _radius, Color _color, Color _iconColor, Vector3 sizeModifier = default, bool _hideLock = false)
	{
		linkedTarget = _linkedTarget;
		animator = GetComponent<Animator>();
		if (_hideLock)
		{
			foreach (Image _im in GetComponentsInChildren<Image>())
			{
				_im.enabled = false;
			}
		}
		Collider existingCollider = _linkedTarget.GetComponent<Collider>();
		if (existingCollider != null)
		{
			switch (existingCollider.GetType().ToString())
			{
				case "BoxCollider":
					if (sizeModifier == default) { sizeModifier = Vector3.one; }
					extendedCollider = _linkedTarget.gameObject.AddComponent<BoxCollider>();
					extendedCollider.isTrigger = true;
					BoxCollider boxCollider = (BoxCollider)extendedCollider;
					boxCollider.size = new Vector3(_radius * sizeModifier.x, _radius * sizeModifier.y, _radius * sizeModifier.z);
					break;
				default:
					extendedCollider = _linkedTarget.gameObject.AddComponent<SphereCollider>();
					extendedCollider.isTrigger = true;
					SphereCollider sphereCollider = (SphereCollider)extendedCollider;
					sphereCollider.radius = _radius;
					break;
			}
		}
		else
		{
			extendedCollider = _linkedTarget.gameObject.AddComponent<SphereCollider>();
			extendedCollider.isTrigger = true;
			SphereCollider sphereCollider = (SphereCollider)extendedCollider;
			sphereCollider.radius = _radius;
		}
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
