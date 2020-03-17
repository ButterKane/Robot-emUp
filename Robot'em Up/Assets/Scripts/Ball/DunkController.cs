using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
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
	public bool enableDunkAspiration;
	public float dunkJumpHeight = 5f;
	public float dunkJumpLength = 1f;
	public float dunkJumpDuration = 2f;
	public float dunkJumpFreezeDuration = 1f;
	[Range(0f, 1f)] public float energyPercentLostOnFail = 0.2f;

	[Space(15)]
	[Range(0f,1f)] public float dunkHitlagForce = 0.5f;
	public float dunkDashSpeed = 5f;
	public float dunkExplosionRadius = 10f;
	public float dunkAspirationRadius = 15f;
	public float aspirationMinDistanceToPlayer = 4;
	public int dunkDamages = 30;
	public float dunkProjectionForce = 10f;

    [Space(15)]
    public float dunkCancelledFallSpeed = 2f;
	public float dunkDashDelay = 1f;

    [Space(15)]
    public float dunkSnapTreshold = 30f;
	public float dunkCooldown = 3f;
	public float dunkCancelFreezeDuration = 0.4f;

	[Space(15)]
	public float dunkForwardAngleTreshold = 7.5f;
	public float dunkMaxForwardDistance = 5f;
	public float dunkForwardSpeed = 5f;

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

	private float currentCD;
	private GameObject dunkWaitingFX;
	private GameObject dunkDashFX;
	private GameObject currentDunkReadyPanel;
	private GameObject currentDunkReadyFX;

	private void Awake ()
	{
		EnergyManager.IncreaseEnergy(1f);
        dunkState = DunkState.None;

        rb = GetComponent<Rigidbody>();
		passController = GetComponent<PassController>();
		pawnController = GetComponent<PawnController>();
		playerController = GetComponent<PlayerController>();

		currentDunkReadyPanel = Instantiate(Resources.Load<GameObject>("PlayerResource/DunkReadyPanel"));
		currentDunkReadyPanel.transform.SetParent(GameManager.mainCanvas.transform);
	}

	private void Update ()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			pawnController.ChangeState("Dunking", Dunk_C());
		}
		if (currentCD >= 0)
		{
			currentCD -= Time.deltaTime;
		}
	}

	private void LateUpdate ()
	{
		UpdateDunkReadyPanel();
	}

	private void UpdateDunkReadyPanel ()
	{
		if (CanDunk() && currentDunkReadyPanel != null)
		{
			if (!currentDunkReadyPanel.activeSelf) { currentDunkReadyPanel.SetActive(true); }
			if (currentDunkReadyFX == null) { currentDunkReadyFX = FeedbackManager.SendFeedback("event.DunkReady", this, playerController.GetCenterPosition(), Vector3.up, Vector3.up).GetVFX(); }
			currentDunkReadyPanel.transform.position = GameManager.mainCamera.WorldToScreenPoint(playerController.GetHeadPosition());
		} else
		{
			if (currentDunkReadyFX != null) { Destroy(currentDunkReadyFX); }
			if (currentDunkReadyPanel.activeSelf) { currentDunkReadyPanel.SetActive(false); }
		}
	}
	public void Explode ()
	{
		Analytics.CustomEvent("SuccessfulDunk", new Dictionary<string, object> { { "Zone", GameManager.GetCurrentZoneName() }, });
		BallBehaviour i_ball = passController.GetBall();
		ChangeState(DunkState.Explosing);
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
		if (EnergyManager.GetEnergy() >= 1f && dunkState == DunkState.None && passController.GetBall() == null && GameManager.deadPlayers.Count <= 0 && currentCD <= 0 && playerController.GetOtherPlayer().passController.GetBall() != null)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	private void AttractEnemies()
	{
		if (playerController == null) { return; }
		GameObject attractionFX = FeedbackManager.SendFeedback("event.DunkAttraction", this, transform.position, Vector3.up, Vector3.up).GetVFX();
		attractionFX.transform.localScale = Vector3.one * dunkAspirationRadius;
		foreach (Collider c in Physics.OverlapSphere(transform.position, dunkAspirationRadius))
		{
			if (c.gameObject.layer == LayerMask.NameToLayer("Enemy"))
			{
				EnemyBehaviour enemy = c.GetComponentInParent<EnemyBehaviour>();
				Vector3 pushDirectionFlat = transform.position - enemy.transform.position;
				pushDirectionFlat.y = 0;
				float pushDistance = Vector3.Distance(transform.position, c.gameObject.transform.position) - aspirationMinDistanceToPlayer;
				pushDistance = Mathf.Clamp(pushDistance, 0, Mathf.Infinity);
				enemy.PushLightCustom(pushDirectionFlat, pushDistance, 0.5f, 1f);
			}
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
		if (enableDunkAspiration)
		{
			AttractEnemies();
		}

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
		ChangeState(DunkState.Waiting);
		SnapController.SetSnappable(SnapType.Pass, this.gameObject, dunkSnapTreshold, dunkJumpFreezeDuration);
		float forwardDistanceTravelled = 0;
		for (float i = 0; i < dunkJumpFreezeDuration; i += Time.deltaTime)
		{
			if (playerController)
			{
				Vector3 camForwardNormalized = GameManager.mainCamera.transform.forward;
				camForwardNormalized.y = 0;
				camForwardNormalized = camForwardNormalized.normalized;
				Vector3 camRightNormalized = GameManager.mainCamera.transform.right;
				camRightNormalized.y = 0;
				camRightNormalized = camRightNormalized.normalized;
				GamePadState state = GamePad.GetState(playerController.playerIndex);
				Vector3 moveInput = (state.ThumbSticks.Left.X * camRightNormalized) + (state.ThumbSticks.Left.Y * camForwardNormalized);
				if (moveInput.magnitude > 0.5f && forwardDistanceTravelled < dunkMaxForwardDistance && Vector3.SignedAngle(playerController.transform.forward, moveInput, Vector3.up) < dunkForwardAngleTreshold)
				{
					playerController.transform.position += playerController.transform.forward * Time.deltaTime * dunkForwardSpeed * moveInput.magnitude;
					forwardDistanceTravelled += Time.deltaTime * dunkForwardSpeed * moveInput.magnitude;
				}
			}
			yield return new WaitForEndOfFrame();
		}
	}

	public void StopDunk()
	{
		StopAllCoroutines();
		ChangeState(DunkState.None);
	}
	IEnumerator DunkCancel_C ()
	{
		ChangeState(DunkState.Canceling);
		currentCD = dunkCooldown;
		EnergyManager.DecreaseEnergy(energyPercentLostOnFail);
		yield return FallOnGround_C(dunkCancelledFallSpeed);
	}

	IEnumerator FallOnGround_C ( float _speed )
	{
		Time.timeScale = 1f - dunkHitlagForce;
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
		Time.timeScale = 1f;
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
				FeedbackManager.SendFeedback("event.DunkJumping", playerController);
				i_playerAnimator.SetTrigger("PrepareDunkTrigger");
				break;
			case DunkState.Dashing:
				dunkDashFX = FeedbackManager.SendFeedback("event.DunkDashing", i_handTransform).GetVFX();
				break;
			case DunkState.Waiting:
				dunkWaitingFX = FeedbackManager.SendFeedback("event.DunkWaiting", i_handTransform).GetVFX();
				break;
			case DunkState.Canceling:
				if (dunkWaitingFX) { Destroy(dunkWaitingFX); }
				i_playerAnimator.SetTrigger("DunkMissedTrigger");
				break;
			case DunkState.Receiving:
				if (dunkWaitingFX) { Destroy(dunkWaitingFX); }
				i_playerAnimator.SetTrigger("DunkTrigger");
				FeedbackManager.SendFeedback("event.DunkCatchingBall", i_handTransform);
				break;
			case DunkState.Explosing:
				if (dunkDashFX) { Destroy(dunkDashFX); }
				FeedbackManager.SendFeedback("event.DunkSmashingOnGround", playerController);
				break;
		}
		dunkState = _newState;
	}
}
