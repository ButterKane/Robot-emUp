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
	[Separator("Global settings")]
	public bool passPreviewInEditor;
	public PassMode passMode;
	public Transform handTransform;
	public BallDatas ballDatas;
	public float passCooldown;
	public Color previewDefaultColor;
	public Color previewSnappedColor;

	public float delayBeforePickingOwnBall;
	public int minBouncesBeforePickingOwnBall;

	[Separator("Reception settings")]
	public float receptionMinDistance = 0.2f;
	public float receptionMinDelay = 0.2f;

	[ConditionalField(nameof(passMode), false, PassMode.Curve)] public float curveMaxLateralDistance;
	[ConditionalField(nameof(passMode), false, PassMode.Curve)] public float curveMaxPlayerDistance;
	[ConditionalField(nameof(passMode), false, PassMode.Curve)] public float curveRaycastIteration = 50;
	[ConditionalField(nameof(passMode), false, PassMode.Curve)] public float curveMinAngle = 10;
	[ConditionalField(nameof(passMode), false, PassMode.Curve)] public float hanseLength;

	private PlayerController linkedPlayer;
	private PlayerController otherPlayer;
	private DunkController linkedDunkController;
	private BallBehaviour ball;
	private LineRenderer lineRenderer;
	private List<Vector3> pathCoordinates;
	private bool passPreview;
	public PassState passState;
	private float currentPassCooldown;
	private Animator animator;
	private bool canReceive;
	private float ballTimeInHand;
	private bool didPerfectReception;
	private PassState previousState;

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
			bool snapped = false;
			switch (passMode)
			{
				case PassMode.Bounce:
					pathCoordinates = GetBouncingPathCoordinates(handTransform.position, SnapController.SnapDirection(handTransform.position, transform.forward, out snapped), ballDatas.maxPreviewDistance);
					break;
				case PassMode.Curve:
					pathCoordinates = GetCurvedPathCoordinates(handTransform.position, otherPlayer.transform, linkedPlayer.GetLookInput());
					break;
			}
			if (snapped) { ChangeColor(previewSnappedColor); } else { ChangeColor(previewDefaultColor); }
			PreviewPath(pathCoordinates);
			LockManager.LockTargetsInPath(pathCoordinates,0);
			if (passPreviewInEditor)
				PreviewPathInEditor(pathCoordinates);
		}

	}
	public void TryReception() //Player tries to do a perfect reception
	{
		if (didPerfectReception) { return; }
		BallBehaviour mainBall = BallBehaviour.instance;
		if (mainBall.GetCurrentThrower() == this.linkedPlayer) { return; }
		if (ball == null)
		{
			if (Vector3.Distance(handTransform.position, mainBall.transform.position) > receptionMinDistance) { return; }
		} else
		{
			if (ballTimeInHand > receptionMinDelay) { return; }
		}
		ball = mainBall;
		ChangePassState(PassState.Aiming);
		didPerfectReception = true;
		mainBall.AddNewDamageModifier(new DamageModifier(ballDatas.damageModifierOnReception, -1, DamageModifierSource.PerfectReception));
		FXManager.InstantiateFX(ballDatas.PerfectReception, handTransform.position, false, Vector3.zero, Vector3.one * 5);
		MomentumManager.IncreaseMomentum(MomentumManager.datas.momentumGainedOnPerfectReception);
	}

	//Used for generating the preview
	public List<Vector3> GetBouncingPathCoordinates(Vector3 _startPosition, Vector3 _direction, float _maxLength)
	{
		RaycastHit hit;
		float remainingLength = _maxLength;
		List<Vector3> pathCoordinates = new List<Vector3>();
		pathCoordinates.Add(_startPosition);

		while (remainingLength > 0)
		{
			Vector3 actualPosition = pathCoordinates[pathCoordinates.Count - 1];
			if (Physics.Raycast(actualPosition, _direction, out hit, remainingLength, ~0, QueryTriggerInteraction.Ignore))
			{
				Debug.DrawRay(actualPosition, hit.normal, Color.red);
				_direction = Vector3.Reflect(_direction, hit.normal);
				Debug.DrawRay(hit.point, _direction, Color.blue);
				pathCoordinates.Add(hit.point);
				remainingLength -= Vector3.Distance(actualPosition, hit.point);
				if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Environment"))
				{
					break;
				}
				if (hit.collider.isTrigger) { continue; }
			} else
			{
				pathCoordinates.Add(actualPosition + _direction * remainingLength);
				remainingLength = 0;
			}
		}

		return pathCoordinates;
	}

	public List<Vector3> GetCurvedPathCoordinates(Vector3 _startPosition, Transform _target, Vector3 _lookDirection)
	{
		//Get the middle position for the curve
		Vector3 startPosition = _startPosition;
		Vector3 endPosition = _target.transform.position + Vector3.up;
		Vector3 direction = endPosition - startPosition;
		float lookDirectionAngle = Vector3.SignedAngle(new Vector3(direction.x, 0, direction.z), new Vector3(_lookDirection.x, 0, _lookDirection.z), Vector3.up);
		List<Vector3> coordinates = new List<Vector3>();
		_lookDirection.y = 0;

		Vector3 firstPoint = startPosition;
		Vector3 firstHandle = startPosition + _lookDirection.normalized * hanseLength;
		Vector3 secondPoint = endPosition;
		for (float i = 0; i < curveRaycastIteration; i++)
		{
			if (Mathf.Abs(lookDirectionAngle) > curveMinAngle)
			{
				coordinates.Add(SwissArmyKnife.CubicBezierCurve(firstPoint, firstHandle, secondPoint, secondPoint, i / curveRaycastIteration));
			}
			else
			{
				coordinates.Add(Vector3.Lerp(firstPoint ,endPosition, i / curveRaycastIteration));
			}
		}
		return coordinates;
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

	public void Shoot()
	{
		if (!CanShoot()) { return; }
		ChangePassState(PassState.Shooting);
		if (ballTimeInHand >= receptionMinDelay) { BallBehaviour.instance.RemoveDamageModifier(DamageModifierSource.PerfectReception); }
		linkedPlayer.Vibrate(0.15f, VibrationForce.Medium);
		currentPassCooldown = passCooldown;
		BallBehaviour shotBall = ball;
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
					shotBall.CurveShoot(this, linkedPlayer, otherPlayer.transform, ballDatas, linkedPlayer.GetLookInput());
				} else
				{
					//shotBall.CurveShoot(this, linkedPlayer, otherPlayer.transform, ballDatas, (otherPlayer.transform.position - transform.position).normalized);
					shotBall.Shoot(handTransform.position, otherPlayer.transform.position - transform.position, linkedPlayer, ballDatas, true);
				}
            }
        }
		if (passMode == PassMode.Bounce)
		{
			if (!passPreview)
			{
				if (otherPlayer != null)
					shotBall.Shoot(handTransform.position, otherPlayer.transform.position - transform.position, linkedPlayer, ballDatas, true);    // shoot in direction of other player
			}
			else // if aiming with right joystick
			{
				shotBall.Shoot(handTransform.position, SnapController.SnapDirection(handTransform.position, transform.forward), linkedPlayer, ballDatas, false);
			}
		}
		ChangePassState(PassState.None);
	}

	public void Receive (BallBehaviour _ball)
	{
		if (!canReceive) { return; }
		CursorManager.SetBallPointerParent(transform);
		linkedPlayer.Vibrate(0.15f, VibrationForce.Medium);
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
		Color newStartColor = _newColor;
		newStartColor.a = lineRenderer.startColor.a;
		lineRenderer.startColor = newStartColor;

		Color newEndColor = _newColor;
		newEndColor.a = lineRenderer.endColor.a;
		lineRenderer.endColor = newEndColor;
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
			Vector3 actualPosition = _pathCoordinates[i];
			Vector3 nextPosition = _pathCoordinates[i + 1];
			Vector3 dir = nextPosition - actualPosition;
			Debug.DrawRay(actualPosition, dir, Color.yellow);
		}
	}

	private void PreviewPath(List<Vector3> _pathCoordinates)
	{
		lineRenderer.positionCount = _pathCoordinates.Count;
		lineRenderer.SetPositions(_pathCoordinates.ToArray());
		float distance = GetPathTotalLength(_pathCoordinates);
		lineRenderer.materials[0].mainTextureScale = new Vector3(distance * (1f/lineRenderer.startWidth), 1, 1);
	}

	private float GetPathTotalLength(List<Vector3> pathCoordinates)
	{
		float totalLength = 0;
		for (int i = 0; i < pathCoordinates.Count - 1; i++)
		{
			Vector3 actualPosition = pathCoordinates[i];
			Vector3 nextPosition = pathCoordinates[i + 1];
			totalLength += Vector3.Distance(actualPosition, nextPosition);
		}
		return totalLength;
	}
	private void UpdateCooldowns ()
	{
		if (currentPassCooldown >= 0)
		{
			currentPassCooldown -= Time.deltaTime;
		}
	}

	public void ChangePassState ( PassState _newState )
	{
		if (_newState == passState) { return; }
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
