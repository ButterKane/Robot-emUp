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
	[Header("Global Settings")]
	public bool passPreviewInEditor;
	public PassMode passMode;

	[Header("Pass settings")]
	public Transform handTransform;
	public BallDatas ballDatas;
	public float passCooldown;
	public Color previewDefaultColor;
	public Color previewSnappedColor;

	[ConditionalField(nameof(passMode), false, PassMode.Curve)] public float curveMaxLateralDistance;
	[ConditionalField(nameof(passMode), false, PassMode.Curve)] public float curveMaxPlayerDistance;
	[ConditionalField(nameof(passMode), false, PassMode.Curve)] public float curveRaycastIteration = 50;
	[ConditionalField(nameof(passMode), false, PassMode.Curve)] public float curveMinAngle = 10;
	[ConditionalField(nameof(passMode), false, PassMode.Curve)] public AnimationCurve curveYOLO_MDR;
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

	private void Awake ()
	{
		lineRenderer = GetComponent<LineRenderer>();
		linkedPlayer = GetComponent<PlayerController>();
		linkedDunkController = GetComponent<DunkController>();
		animator = GetComponentInChildren<Animator>();

		canReceive = true;
		lineRenderer.startColor = previewDefaultColor;
		lineRenderer.endColor = previewDefaultColor;

		otherPlayer = GetTarget();
	}
	private void Update ()
	{
		UpdateCooldowns();

		if (ballDatas == null) { return; }

		if (passPreview)
		{
			bool snapped = false;
			switch (passMode)
			{
				case PassMode.Bounce:
					pathCoordinates = GetBouncingPathCoordinates(handTransform.position, SnapController.SnapDirection(handTransform.position, transform.forward, out snapped), ballDatas.maxPreviewDistance);
					break;
				case PassMode.Curve:
					pathCoordinates = GetCurvedPathCoordinates(otherPlayer.transform, linkedPlayer.GetLookInput());
					break;
			}
			if (snapped) { ChangeColor(previewSnappedColor); } else { ChangeColor(previewDefaultColor); }
			PreviewPath(pathCoordinates);
			if (passPreviewInEditor)
				PreviewPathInEditor(pathCoordinates);
		}

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

	public List<Vector3> GetCurvedPathCoordinates(Transform _target, Vector3 _lookDirection)
	{
		//Get the middle position for the curve
		Vector3 startPosition = handTransform.position;
		Vector3 endPosition = _target.transform.position + Vector3.up;
		Vector3 direction = endPosition - startPosition;
		Vector3 lateralDirection = Quaternion.AngleAxis(90, Vector3.up) * direction.normalized;
		Vector3 upDirection = Quaternion.AngleAxis(-90, lateralDirection) * direction.normalized;
		float normalizedPlayerDistance = direction.magnitude / curveMaxPlayerDistance;
		float lateralDistance = Mathf.Lerp(0, curveMaxLateralDistance, normalizedPlayerDistance);
		Vector3 perp = Vector3.Cross(direction, _lookDirection);
		float dir = Vector3.Dot(perp, upDirection);
		float lateralSign = Mathf.Sign(dir);
		float lookDirectionAngle = Vector3.SignedAngle(new Vector3(direction.x, 0, direction.z), new Vector3(_lookDirection.x, 0, _lookDirection.z), Vector3.up);
		float magnitude = (Mathf.Tan(lookDirectionAngle * Mathf.Deg2Rad) * (direction/2)).magnitude;
		if (Mathf.Abs(lookDirectionAngle) < 90)
		{
			lateralDistance = Mathf.Clamp(lateralDistance, 0, magnitude);
		}
		Vector3 middlePosition = startPosition + (direction / 2f) + lateralDirection.normalized * lateralDistance * lateralSign;
		Debug.DrawRay(startPosition, new Vector3(_lookDirection.x, 0, _lookDirection.z) * direction.magnitude, Color.red);
		Debug.DrawRay(startPosition + (direction / 2f), upDirection.normalized * 2, Color.green);
		Debug.DrawRay(startPosition, direction, Color.blue);
		Debug.DrawRay(startPosition + (direction/2f), lateralDirection.normalized * lateralDistance * lateralSign, Color.blue);

		List<Vector3> coordinates = new List<Vector3>();
		_lookDirection.y = 0;

		//Get the first part of the curve
		Vector3 firstPoint = startPosition;
		Vector3 firstHandle = startPosition + _lookDirection.normalized * hanseLength;
		Vector3 secondPoint = middlePosition;
		Vector3 secondHandle = middlePosition - direction.normalized * 0.7f * direction.magnitude * curveYOLO_MDR.Evaluate(Mathf.Abs(lookDirectionAngle/180));
		Debug.DrawRay(firstPoint, firstHandle - firstPoint, Color.cyan);
		Debug.DrawRay(secondPoint, secondHandle - secondPoint, Color.cyan);
		for (float i = 0; i < curveRaycastIteration; i++)
		{
			if (Mathf.Abs(lookDirectionAngle) > curveMinAngle)
			{
				//coordinates.Add(SwissArmyKnife.CubicBezierCurve(firstPoint, firstHandle, secondHandle, secondPoint, i / curveRaycastIteration));
			} else
			{
				//coordinates.Add(Vector3.Lerp(firstPoint, firstPoint + (direction / 2), i / curveRaycastIteration));
			}
		}

		//Get the second part of the curve
		Vector3 N = lateralDirection.normalized;
		Vector3 perpendicular = new Vector3(-N.z, 0, N.x);

		Vector3 thirdPoint = middlePosition;
	    Vector3 thirdHandle = secondHandle - 2 * Vector3.Dot((secondHandle - middlePosition), perpendicular) * perpendicular;
		Vector3 fourthHandle = firstHandle - 2 * Vector3.Dot((firstHandle - middlePosition), perpendicular) * perpendicular;
		Vector3 fourthPoint = endPosition;
		Debug.DrawRay(thirdPoint, thirdHandle - thirdPoint, Color.cyan);
		Debug.DrawRay(fourthPoint, fourthHandle - fourthPoint, Color.cyan);
		for (float i = 0; i < curveRaycastIteration; i++)
		{
			if (Mathf.Abs(lookDirectionAngle) > curveMinAngle)
			{
				//coordinates.Add(SwissArmyKnife.CubicBezierCurve(thirdPoint, thirdHandle, fourthHandle, fourthPoint, i / curveRaycastIteration));
				coordinates.Add(SwissArmyKnife.CubicBezierCurve(firstPoint, firstHandle, fourthPoint, fourthPoint, i / curveRaycastIteration));
			}
			else
			{
				coordinates.Add(Vector3.Lerp(firstPoint + (direction / 2),endPosition, i / curveRaycastIteration));
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
		linkedPlayer.Vibrate(0.15f, VibrationForce.Medium);
		currentPassCooldown = passCooldown;
		BallBehaviour shotBall = ball;
		ball = null;
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
					shotBall.Shoot(handTransform.position, otherPlayer.transform.position - transform.position, linkedPlayer, ballDatas);
				}
            }
        }
		if (passMode == PassMode.Bounce)
		{
			if (!passPreview)
			{
				if (otherPlayer != null)
					shotBall.Shoot(handTransform.position, otherPlayer.transform.position - transform.position, linkedPlayer, ballDatas);    // shoot in direction of other player
			}
			else // if aiming with right joystick
			{
				shotBall.Shoot(handTransform.position, SnapController.SnapDirection(handTransform.position, transform.forward), linkedPlayer, ballDatas);
			}
		}
		ChangePassState(PassState.None);
	}

	public void Receive (BallBehaviour _ball)
	{
		if (!canReceive) { return; }
		linkedPlayer.Vibrate(0.15f, VibrationForce.Medium);
		ball = _ball;
		ball.GoToHands(handTransform, 0.2f,ballDatas) ;
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

	public void ChangeColor(Color _newColor)
	{
		lineRenderer.startColor = _newColor;
		lineRenderer.endColor = _newColor;
	}

	public void ResetPreviewColor()
	{
		lineRenderer.startColor = previewDefaultColor;
		lineRenderer.endColor = previewDefaultColor;
	}

	public void DisablePassPreview()
	{
		passPreview = false;
		lineRenderer.positionCount = 0;
	}

	public bool CanShoot()
	{
		if (ball == null || currentPassCooldown >= 0 || linkedDunkController.isDunking())
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
		passState = _newState;
	}
}
