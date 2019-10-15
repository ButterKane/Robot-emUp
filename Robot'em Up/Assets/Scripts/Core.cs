using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core : MonoBehaviour
{
	private Rigidbody rigidbody;
	private int defaultLayer;

	private void Awake ()
	{
		rigidbody = GetComponent<Rigidbody>();
		defaultLayer = gameObject.layer;
	}
	public void EnableGravity()
	{
		rigidbody.useGravity = true;
	}

	public void DisableGravity()
	{
		rigidbody.useGravity = false;
	}

	public void EnableCollisions ()
	{
		rigidbody.isKinematic = false;
		gameObject.layer = defaultLayer;
	}
	public void DisableCollisions ()
	{
		rigidbody.isKinematic = true;
		gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
	}
}
