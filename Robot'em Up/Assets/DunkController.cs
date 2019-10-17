using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public enum DunkState
{
	Jumping,
	Waiting,
	Dashing,
	Cancelling,
	None
}
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


	private Rigidbody rb;
	[SerializeField] private DunkState dunkState;
	private Coroutine jumpCoroutine;

	private void Awake ()
	{
		rb = GetComponent<Rigidbody>();
	}

	public void Explode()
	{

	}

	public void OnBallReceive()
	{
		if (jumpCoroutine != null && dunkState == DunkState.Waiting)
		{
			StopCoroutine(jumpCoroutine);
		}
		StartCoroutine(DunkOnGround_C());
	}

	public void Dunk()
	{
		jumpCoroutine = StartCoroutine(Dunk_C());
	}

	IEnumerator Dunk_C()
	{
		yield return DunkJump_C();
		yield return DunkWait_C();
		yield return DunkCancel_C();
	}

	IEnumerator DunkOnGround_C ()
	{
		ChangeState(DunkState.Dashing);
		yield return FallOnGround_C(dunkDashSpeed);
		Explode();
	}

	IEnumerator DunkJump_C()
	{
		ChangeState(DunkState.Jumping);
		rb.isKinematic = true;
		rb.useGravity = false;

		Vector3 startPosition = transform.position;
		Vector3 endPosition = startPosition + Vector3.up * dunkJumpHeight + transform.forward * dunkJumpLength;

		for (float i = 0; i < dunkJumpDuration; i+=Time.deltaTime)
		{
			transform.position = Vector3.Lerp(startPosition, endPosition, i / dunkJumpDuration);
			yield return new WaitForEndOfFrame();
		}

		transform.position = endPosition;
	}

	IEnumerator DunkWait_C()
	{
		ChangeState(DunkState.Waiting);
		for (float i = 0; i < dunkJumpFreezeDuration; i+= Time.deltaTime)
		{
			yield return new WaitForEndOfFrame();
		}
	}

	IEnumerator DunkCancel_C()
	{
		ChangeState(DunkState.Cancelling);
		yield return FallOnGround_C(dunkCancelledFallSpeed);

	}
	IEnumerator FallOnGround_C(float _speed)
	{
		Vector3 startPosition = transform.position;
		Vector3 endPosition = startPosition;

		//Raycast to the ground to find end position
		RaycastHit hit;
		if (Physics.Raycast(startPosition, Vector3.down * 50, out hit, Mathf.Infinity, LayerMask.GetMask("Environment")))
		{
			endPosition = hit.point;
		} else
		{
			StopAllCoroutines();
			ChangeState(DunkState.None);
		}

		for (float i = 0; i < Vector3.Distance(startPosition, endPosition); i+=Time.deltaTime * _speed)
		{
			transform.position = Vector3.Lerp(startPosition, endPosition, i / Vector3.Distance(startPosition, endPosition));
			yield return new WaitForEndOfFrame();
		}
		transform.position = endPosition;

		ChangeState(DunkState.None);
		rb.isKinematic = false;
		rb.useGravity = true;
	}

	private void ChangeState(DunkState _newState)
	{
		switch (_newState)
		{
			case DunkState.Jumping:
				break;
			case DunkState.Dashing:
				break;
			case DunkState.Waiting:
				break;
			case DunkState.Cancelling:
				break;
		}
		dunkState = _newState;
	}
}
