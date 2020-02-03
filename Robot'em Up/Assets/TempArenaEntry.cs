using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempArenaEntry : MonoBehaviour
{
	public Animator animator;
	public void CloseDoor()
	{
		GetComponent<Collider>().isTrigger = false;
		animator.SetBool("Closed", true);
	}
}
