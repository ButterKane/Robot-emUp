using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;


public enum PassMode
{
    Bounce, 
    Curve,
    Straight,
    Count
}
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
	public PassMode passMode;
	public float passCooldown;
	public Color previewDefaultColor;
	public Color previewSnappedColor;

	public float delayBeforePickingOwnBall;
	public int minBouncesBeforePickingOwnBall;

	[Separator("Reception settings")]
	public float perfectReceptionBufferOnFail = 0.25f;
	public float perfectReceptionMinDistance = 0.2f;
	public float perfectReceptionMinDelay = 0.2f;
	public float perfectReceptionExplosionRadius = 2f;
	public int perfectReceptionExplosionDamages = 1;

	[ConditionalField(nameof(passMode), false, PassMode.Curve)] public float curveMaxLateralDistance;
	[ConditionalField(nameof(passMode), false, PassMode.Curve)] public float curveMaxPlayerDistance;
	[ConditionalField(nameof(passMode), false, PassMode.Curve)] public float curveRaycastIteration = 50;
	[ConditionalField(nameof(passMode), false, PassMode.Curve)] public float curveMinAngle = 10;
	[ConditionalField(nameof(passMode), false, PassMode.Curve)] public float hanseLength;

    // Auto-assigned References
    private PlayerController linkedPlayer;
	private PlayerController otherPlayer;
	private DunkController linkedDunkController;
    private LineRenderer lineRenderer;
    private Animator animator;


    private BallBehaviour ball;
	private List<Vector3> pathCoordinates;
	private bool passPreview;
	private float currentPassCooldown;
	private bool canReceive;
	private float ballTimeInHand;
	private bool didPerfectReception;
	private PassState previousState;
	private float perfectReceptionBuffer;
	[ReadOnly] public PassState passState;

    private void Awake ()
	{
		lineRenderer = GetComponent<LineRenderer>();
		linkedPlayer = GetComponent<PlayerController>();
		linkedDunkController = GetComponent<DunkController>();
		animator = GetComponentInChildren<Animator>();

		canReceive = true;
		ChangeColor(previewDefaultColor);

		otherPlayer = GetTarget();
	}
	private void Update ()
	{
		UpdateCooldowns();

		if (ballDatas == null) { ballTimeInHand = 0; return; }

		if (ball != null)
		{
			ballTimeInHand += Time.deltaTime;
		}

		if (!otherPlayer.IsTargetable() && passMode == PassMode.Curve) { DisablePassPreview(); }

		if (passPreview)
		{
			bool i_snapped = false;
			switch (passMode)
			{
				case PassMode.Bounce:
					pathCoordinates = GetBouncingPathCoordinates(handTransform.position, SnapController.SnapDirection(handTransform.position, transform.forward, out i_snapped), ballDatas.maxPreviewDistance);
					break;
				case PassMode.Curve:
					pathCoordinates = GetCurvedPathCoordinates(handTransform.position, otherPlayer.transform, linkedPlayer.GetLookInput());
					break;
			}
			if (i_snapped) { ChangeColor(previewSnappedColor); } else { ChangeColor(previewDefaultColor); }
			PreviewPath(pathCoordinates);
			LockManager.LockTargetsInPath(pathCoordinates,0);
		}

	}
	public void TryReception() //Player tries to do a perfect reception
	{
		if (didPerfectReception) { return; }
		if (perfectReceptionBuffer > 0) { return; }
		BallBehaviour i_mainBall = BallBehaviour.instance;
		if (i_mainBall.GetCurrentThrower() == this.linkedPlayer) { return; }
		if (ball == null)
		{
			if (Vector3.Distance(handTransform.position, i_mainBall.transform.position) > perfectReceptionMinDistance) { perfectReceptionBuffer += 0.2f; return; }
		} else
		{
			if (ballTimeInHand > perfectReceptionMinDelay) { perfectReceptionBuffer = 0; return; }
		}
		ExecutePerfectReception(i_mainBall);
	}

	public void ExecutePerfectReception(BallBehaviour _ball)
	{
		Receive(_ball);
		ChangePassState(PassState.Aiming);
		EnablePassPreview();
		StartCoroutine(ShootAfterDelay_C(perfectReceptionMinDelay - ballTimeInHand));
		didPerfectReception = true;
		_ball.AddNewDamageModifier(new DamageModifier(ballDatas.damageModifierOnPerfectReception, -1, DamageModifierSource.PerfectReception));
		_ball.AddNewSpeedModifier(new SpeedCoef(ballDatas.speedMultiplierOnPerfectReception, -1, SpeedMultiplierReason.PerfectReception, false));
		float i_lerpValue = (_ball.GetCurrentDamageModifier() - 1) / (ballDatas.maxDamageModifierOnPerfectReception - 1); //Used for color
		GameObject i_perfectReceptionFX = FeedbackManager.SendFeedback("event.PlayerPerfectReception", linkedPlayer, handTransform.position, _ball.GetCurrentDirection(), default).GetVFX();
		ParticleColorer.ReplaceParticleColor(i_perfectReceptionFX, Color.white, ballDatas.colorOverDamage.Evaluate(i_lerpValue));
		MomentumManager.IncreaseMomentum(MomentumManager.datas.momentumGainedOnPerfectReception);
		Collider[] i_hitColliders = Physics.OverlapSphere(transform.position, perfectReceptionExplosionRadius);
		PawnController i_pawnController = GetComponent<PawnController>();
		int i = 0;
		while (i < i_hitColliders.Length)
		{
			IHitable i_potentialHitableObject = i_hitColliders[i].GetComponentInParent<IHitable>();
			if (i_potentialHitableObject != null)
			{
				i_potentialHitableObject.OnHit(_ball, (i_hitColliders[i].transform.position - transform.position).normalized, i_pawnController, perfectReceptionExplosionDamages, DamageSource.PerfectReceptionExplosion);
			}
			i++;
		}
	}

	//Used for generating the preview
	public List<Vector3> GetBouncingPathCoordinates(Vector3 _startPosition, Vector3 _direction, float _maxLength)
	{
		RaycastHit hit;
		float i_remainingLength = _maxLength;
		List<Vector3> i_pathCoordinates = new List<Vector3>();
		i_pathCoordinates.Add(_startPosition);

		while (i_remainingLength > 0)
		{
			Vector3 i_actualPosition = i_pathCoordinates[i_pathCoordinates.Count - 1];
			if (Physics.Raycast(i_actualPosition, _direction, out hit, i_remainingLength, ~0, QueryTriggerInteraction.Ignore))
			{
				Debug.DrawRay(i_actualPosition, hit.normal, Color.red);
				_direction = Vector3.Reflect(_direction, hit.normal);
				Debug.DrawRay(hit.point, _direction, Color.blue);
				i_pathCoordinates.Add(hit.point);
				i_remainingLength -= Vector3.Distance(i_actualPosition, hit.point);
				if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Environment"))
				{
					break;
				}
				if (hit.collider.isTrigger) { continue; }
			} else
			{
				i_pathCoordinates.Add(i_actualPosition + _direction * i_remainingLength);
				i_remainingLength = 0;
			}
		}

		return i_pathCoordinates;
	}

	public List<Vector3> GetCurvedPathCoordinates(Vector3 _startPosition, Transform _target, Vector3 _lookDirection)
	{
		//Get the middle position for the curve
		Vector3 i_startPosition = _startPosition;
		Vector3 i_endPosition = _target.transform.position + Vector3.up;
		Vector3 i_direction = i_endPosition - i_startPosition;
		float i_lookDirectionAngle = Vector3.SignedAngle(new Vector3(i_direction.x, 0, i_direction.z), new Vector3(_lookDirection.x, 0, _lookDirection.z), Vector3.up);
		List<Vector3> i_coordinates = new List<Vector3>();
		_lookDirection.y = 0;

		Vector3 i_firstPoint = i_startPosition;
		Vector3 i_firstHandle = i_startPosition + _lookDirection.normalized * hanseLength;
		Vector3 i_secondPoint = i_endPosition;
		for (int i = 0; i < curveRaycastIteration; i++)
		{
			if (Mathf.Abs(i_lookDirectionAngle) > curveMinAngle)
			{
				i_coordinates.Add(SwissArmyKnife.CubicBezierCurve(i_firstPoint, i_firstHandle, i_secondPoint, i_secondPoint, i / curveRaycastIteration));
			}
			else
			{
				i_coordinates.Add(Vector3.Lerp(i_firstPoint ,i_endPosition, i / curveRaycastIteration));
			}
			if (i > 0)
			{
				foreach (RaycastHit hito in Physics.RaycastAll(i_coordinates[i - 1], i_coordinates[i] - i_coordinates[i - 1], Vector3.Distance(i_coordinates[i], i_coordinates[i - 1]))) {
					if (hito.collider.gameObject.layer == LayerMask.NameToLayer("Environment"))
					{
						i_coordinates.Add(hito.point);
						Vector3 _direction = Vector3.Reflect(i_coordinates[i] - i_coordinates[i - 1], hito.normal);
						_direction = _direction.normalized * 10;
						i_coordinates.Add(hito.point + _direction);
						return i_coordinates;
					}
					else if (hito.collider.gameObject.GetComponentInParent<Shield>() != null)
					{
						Vector3 impactVector = (i_coordinates[i] - i_coordinates[i - 1]);
						if ((impactVector.normalized + hito.collider.gameObject.transform.forward.normalized).magnitude >= (hito.collider.gameObject.GetComponentInParent<Shield>().enemy.angleRangeForRebound / 63.5f))
						{
							i_coordinates.Add(hito.point);
							Vector3 _direction = Vector3.Reflect(i_coordinates[i] - i_coordinates[i - 1], hito.normal);
							_direction = _direction.normalized * 10;
							i_coordinates.Add(hito.point + _direction);
							return i_coordinates;
						}
					}
				}
			}
		}
		return i_coordinates;
	}

	public void Aim()
	{
		ChangePassState(PassState.Aiming);
	}

	public void StopAim()
	{
		ChangePassState(PassState.None);
	}


	public PlayerController GetTarget ()
	{
		foreach (PlayerController p in FindObjectsOfType<PlayerController>())
		{
			if (p != linkedPlayer)
			{
				return p;
			}
		}
		return null;
	}

	public IEnumerator ShootAfterDelay_C(float _delay)
	{
		yield return new WaitForSeconds(_delay);
		didPerfectReception = false;
		FeedbackManager.SendFeedback("event.PlayerShootingAfterPerfectReception", linkedPlayer, handTransform.position, default, default); 
		Shoot();
	}
	public void Shoot()
	{
		if (!CanShoot()) { return; }
		if (didPerfectReception) { return; }
		ChangePassState(PassState.Shooting);
		FeedbackManager.SendFeedback("event.PlayerThrowingBall", linkedPlayer, handTransform.position, linkedPlayer.GetLookInput(), Vector3.zero); ;
		if (ballTimeInHand > perfectReceptionMinDelay + 0.1f) { BallBehaviour.instance.RemoveDamageModifier(DamageModifierSource.PerfectReception); BallBehaviour.instance.RemoveSpeedModifier(SpeedMultiplierReason.PerfectReception); }
		currentPassCooldown = passCooldown;
		BallBehaviour i_shotBall = ball;
		ball = null;
		didPerfectReception = false;
		MomentumManager.IncreaseMomentum(MomentumManager.datas.momentumGainedOnPass);
		MomentumManager.DisableMomentumExpontentialLoss();
		if (passMode == PassMode.Curve)
        {
            // Throw a curve pass
            if (otherPlayer != null)
            {
				if (passPreview)
				{
					i_shotBall.CurveShoot(this, linkedPlayer, otherPlayer.transform, ballDatas, linkedPlayer.GetLookInput());
				} else
				{
					//shotBall.CurveShoot(this, linkedPlayer, otherPlayer.transform, ballDatas, (otherPlayer.transform.position - transform.position).normalized);
					i_shotBall.Shoot(handTransform.position, otherPlayer.transform.position - transform.position, linkedPlayer, ballDatas, true);
				}
            }
        }
		if (passMode == PassMode.Bounce)
		{
			if (!passPreview)
			{
				if (otherPlayer != null)
				{
					i_shotBall.Shoot(handTransform.position, otherPlayer.transform.position - transform.position, linkedPlayer, ballDatas, true);    // shoot in direction of other player
				}
			}
			else // if aiming with right joystick
			{
				i_shotBall.Shoot(handTransform.position, SnapController.SnapDirection(handTransform.position, transform.forward), linkedPlayer, ballDatas, false);
			}
		}
		ChangePassState(PassState.None);
	}

	public void Receive (BallBehaviour _ball)
	{
		if (!canReceive) { return; }
		FeedbackManager.SendFeedback("event.PlayerReceivingBall", linkedPlayer, handTransform.position, _ball.GetCurrentDirection(), _ball.GetCurrentDirection());
		CursorManager.SetBallPointerParent(transform);
		ball = _ball;
		ball.GoToHands(handTransform, 0.2f,ballDatas) ;
		ballTimeInHand = 0;
		MomentumManager.EnableMomentumExponentialLoss(MomentumManager.datas.minPassDelayBeforeMomentumLoss, MomentumManager.datas.momentumLossSpeedIfNoPass);
		if (!linkedPlayer.IsTargetable()) { DropBall(); }
		if (linkedDunkController != null) { linkedDunkController.OnBallReceive(); }
	}

	public void EnablePassPreview()
	{
		passPreview = true;
	}

	public void EnableBallReception()
	{
		canReceive = true;
	}

	public void DisableBallReception()
	{
		canReceive = false;
	}

	public bool CanReceive()
	{
		return canReceive;
	}

	void ChangeColor ( Color _newColor )
	{
		Color i_newStartColor = _newColor;
		i_newStartColor.a = lineRenderer.startColor.a;
		lineRenderer.startColor = i_newStartColor;

		Color i_newEndColor = _newColor;
		i_newEndColor.a = lineRenderer.endColor.a;
		lineRenderer.endColor = i_newEndColor;
	}

	public void ResetPreviewColor()
	{
		lineRenderer.startColor = previewDefaultColor;
		lineRenderer.endColor = previewDefaultColor;
	}

	public void DisablePassPreview()
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

	public bool CanShoot()
	{
		if (ball == null || currentPassCooldown >= 0 || linkedDunkController.isDunking() || (!GetTarget().IsTargetable() && passMode == PassMode.Curve))
		{
			return false;
		} else
		{
			return true;
		}
	}

	public BallBehaviour GetBall()
	{
		return ball;
	}

	public void DropBall()
	{
		if (GetBall() == null) { return; }
		ball.transform.SetParent(null);
		ball.ChangeSpeed(0);
		ball.ChangeState(BallState.Grounded);
		ball = null;
	}

	public BallDatas GetBallDatas()
	{
		return ballDatas;
	}

	public Transform GetHandTransform()
	{
		return handTransform;
	}

	private void PreviewPathInEditor(List<Vector3> _pathCoordinates)
	{
		for (int i = 0; i < _pathCoordinates.Count - 1; i++)
		{
			Vector3 i_actualPosition = _pathCoordinates[i];
			Vector3 i_nextPosition = _pathCoordinates[i + 1];
			Vector3 i_dir = i_nextPosition - i_actualPosition;
			Debug.DrawRay(i_actualPosition, i_dir, Color.yellow);
		}
	}

	private void PreviewPath(List<Vector3> _pathCoordinates)
	{
		lineRenderer.positionCount = _pathCoordinates.Count;
		lineRenderer.SetPositions(_pathCoordinates.ToArray());
		float distance = GetPathTotalLength(_pathCoordinates);
		lineRenderer.materials[0].mainTextureScale = new Vector3(distance * (1f/lineRenderer.startWidth), 1, 1);
	}

	private float GetPathTotalLength(List<Vector3> _pathCoordinates)
	{
		float totalLength = 0;
		for (int i = 0; i < _pathCoordinates.Count - 1; i++)
		{
			Vector3 i_actualPosition = _pathCoordinates[i];
			Vector3 i_nextPosition = _pathCoordinates[i + 1];
			totalLength += Vector3.Distance(i_actualPosition, i_nextPosition);
		}
		return totalLength;
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

	public void ChangePassState ( PassState _newState )
	{
		//if (_newState == passState) { return; }
		previousState = passState;
		passState = _newState;
		switch (_newState)
		{
			case PassState.Aiming:
				if (!CanShoot()) { return; }
				EnablePassPreview();
				animator.SetTrigger("PrepareShootingTrigger");
				break;
			case PassState.Shooting:
				if (!CanShoot()) { return; }
				animator.SetTrigger("ShootingTrigger");
				return;
			case PassState.None:
				DisablePassPreview();
				animator.ResetTrigger("PrepareShootingTrigger");
				animator.SetTrigger("ShootingMissedTrigger");
				break;
		}
	}
}
