using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public enum DunkState
{
	Jumping,
	Waiting,
	Dashing,
	Canceling,
	Explosing,
	Receiving,
	None
}
public class DunkController : MonoBehaviour
{
	[Header("Settings")]
	public float dunkJumpHeight = 5f;
	public float dunkJumpLength = 1f;
	public float dunkJumpDuration = 2f;
	public float dunkJumpFreezeDuration = 1f;

	public float dunkDashSpeed = 5f;
	public float dunkExplosionRadius = 10f;
	public int dunkDamages = 30;
	public float dunkProjectionForce = 10f;

	public float dunkCancelledFallSpeed = 2f;
	public float dunkDashDelay = 1f;


	private Rigidbody rb;
	[SerializeField] private DunkState dunkState;
	private Coroutine jumpCoroutine;
	private PassController passController;
	private PlayerController playerController;

	private GameObject waitingFX;
	private GameObject dashFX;

	private void Awake ()
	{
		rb = GetComponent<Rigidbody>();
		passController = GetComponent<PassController>();
		playerController = GetComponent<PlayerController>();
	}

	public void Explode ()
	{
		BallBehaviour ball = passController.GetBall();
		ChangeState(DunkState.Explosing);
		Collider[] hitColliders = Physics.OverlapSphere(ball.transform.position, dunkExplosionRadius);
		int i = 0;
		while (i < hitColliders.Length)
		{
			IHitable potentialHitableObject = hitColliders[i].GetComponentInParent<IHitable>();
			if (potentialHitableObject != null)
			{
				potentialHitableObject.OnHit(ball, Vector3.zero, playerController, dunkDamages, DamageSource.Dunk);
			}
			i++;
		}
		ChangeState(DunkState.None);
	}

	public void OnBallReceive ()
	{
		if (jumpCoroutine != null && dunkState == DunkState.Waiting)
		{
			StopCoroutine(jumpCoroutine);
			StartCoroutine(DunkOnGround_C());
		}
	}

	public void Dunk ()
	{
		jumpCoroutine = StartCoroutine(Dunk_C());
	}
	public bool CanDunk ()
	{
		if (MomentumManager.GetMomentum() >= 1f && dunkState == DunkState.None && passController.GetBall() == null)
		{
			return true;
		}
		else
		{
			return false;
		}
	}


	IEnumerator Dunk_C ()
	{
		yield return DunkJump_C();
		yield return DunkWait_C();
		yield return DunkCancel_C();
	}

	IEnumerator DunkOnGround_C ()
	{
		ChangeState(DunkState.Receiving);
		yield return new WaitForSeconds(dunkDashDelay);
		ChangeState(DunkState.Dashing);
		yield return FallOnGround_C(dunkDashSpeed);
		Explode();
	}

	IEnumerator DunkJump_C ()
	{
		ChangeState(DunkState.Jumping);
		rb.isKinematic = true;
		rb.useGravity = false;

		Vector3 startPosition = transform.position;
		Vector3 endPosition = startPosition + Vector3.up * dunkJumpHeight + transform.forward * dunkJumpLength;

		for (float i = 0; i < dunkJumpDuration; i += Time.deltaTime)
		{
			transform.position = Vector3.Lerp(startPosition, endPosition, i / dunkJumpDuration);
			yield return new WaitForEndOfFrame();
		}

		transform.position = endPosition;
	}

	IEnumerator DunkWait_C ()
	{
		ChangeState(DunkState.Waiting);
		for (float i = 0; i < dunkJumpFreezeDuration; i += Time.deltaTime)
		{
			yield return new WaitForEndOfFrame();
		}
	}

	IEnumerator DunkCancel_C ()
	{
		ChangeState(DunkState.Canceling);
		yield return FallOnGround_C(dunkCancelledFallSpeed);
	}

	IEnumerator FallOnGround_C ( float _speed )
	{
		Vector3 startPosition = transform.position;
		Vector3 endPosition = startPosition;

		//Raycast to the ground to find end position
		RaycastHit hit;
		if (Physics.Raycast(startPosition, Vector3.down * 50, out hit, Mathf.Infinity, LayerMask.GetMask("Environment")))
		{
			endPosition = hit.point;
		}
		else
		{
			StopAllCoroutines();
			ChangeState(DunkState.None);
		}

		for (float i = 0; i < Vector3.Distance(startPosition, endPosition); i += Time.deltaTime * _speed)
		{
			transform.position = Vector3.Lerp(startPosition, endPosition, i / Vector3.Distance(startPosition, endPosition));
			yield return new WaitForEndOfFrame();
		}
		transform.position = endPosition;

		ChangeState(DunkState.None);
		rb.isKinematic = false;
		rb.useGravity = true;
	}

	public bool isDunking()
	{
		if (dunkState == DunkState.None)
		{
			return false;
		} else
		{
			return true;
		}
	}
	private void ChangeState ( DunkState _newState )
	{
		BallDatas ballDatas = passController.GetBallDatas();
		BallBehaviour ball = passController.GetBall();
		Transform handTransform = passController.GetHandTransform();
		Animator playerAnimator = playerController.GetAnimator();

		switch (_newState)
		{
			case DunkState.Jumping:
				FXManager.InstantiateFX(ballDatas.DunkJump, transform.position, false, Vector3.up, Vector3.one * 2);
				playerAnimator.SetTrigger("PrepareDunkTrigger");
				break;
			case DunkState.Dashing:
				MomentumManager.DecreaseMomentum(1f);
				Destroy(waitingFX);
				dashFX = FXManager.InstantiateFX(ballDatas.DunkDash, ball.transform.position, false, Vector3.zero, Vector3.one, ball.transform);
				break;
			case DunkState.Waiting:
				waitingFX = FXManager.InstantiateFX(ballDatas.DunkIdle, handTransform.position, false, Vector3.zero, Vector3.one, handTransform);
				break;
			case DunkState.Canceling:
				playerAnimator.SetTrigger("DunkMissedTrigger");
				Destroy(waitingFX);
				break;
			case DunkState.Receiving:
				FXManager.InstantiateFX(ballDatas.DunkReceiving, ball.transform.position, false, Vector3.zero, Vector3.one, ball.transform);
				playerAnimator.SetTrigger("DunkTrigger");
				break;
			case DunkState.Explosing:
				foreach (PlayerController player in FindObjectsOfType<PlayerController>())
				{
					player.Vibrate(0.3f, VibrationForce.Heavy);
				}
				Destroy(dashFX);
				FXManager.InstantiateFX(ballDatas.DunkExplosion, ball.transform.position, false, Vector3.zero, Vector3.one);
				break;
		}
		dunkState = _newState;
	}
}
