using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using UnityEngine.Analytics;

public enum PassState
{
	None,
	Aiming,
	Shooting,
	Receiving
}
public class PassController : MonoBehaviour
{
    [Separator("Private references")]
    [SerializeField] private Transform handTransform = default;
    [SerializeField] private BallDatas ballDatas = default;

    [Separator("General settings")]
	public bool passPreviewInEditor;
	public Color previewDefaultColor;

	public float delayBeforePickingOwnBall;
	public int minBouncesBeforePickingOwnBall;

	[Separator("Reception settings")]
	public float perfectReceptionBufferOnFail = 0.25f; //Delay before being able to retry another perfect reception if failed
	public float perfectReceptionMinDistance = 0.2f;
	public float perfectReceptionMinDelay = 0.2f;
	public float perfectReceptionExplosionRadius = 2f;
	public int perfectReceptionExplosionDamages = 1;

	public float curveRaycastIteration = 50;
	public float curveMinAngle = 10;
	public float hanseLength;

    // Auto-assigned References
    private PlayerController linkedPlayer;
	private PlayerController otherPlayer;
	private DunkController linkedDunkController;
    private LineRenderer lineRenderer;
    private Animator animator;


    private BallBehaviour currentBall;
	private List<Vector3> pathCoordinates;
	private bool passPreview;
	private float currentPassCooldown;
	private bool canReceive;
	private float ballTimeInHand;
	private bool didPerfectReception;
	private PassState previousState;
	private float perfectReceptionBuffer;
	private bool keepPerfectReceptionModifiers;
	private bool perfectReceptionShoot;
	private float pathLength;
	[ReadOnly] public PassState passState;

    private void Awake ()
	{
		lineRenderer = GetComponent<LineRenderer>();
		linkedPlayer = GetComponent<PlayerController>();
		linkedDunkController = GetComponent<DunkController>();
		animator = linkedPlayer.GetAnimator();

		canReceive = true;
		ChangeColor(previewDefaultColor);

		otherPlayer = linkedPlayer.GetOtherPlayer();
	}
	private void Update ()
	{
		UpdateCooldowns();
		UpdateBallTimeInHand();
		if (!otherPlayer.IsTargetable()) { DisablePassPreview(); }
	}

	private void LateUpdate ()
	{
		UpdatePass();
	}

