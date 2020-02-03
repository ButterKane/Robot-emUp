using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextSceneLoader : MonoBehaviour
{
	private void OnTriggerEnter ( Collider other )
	{
		if (other.GetComponent<PlayerController>() != null)
		{
			GameManager.LoadNextScene();
		}
	}
}
