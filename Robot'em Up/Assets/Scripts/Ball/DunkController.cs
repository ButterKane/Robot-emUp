using MyBox;
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

	public float dunkSnapTreshold = 30f;
	public float dunkCooldown = 3f;
	public float dunkCancelFreezeDuration = 0.4f;

    [SerializeField] private DunkState dunkState;

    [Separator("Bump Variables")]
    public float BumpDistanceMod = 1;
    public float BumpDurationMod = 0.5f;
    public float BumpRestDurationMod = 0.7f;


    private Rigidbody rb;
	
	private Coroutine jumpCoroutine;
	private PassController passController;
	private PawnController pawnController;
	private PlayerController playerController;

	private GameObject waitingFX;
	private GameObject dashFX;
	private float currentCD;

	private void Awake ()
	{
        dunkState = DunkState.None;

        rb = GetComponent<Rigidbody>();
		passController = GetComponent<PassController>();
		pawnController = GetComponent<PawnController>();
		playerController = GetComponent<PlayerController>();
	}

	private void Update ()
	{
		if (currentCD >= 0)
		{
			currentCD -= Time.deltaTime;
		}
	}

	public void Explode ()
	{
		BallBehaviour ball = passController.GetBall();
		ChangeState(DunkState.Explosing);
		EnergyManager.DecreaseEnergy(1f);
		Collider[] hitColliders = Physics.OverlapSphere(ball.transform.position, dunkExplosionRadius);
		int i = 0;
		while (i < hitColliders.Length)
		{
			IHitable potentialHitableObject = hitColliders[i].GetComponentInParent<IHitable>();
			if (potentialHitableObject != null)
			{
				potentialHitableObject.OnHit(ball, (hitColliders[i].transform.position - transform.position).normalized, pawnController, dunkDamages, DamageSource.Dunk);
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
		if (!CanDunk()) { return; }
		jumpCoroutine = StartCoroutine(Dunk_C());
	}
	public bool CanDunk ()
	{
		if (EnergyManager.GetEnergy() >= 1f && dunkState == DunkState.None && passController.GetBall() == null && GameManager.deadPlayers.Count <= 0 && currentCD <= 0)
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
		if (playerController)
		{
			playerController.DisableInput();
		}
		passController.DisableBallReception();
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
		passController.EnableBallReception();
		ChangeState(DunkState.Waiting);
		SnapController.SetSnappable(SnapType.Pass, this.gameObject, dunkSnapTreshold, dunkJumpFreezeDuration);
		for (float i = 0; i < dunkJumpFreezeDuration; i += Time.deltaTime)
		{
			yield return new WaitForEndOfFrame();
		}
	}

	IEnumerator DunkCancel_C ()
	{
		ChangeState(DunkState.Canceling);
		currentCD = dunkCooldown;
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
			if (playerController)
			{
				playerController.EnableInput();
			}
			ChangeState(DunkState.None);
		}

		for (float i = 0; i < Vector3.Distance(startPosition, endPosition); i += Time.deltaTime * _speed)
		{
			transform.position = Vector3.Lerp(startPosition, endPosition, i / Vector3.Distance(startPosition, endPosition));
			yield return new WaitForEndOfFrame();
		}
		transform.position = endPosition;

		ChangeState(DunkState.None);
		if (playerController)
		{
			playerController.EnableInput();
			playerController.FreezeTemporarly(dunkCancelFreezeDuration);
		}
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
		Animator playerAnimator = pawnController.GetAnimator();

		switch (_newState)
		{
			case DunkState.Jumping:
				FXManager.InstantiateFX(ballDatas.DunkJump, transform.position, false, Vector3.up, Vector3.one * 2);
				playerAnimator.SetTrigger("PrepareDunkTrigger");
				break;
			case DunkState.Dashing:
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
				if (dashFX != null)
				{
					Destroy(dashFX);
				}
				FXManager.InstantiateFX(ballDatas.DunkExplosion, ball.transform.position, false, Vector3.zero, Vector3.one);
				break;
		}
		dunkState = _newState;
	}
}