	#region Public functions
	public void ExecutePerfectReception ( BallBehaviour _ball )
	{
		Receive(_ball);
		keepPerfectReceptionModifiers = true;
		ChangePassState(PassState.Aiming);
		EnablePassPreview();
		StartCoroutine(ShootAfterDelay_C(perfectReceptionMinDelay - ballTimeInHand));
		didPerfectReception = true;

		//Add ball modifier on perfect reception
		_ball.AddNewDamageModifier(new DamageModifier(ballDatas.damageModifierOnPerfectReception, -1, DamageModifierSource.PerfectReception));
		_ball.AddNewSpeedModifier(new SpeedCoef(ballDatas.speedMultiplierOnPerfectReception, -1, SpeedMultiplierReason.PerfectReception, false));

		float i_lerpValue = (_ball.GetCurrentDamageModifier() - 1) / (ballDatas.maxDamageModifierOnPerfectReception - 1); //Used for color
		GameObject i_perfectReceptionFX = FeedbackManager.SendFeedback("event.PlayerPerfectReception", linkedPlayer, handTransform.position, _ball.GetCurrentDirection(), default).GetVFX();
		ParticleColorer.ReplaceParticleColor(i_perfectReceptionFX, Color.white, ballDatas.colorOverDamage.Evaluate(i_lerpValue));
		MomentumManager.IncreaseMomentum(MomentumManager.datas.momentumGainedOnPerfectReception);
		Collider[] i_hitColliders = Physics.OverlapSphere(transform.position, perfectReceptionExplosionRadius);
		List<IHitable> i_hitTargets = new List<IHitable>();
		int i = 0;
		while (i < i_hitColliders.Length)
		{
			IHitable i_potentialHitableObject = i_hitColliders[i].GetComponentInParent<IHitable>();
			if (i_potentialHitableObject != null && !i_hitTargets.Contains(i_potentialHitableObject))
			{
				i_potentialHitableObject.OnHit(_ball, (i_hitColliders[i].transform.position - transform.position).normalized, linkedPlayer, perfectReceptionExplosionDamages, DamageSource.PerfectReceptionExplosion);
				i_hitTargets.Add(i_potentialHitableObject);
			}
			i++;
		}
	}
	public void TryReception () //Player tries to do a perfect reception
	{
		if (didPerfectReception || perfectReceptionBuffer > 0) { return; }
		BallBehaviour i_mainBall = BallBehaviour.instance;
		if (i_mainBall.GetCurrentThrower() == linkedPlayer) { return; }
		if (currentBall == null)
		{
			if (Vector3.Distance(handTransform.position, i_mainBall.transform.position) > perfectReceptionMinDistance) { perfectReceptionBuffer += perfectReceptionBufferOnFail; return; }
		}
		else
		{
			if (ballTimeInHand > perfectReceptionMinDelay) { perfectReceptionBuffer = 0; return; }
		}
		ExecutePerfectReception(i_mainBall);
	}
	public PlayerController GetTarget ()
	{
		return otherPlayer;
	}
	public List<Vector3> GetCurvedPathCoordinates ( Vector3 _startPosition, PawnController _target, Vector3 _lookDirection, out float totalLength )
	{
		//Get the middle position for the curve
		Vector3 i_startPosition = _startPosition;
		Vector3 i_endPosition = _target.GetCenterPosition();
		Vector3 i_direction = i_endPosition - i_startPosition;
		float i_lookDirectionAngle = Vector3.SignedAngle(new Vector3(i_direction.x, 0, i_direction.z), new Vector3(_lookDirection.x, 0, _lookDirection.z), Vector3.up);
		List<Vector3> i_coordinates = new List<Vector3>();
		_lookDirection.y = 0;

		Vector3 i_firstPoint = i_startPosition;
		Vector3 i_firstHandle = i_startPosition + _lookDirection.normalized * hanseLength;
		Vector3 i_secondPoint = i_endPosition;
		totalLength = 0;
		for (int i = 0; i < curveRaycastIteration; i++)
		{
			if (Mathf.Abs(i_lookDirectionAngle) > curveMinAngle)
			{
				i_coordinates.Add(SwissArmyKnife.CubicBezierCurve(i_firstPoint, i_firstHandle, i_secondPoint, i_secondPoint, i / curveRaycastIteration));
			}
			else
			{
				i_coordinates.Add(Vector3.Lerp(i_firstPoint, i_endPosition, i / curveRaycastIteration));
			}
			if (i > 0)
			{
				float distance = Vector3.Distance(i_coordinates[i], i_coordinates[i - 1]);
				totalLength += distance;
				foreach (RaycastHit hito in Physics.RaycastAll(i_coordinates[i - 1], i_coordinates[i] - i_coordinates[i - 1], distance))
				{

					//Raycast hit environment
					if (hito.collider.gameObject.layer == LayerMask.NameToLayer("Environment"))
					{
						i_coordinates.Add(hito.point);
						Vector3 _direction = Vector3.Reflect(i_coordinates[i] - i_coordinates[i - 1], hito.normal);
						_direction = _direction.normalized * 10;
						i_coordinates.Add(hito.point + _direction);
						totalLength += _direction.magnitude;
						return i_coordinates;
					}

					//Raycast hit shield enemy
					if (hito.collider.gameObject.tag == "Shield")
					{
						EnemyShield potentialEnemyShield = hito.collider.gameObject.GetComponentInParent<EnemyShield>();
						if (potentialEnemyShield != null && potentialEnemyShield.shield != null)
						{
							Vector3 impactVector = (i_coordinates[i] - i_coordinates[i - 1]);
							if (potentialEnemyShield.shield.transform.InverseTransformPoint(hito.point).z > -0.05)
							{
								i_coordinates.Add(hito.collider.ClosestPointOnBounds(i_coordinates[i]));
								Vector3 _direction = Vector3.Reflect(i_coordinates[i] - i_coordinates[i - 1], hito.normal);
								_direction = _direction.normalized * 10;
								i_coordinates.Add(hito.point + _direction);
								totalLength += _direction.magnitude;
								return i_coordinates;
							}
						}
					}
				}
			}
		}
		return i_coordinates;
	}
	public void Aim ()
	{
		ChangePassState(PassState.Aiming);
	}
	public void StopAim ()
	{
		ChangePassState(PassState.None);
	}
	public void ResetPreviewColor ()
	{
		lineRenderer.startColor = previewDefaultColor;
		lineRenderer.endColor = previewDefaultColor;
	}
	public void DisablePassPreview ()
	{
		if (passPreview)
		{
			if (previousState != PassState.Shooting)
			{
				LockManager.UnlockAll();
			}
			passPreview = false;
			lineRenderer.positionCount = 0;
		}
	}
	public bool CanShoot ()
	{
		if (linkedPlayer.GetCurrentPawnState() != null && !linkedPlayer.GetCurrentPawnState().allowBallThrow)
		{
			return false;
		}
		if (currentBall == null || currentPassCooldown >= 0 || linkedDunkController.IsDunking() || (!otherPlayer.IsTargetable()) || passState == PassState.Shooting)
		{
			return false;
		}
		else
		{
			return true;
		}
	}
	public BallBehaviour GetBall ()
	{
		return currentBall;
	}
	public void DropBall ()
	{
		if (GetBall() == null) { return; }
		currentBall.transform.SetParent(null);
		currentBall.ChangeSpeed(Mathf.Clamp(currentBall.GetCurrentSpeed(), 0f, 1f));
		currentBall.ChangeState(BallState.Grounded);
		currentBall = null;
	}
	public BallDatas GetBallDatas ()
	{
		return ballDatas;
	}
	public Transform GetHandTransform ()
	{
		return handTransform;
	}
	public void Shoot ()
	{
		if (!CanShoot()) { return; }
		if (didPerfectReception) { return; }
		ChangePassState(PassState.Shooting);
		Analytics.CustomEvent("PlayerPass", new Dictionary<string, object> { { "Zone", GameManager.GetCurrentZoneName() }, });
		FeedbackManager.SendFeedback("event.PlayerThrowingBall", linkedPlayer, handTransform.position, linkedPlayer.GetLookInput(), Vector3.zero); ;
		if (!keepPerfectReceptionModifiers)
		{
			BallBehaviour.instance.RemoveDamageModifier(DamageModifierSource.PerfectReception); BallBehaviour.instance.RemoveSpeedModifier(SpeedMultiplierReason.PerfectReception);
		}
		keepPerfectReceptionModifiers = false;
		BallBehaviour i_shotBall = currentBall;
		currentBall = null;
		didPerfectReception = false;
		MomentumManager.IncreaseMomentum(MomentumManager.datas.momentumGainedOnPass);
		MomentumManager.DisableMomentumExpontentialLoss();
		// Throw a curve pass
		if (otherPlayer != null)
		{
			if (passPreview)
			{
				i_shotBall.CurveShoot(this, linkedPlayer, otherPlayer, ballDatas, linkedPlayer.GetLookInput());
			}
			else
			{
				i_shotBall.Shoot(handTransform.position, otherPlayer.transform.position - transform.position, linkedPlayer, ballDatas, true);
			}
		}
		perfectReceptionShoot = false;
		ChangePassState(PassState.None);
	}
	public void Receive ( BallBehaviour _ball )
	{
		if (!canReceive) { return; }
		FeedbackManager.SendFeedback("event.PlayerReceivingBall", linkedPlayer, handTransform.position, _ball.GetCurrentDirection(), _ball.GetCurrentDirection());
		CursorManager.SetBallPointerParent(transform);
		currentBall = _ball;
		currentBall.GoToHands(handTransform, 0.2f, ballDatas);
		ballTimeInHand = 0;
		MomentumManager.EnableMomentumExponentialLoss(MomentumManager.datas.minPassDelayBeforeMomentumLoss, MomentumManager.datas.momentumLossSpeedIfNoPass);
		if (!linkedPlayer.IsTargetable()) { DropBall(); }
		if (linkedDunkController != null) { linkedDunkController.OnBallReceive(); }
	}
	public void EnablePassPreview ()
	{
		passPreview = true;
	}
	public void EnableBallReception ()
	{
		canReceive = true;
	}
	public void DisableBallReception ()
	{
		canReceive = false;
	}
	public bool CanReceive ()
	{
		if (linkedPlayer.GetCurrentPawnState() != null && !linkedPlayer.GetCurrentPawnState().allowBallReception)
		{
			return false;
		}
		return canReceive;
	}
	public void ChangePassState ( PassState _newState )
	{
		if (_newState == passState) { return; }
		previousState = passState;
		switch (_newState)
		{
			case PassState.Aiming:
				if (!CanShoot()) { return; }
				EnablePassPreview();
				animator.ResetTrigger("ShootingMissedTrigger");
				animator.SetTrigger("PrepareShootingTrigger");
				break;
			case PassState.Shooting:
				if (!CanShoot()) { return; }
				if (perfectReceptionShoot)
				{
					animator.ResetTrigger("ShootingMissedTrigger");
					animator.SetTrigger("PrepareShootingTrigger");
					animator.SetTrigger("ShootingTrigger");
				}
				else
				{
					animator.ResetTrigger("ShootingMissedTrigger");
					animator.ResetTrigger("PrepareShootingTrigger");
					animator.SetTrigger("HandoffTrigger");
				}
				return;
			case PassState.None:
				DisablePassPreview();
				animator.ResetTrigger("PrepareShootingTrigger");
				animator.SetTrigger("ShootingMissedTrigger");
				break;
		}
		passState = _newState;
	}
	#endregion

