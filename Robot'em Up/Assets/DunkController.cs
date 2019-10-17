using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class DunkController : MonoBehaviour
{
	[Header("Settings")]
	public float dunkJumpHeight;
	public float dunkJumpLength;
	public float dunkJumpDuration;
	public float dunkJumpFreezeDuration;

	public float dunkDashSpeed;
	public float dunkExplosionRadius;
	public int dunkDamages;
	public float dunkProjectionForce;

	public float dunkCancelledFallSpeed;




	public void DunkJump() //Player goes to the skies and waits for the ball
	{
		StartCoroutine(DunkJump_C());
	}

	public void DunkCancel () //Player stayed some time in the skies and got no ball, so dunk is cancelled :( 
	{

	}

	public void DunkDash() //Player got a ball while in the skies, now it's show time
	{

	}

	IEnumerator DunkJump_C()
	{
		Vector3 startPosition = transform.position;
		Vector3 endPosition = startPosition + Vector3.up * dunkJumpHeight + transform.forward * dunkJumpLength;

		for (float i = 0; i < dunkJumpDuration; i+=Time.deltaTime)
		{
			transform.position = Vector3.Lerp(startPosition, endPosition, i / dunkJumpDuration);
			yield return new WaitForEndOfFrame();
		}
		yield return null;
	}
}
