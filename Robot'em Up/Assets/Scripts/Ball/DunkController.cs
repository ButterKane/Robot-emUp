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

	public float dunkDashSpeed = 5f;
	public float dunkExplosionRadius = 10f;
	public int dunkDamages = 30;
	public float dunkProjectionForce = 10f;

	public float dunkCancelledFallSpeed = 2f;
	public float dunkDashDelay = 1f;

	public float dunkSnapTreshold = 30f;
	public float dunkCooldown = 3f;
	public float dunkCancelFreezeDuration = 0.4f;

	[ReadOnly] [SerializeField] private DunkState dunkState;

    [Separator("Bump settings")]
    public float bumpDistanceMod = 1;
    public float bumpDurationMod = 0.5f;
    public float bumpRestDurationMod = 0.7f;


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
		BallBehaviour internal_ball = passController.GetBall();
		ChangeState(DunkState.Explosing);
		FeedbackManager.SendFeedback("event.DunkSmashingOnTheGround", this);
		SoundManager.PlaySound("DunkOnGround", transform.position);
		EnergyManager.DecreaseEnergy(1f);
		Collider[] internal_hitColliders = Physics.OverlapSphere(internal_ball.transform.position, dunkExplosionRadius);
		int i = 0;
		while (i < internal_hitColliders.Length)
		{
			IHitable internal_potentialHitableObject = internal_hitColliders[i].GetComponentInParent<IHitable>();
			if (internal_potentialHitableObject != null)
			{
				internal_potentialHitableObject.OnHit(internal_ball, (internal_hitColliders[i].transform.position - transform.position).normalized, pawnController, dunkDamages, DamageSource.Dunk);
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

		Vector3 internal_startPosition = transform.position;
		Vector3 internal_endPosition = internal_startPosition + Vector3.up * dunkJumpHeight + transform.forward * dunkJumpLength;

		for (float i = 0; i < dunkJumpDuration; i += Time.deltaTime)
		{
			transform.position = Vector3.Lerp(internal_startPosition, internal_endPosition, i / dunkJumpDuration);
			yield return new WaitForEndOfFrame();
		}

		transform.position = internal_endPosition;
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
		Vector3 internal_startPosition = transform.position;
		Vector3 internal_endPosition = internal_startPosition;

		//Raycast to the ground to find end position
		RaycastHit hit;
		if (Physics.Raycast(internal_startPosition, Vector3.down * 50, out hit, Mathf.Infinity, LayerMask.GetMask("Environment")))
		{
			internal_endPosition = hit.point;
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

		for (float i = 0; i < Vector3.Distance(internal_startPosition, internal_endPosition); i += Time.deltaTime * _speed)
		{
			transform.position = Vector3.Lerp(internal_startPosition, internal_endPosition, i / Vector3.Distance(internal_startPosition, internal_endPosition));
			yield return new WaitForEndOfFrame();
		}
		transform.position = internal_endPosition;

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
		BallDatas internal_ballDatas = passController.GetBallDatas();
		BallBehaviour internal_ball = passController.GetBall();
		Transform internal_handTransform = passController.GetHandTransform();
		Animator internal_playerAnimator = pawnController.GetAnimator();

		switch (_newState)
		{
			case DunkState.Jumping:
				FXManager.InstantiateFX(internal_ballDatas.dunkJump, transform.position + new Vector3(0,0.1f,0), false, Vector3.up, Vector3.one * 2.5f);
				internal_playerAnimator.SetTrigger("PrepareDunkTrigger");
				break;
			case DunkState.Dashing:
				Destroy(waitingFX);
				dashFX = FXManager.InstantiateFX(internal_ballDatas.dunkDash, internal_ball.transform.position, false, Vector3.zero, Vector3.one, internal_ball.transform);
				break;
			case DunkState.Waiting:
				waitingFX = FXManager.InstantiateFX(internal_ballDatas.dunkIdle, internal_handTransform.position, false, Vector3.zero, Vector3.one, internal_handTransform);
				break;
			case DunkState.Canceling:
				internal_playerAnimator.SetTrigger("DunkMissedTrigger");
				Destroy(waitingFX);
				break;
			case DunkState.Receiving:
				FXManager.InstantiateFX(internal_ballDatas.dunkReceiving, internal_ball.transform.position, false, Vector3.zero, Vector3.one, internal_ball.transform);
				internal_playerAnimator.SetTrigger("DunkTrigger");
				break;
			case DunkState.Explosing:
				if (dashFX != null)
				{
					Destroy(dashFX);
				}
				FXManager.InstantiateFX(internal_ballDatas.dunkExplosion, internal_ball.transform.position, false, Vector3.zero, Vector3.one);
				break;
		}
		dunkState = _newState;
	}
}
