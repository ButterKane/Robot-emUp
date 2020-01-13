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
	[Separator("General settings")]
	public float dunkJumpHeight = 5f;
	public float dunkJumpLength = 1f;
	public float dunkJumpDuration = 2f;
	public float dunkJumpFreezeDuration = 1f;

    [Space(15)]
	public float dunkDashSpeed = 5f;
	public float dunkExplosionRadius = 10f;
	public int dunkDamages = 30;
	public float dunkProjectionForce = 10f;

    [Space(15)]
    public float dunkCancelledFallSpeed = 2f;
	public float dunkDashDelay = 1f;

    [Space(15)]
    public float dunkSnapTreshold = 30f;
	public float dunkCooldown = 3f;
	public float dunkCancelFreezeDuration = 0.4f;

	private DunkState dunkState;

    [Separator("Bump settings")]
    public float bumpDistanceMod = 1;
    public float bumpDurationMod = 0.5f;
    public float bumpRestDurationMod = 0.7f;


    // Auto-assigned References
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
		if (Input.GetKeyDown(KeyCode.Space))
		{
			jumpCoroutine = StartCoroutine(Dunk_C());
		}
		if (currentCD >= 0)
		{
			currentCD -= Time.deltaTime;
		}
	}

	public void Explode ()
	{
		BallBehaviour i_ball = passController.GetBall();
		ChangeState(DunkState.Explosing);
		FeedbackManager.SendFeedback("event.DunkSmashingOnTheGround", this);
		SoundManager.PlaySound("DunkOnGround", transform.position);
		EnergyManager.DecreaseEnergy(1f);
		Collider[] i_hitColliders = Physics.OverlapSphere(i_ball.transform.position, dunkExplosionRadius);
		int i = 0;
		while (i < i_hitColliders.Length)
		{
			IHitable i_potentialHitableObject = i_hitColliders[i].GetComponentInParent<IHitable>();
			if (i_potentialHitableObject != null)
			{
				i_potentialHitableObject.OnHit(i_ball, (i_hitColliders[i].transform.position - transform.position).normalized, pawnController, dunkDamages, DamageSource.Dunk);
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
		FeedbackManager.SendFeedback("event.CaughtTheBallForDunk", this);
		SoundManager.PlaySound("CaughtBallForDunk", transform.position, transform);
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
		FeedbackManager.SendFeedback("event.JumpForDunk", this);
		SoundManager.PlaySound("JumpForDunk", transform.position, transform);
		passController.DisableBallReception();
		ChangeState(DunkState.Jumping);
		rb.isKinematic = true;
		rb.useGravity = false;

		Vector3 i_startPosition = transform.position;
		Vector3 i_endPosition = i_startPosition + Vector3.up * dunkJumpHeight + transform.forward * dunkJumpLength;

		for (float i = 0; i < dunkJumpDuration; i += Time.deltaTime)
		{
			transform.position = Vector3.Lerp(i_startPosition, i_endPosition, i / dunkJumpDuration);
			yield return new WaitForEndOfFrame();
		}

		transform.position = i_endPosition;
	}

	IEnumerator DunkWait_C ()
	{
		passController.EnableBallReception();
		SoundManager.PlaySound("ReadyToCatchDunk", transform.position, transform);
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
		Vector3 i_startPosition = transform.position;
		Vector3 i_endPosition = i_startPosition;

		//Raycast to the ground to find end position
		RaycastHit hit;
		if (Physics.Raycast(i_startPosition, Vector3.down * 50, out hit, Mathf.Infinity, LayerMask.GetMask("Environment")))
		{
			i_endPosition = hit.point;
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

		for (float i = 0; i < Vector3.Distance(i_startPosition, i_endPosition); i += Time.deltaTime * _speed)
		{
			transform.position = Vector3.Lerp(i_startPosition, i_endPosition, i / Vector3.Distance(i_startPosition, i_endPosition));
			yield return new WaitForEndOfFrame();
		}
		transform.position = i_endPosition;

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
		BallDatas i_ballDatas = passController.GetBallDatas();
		BallBehaviour i_ball = passController.GetBall();
		Transform i_handTransform = passController.GetHandTransform();
		Animator i_playerAnimator = pawnController.GetAnimator();

		switch (_newState)
		{
			case DunkState.Jumping:
				FXManager.InstantiateFX(i_ballDatas.dunkJump, transform.position + new Vector3(0,0.1f,0), false, Vector3.up, Vector3.one * 2.5f);
				i_playerAnimator.SetTrigger("PrepareDunkTrigger");
				break;
			case DunkState.Dashing:
				Destroy(waitingFX);
				dashFX = FXManager.InstantiateFX(i_ballDatas.dunkDash, i_ball.transform.position, false, Vector3.zero, Vector3.one, i_ball.transform);
				break;
			case DunkState.Waiting:
				waitingFX = FXManager.InstantiateFX(i_ballDatas.dunkIdle, i_handTransform.position, false, Vector3.zero, Vector3.one, i_handTransform);
				break;
			case DunkState.Canceling:
				i_playerAnimator.SetTrigger("DunkMissedTrigger");
				Destroy(waitingFX);
				break;
			case DunkState.Receiving:
				FXManager.InstantiateFX(i_ballDatas.dunkReceiving, i_ball.transform.position, false, Vector3.zero, Vector3.one, i_ball.transform);
				i_playerAnimator.SetTrigger("DunkTrigger");
				break;
			case DunkState.Explosing:
				if (dashFX != null)
				{
					Destroy(dashFX);
				}
				FXManager.InstantiateFX(i_ballDatas.dunkExplosion, i_ball.transform.position, false, Vector3.zero, Vector3.one);
				break;
		}
		dunkState = _newState;
	}
}
