using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
	private PlayerController linkedPlayer;
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
	}
	private void Update ()
	{
		UpdateCooldowns();

		if (ballDatas == null) { return; }

		if (passPreview)
		{
			bool snapped;
			pathCoordinates = GetPathCoordinates(handTransform.position, SnapController.SnapDirection(handTransform.position, transform.forward, out snapped), ballDatas.maxPreviewDistance);
			if (snapped) { ChangeColor(previewSnappedColor); } else { ChangeColor(previewDefaultColor); }
			PreviewPath(pathCoordinates, ballDatas);
			if (passPreviewInEditor)
				PreviewPathInEditor(pathCoordinates);
		}

	}

	//Used for generating the preview
	public List<Vector3> GetPathCoordinates(Vector3 _startPosition, Vector3 _direction, float _maxLength)
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

	public void Aim()
	{
		ChangePassState(PassState.Aiming);
	}

	public void StopAim()
	{
		ChangePassState(PassState.None);
	}


	PlayerController GetOtherPlayer ()
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
		BallBehaviour shootedBall = ball;
		ball = null;
		PlayerController otherPlayer = GetOtherPlayer();
		if (passMode == PassMode.Curve)
        {
            // Throw a curve pass
            if (otherPlayer != null)
            {
				if (passPreview)
				{
					float _angle = Vector3.SignedAngle(otherPlayer.transform.position - transform.position, transform.forward, Vector3.up);
					shootedBall.CurveShoot(handTransform.position, otherPlayer.transform.position, _angle, linkedPlayer, ballDatas);
				} else
				{
					shootedBall.Shoot(handTransform.position, otherPlayer.transform.position - transform.position, linkedPlayer, ballDatas);
				}
            }
        }
		if (passMode == PassMode.Bounce)
		{
			if (!passPreview)
			{
				if (otherPlayer != null)
					shootedBall.Shoot(handTransform.position, otherPlayer.transform.position - transform.position, linkedPlayer, ballDatas);    // shoot in direction of other player
			}
			else // if aiming with right joystick
			{
				shootedBall.Shoot(handTransform.position, SnapController.SnapDirection(handTransform.position, transform.forward), linkedPlayer, ballDatas);
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

	private void PreviewPath(List<Vector3> _pathCoordinates, BallDatas _passDatas)
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
