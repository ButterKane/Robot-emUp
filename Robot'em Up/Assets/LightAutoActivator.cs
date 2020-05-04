using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightAutoActivator : MonoBehaviour
{
	public Light light;

	private void Awake ()
	{
		light.enabled = false;
	}
	private void OnTriggerEnter ( Collider other )
	{
		light.enabled = true;
	}
}
