using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPunch : MonoBehaviour
{
	private Collider collider;

	private void Awake ()
	{
		collider = GetComponent<Collider>();
		StartCoroutine(activateColliderAfterDelay());
	}

	IEnumerator activateColliderAfterDelay ()
	{
		yield return new WaitForSeconds(3f);
		collider.enabled = true;
		Debug.Log("Enabling collider");
	}

	private void OnTriggerEnter ( Collider other )
	{
		if (other.tag == "Player")
		{
			Debug.Log("Player entered zone");
		}
	}
}
