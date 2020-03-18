using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableDunkReadyPanel : MonoBehaviour
{
	private void OnTriggerEnter ( Collider other )
	{
		DunkController.enableDunkReadyPanel = false;
	}
}