	#region Private functions
	private void ChangeColor ( Color _newColor )
	{
		Color i_newStartColor = _newColor;
		i_newStartColor.a = lineRenderer.startColor.a;
		lineRenderer.startColor = i_newStartColor;

		Color i_newEndColor = _newColor;
		i_newEndColor.a = lineRenderer.endColor.a;
		lineRenderer.endColor = i_newEndColor;
	}
	private void PreviewPath ( List<Vector3> _pathCoordinates )
	{
		lineRenderer.positionCount = _pathCoordinates.Count;
		lineRenderer.SetPositions(_pathCoordinates.ToArray());
		lineRenderer.materials[0].mainTextureScale = new Vector3(pathLength * (1f / lineRenderer.startWidth), 1, 1);
	}
	private void UpdateCooldowns ()
	{
		if (currentPassCooldown >= 0)
		{
			currentPassCooldown -= Time.deltaTime;
		}
		if (perfectReceptionBuffer >= 0)
		{
			perfectReceptionBuffer -= Time.deltaTime;
		}
	}
	private void UpdatePass()
	{
		if (passPreview)
		{
			pathCoordinates = GetCurvedPathCoordinates(handTransform.position, otherPlayer, linkedPlayer.GetLookInput(), out pathLength);
			PreviewPath(pathCoordinates);
			LockManager.LockTargetsInPath(pathCoordinates, 0);
		}
	}
	private void UpdateBallTimeInHand()
	{
		if (ballDatas == null && ballTimeInHand != 0) { ballTimeInHand = 0; return; }

		if (currentBall != null)
		{
			ballTimeInHand += Time.deltaTime;
			if (ballTimeInHand > perfectReceptionMinDelay + 0.1f)
			{
				BallBehaviour.instance.RemoveDamageModifier(DamageModifierSource.PerfectReception); BallBehaviour.instance.RemoveSpeedModifier(SpeedMultiplierReason.PerfectReception);
			}
		}
	}
	#endregion

	#region Coroutines
	IEnumerator ShootAfterDelay_C (float _delay)
	{
		yield return new WaitForSeconds(_delay);
		didPerfectReception = false;
		Analytics.CustomEvent("PerfectReception", new Dictionary<string, object> { { "Zone", GameManager.GetCurrentZoneName() }, });
		FeedbackManager.SendFeedback("event.PlayerShootingAfterPerfectReception", linkedPlayer, handTransform.position, default, default);
		perfectReceptionShoot = true;
		Shoot();
	}
	#endregion

}
