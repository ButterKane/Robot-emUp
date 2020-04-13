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
	public static bool enableDunkReadyPanel = true;
	public float dunkJumpHeight = 5f;
	public float dunkJumpLength = 1f;
	public float dunkJumpDuration = 2f;
	public float dunkJumpFreezeDuration = 1f;
	[Range(0f, 1f)] public float energyCost = 1f;
	[Range(0f, 1f)] public float energyPercentLostOnFail = 0.2f;
	[Range(0f, 1f)] public float energyPercentLostOnSuccess = 1f;

	[Space(15)]
	[Range(0f,1f)] public float dunkHitlagForce = 0.5f;
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

	[Space(15)]
	public float dunkForwardAngleTreshold = 7.5f;
	public float dunkMaxForwardDistance = 5f;
	public float dunkForwardSpeed = 5f;

	[Space(15)]
	public bool enableDunkAspiration;
	[ConditionalField(nameof(enableDunkAspiration))] public float dunkAspirationRadius = 15f;
	[ConditionalField(nameof(enableDunkAspiration))] public float aspirationMinDistanceToPlayer = 4;

	private DunkState dunkState;


    // Auto-assigned References
	
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
        dunkState = DunkState.None;

		passController = GetComponent<PassController>();
		pawnController = GetComponent<PawnController>();
		playerController = GetComponent<PlayerController>();

		currentDunkReadyPanel = Instantiate(Resources.Load<GameObject>("PlayerResource/DunkReadyPanel"));
		currentDunkReadyPanel.transform.SetParent(GameManager.mainCanvas.transform);
	}
	private void Update ()
	{
		UpdateCooldown();
	}
	private void LateUpdate ()
	{
		UpdateDunkReadyPanel();
	}

	#region Public functions
	public void StopDunk ()
	{
		StopAllCoroutines();
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
		if (EnergyManager.GetEnergy() < energyCost || dunkState != DunkState.None || passController.GetBall() != null || GameManager.deadPlayers.Count > 0 || currentCD > 0 || playerController.GetOtherPlayer().passController.GetBall() == null)
		{
			return false;
		}
		return true;
	}
	public bool IsDunking ()
	{
		if (dunkState == DunkState.None)
		{
			return false;
		}
		else
		{
			return true;
		}
	}
	#endregion

	#region Private functions
	private void UpdateCooldown()
	{
		if (currentCD >= 0)
		{
			currentCD -= Time.deltaTime;
		}
	}
	private void UpdateDunkReadyPanel ()
	{
		if (CanDunk() && currentDunkReadyPanel != null)
		{
			if (!currentDunkReadyPanel.activeSelf) { if (enableDunkReadyPanel) { currentDunkReadyPanel.SetActive(true); } }
			if (currentDunkReadyFX == null) { currentDunkReadyFX = FeedbackManager.SendFeedback("event.DunkReady", this, pawnController.GetCenterPosition(), Vector3.up, Vector3.up).GetVFX(); }
			currentDunkReadyPanel.transform.position = GameManager.mainCamera.WorldToScreenPoint(pawnController.GetHeadPosition());
		}
		else
		{
			if (currentDunkReadyFX != null) { Destroy(currentDunkReadyFX); }
			if (currentDunkReadyPanel.activeSelf) { currentDunkReadyPanel.SetActive(false); }
		}
	}
	private void Explode ()
	{
		Analytics.CustomEvent("SuccessfulDunk", new Dictionary<string, object> { { "Zone", GameManager.GetCurrentZoneName() }, });
		BallBehaviour i_ball = passController.GetBall();
		ChangeState(DunkState.Explosing);
		EnergyManager.DecreaseEnergy(energyPercentLostOnSuccess);
		List<IHitable> i_hitTarget = new List<IHitable>();
		Collider[] i_hitColliders = Physics.OverlapSphere(i_ball.transform.position, dunkExplosionRadius);
		int i = 0;
		while (i < i_hitColliders.Length)
		{
			IHitable i_potentialHitableObject = i_hitColliders[i].GetComponentInParent<IHitable>();
			if (i_potentialHitableObject != null && !i_hitTarget.Contains(i_potentialHitableObject))
			{
				i_potentialHitableObject.OnHit(i_ball, (i_hitColliders[i].transform.position - transform.position).normalized, pawnController, dunkDamages, DamageSource.Dunk);
				i_hitTarget.Add(i_potentialHitableObject);
			}
			i++;
		}
		ChangeState(DunkState.None);
	}
	private void AttractEnemies()
	{
		if (playerController == null) { return; }
		GameObject attractionFX = FeedbackManager.SendFeedback("event.DunkAttraction", this, transform.position, Vector3.up, Vector3.up).GetVFX();
		attractionFX.transform.localScale = Vector3.one * dunkAspirationRadius;
		List<EnemyBehaviour> pushedEnemies = new List<EnemyBehaviour>();
		foreach (Collider c in Physics.OverlapSphere(transform.position, dunkAspirationRadius))
		{
			if (c.gameObject.layer == LayerMask.NameToLayer("Enemy"))
			{
				EnemyBehaviour enemy = c.GetComponentInParent<EnemyBehaviour>();
				if (!pushedEnemies.Contains(enemy))
				{
					Vector3 pushDirectionFlat = transform.position - enemy.transform.position;
					pushDirectionFlat.y = 0;
					float pushDistance = Vector3.Distance(transform.position, c.gameObject.transform.position) - aspirationMinDistanceToPlayer;
					pushDistance = Mathf.Clamp(pushDistance, 0, Mathf.Infinity);
					enemy.PushLightCustom(pushDirectionFlat, pushDistance, 0.5f, 0f);
					pushedEnemies.Add(enemy);
				}
			}
		}
	}
	private void ChangeState ( DunkState _newState )
	{
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
	#endregion

	#region Coroutines
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
		pawnController.Freeze();
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
				if (moveInput.magnitude > 0.5f && forwardDistanceTravelled < dunkMaxForwardDistance && Vector3.SignedAngle(pawnController.transform.forward, moveInput, Vector3.up) < dunkForwardAngleTreshold)
				{
					pawnController.transform.position += pawnController.transform.forward * Time.deltaTime * dunkForwardSpeed * moveInput.magnitude;
					forwardDistanceTravelled += Time.deltaTime * dunkForwardSpeed * moveInput.magnitude;
				}
			}
			yield return new WaitForEndOfFrame();
		}
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
		Time.timeScale = Mathf.Clamp(GameManager.i.gameSpeed - dunkHitlagForce, 0.2f, 1);
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
		pawnController.UnFreeze();
		Time.timeScale = GameManager.i.gameSpeed;
	}
	#endregion
}
