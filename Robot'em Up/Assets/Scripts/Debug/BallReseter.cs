using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallReseter : MonoBehaviour
{
	private void OnTriggerEnter ( Collider other )
	{
		GameManager.ResetBall();
	}
}
