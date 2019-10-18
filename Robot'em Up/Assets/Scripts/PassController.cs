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
public class PassController : MonoBehaviour
{
	[Header("Global Settings")]
	public bool passPreviewInEditor;
	public PassMode passMode;

	[Header("Pass settings")]
	public Transform handTransform;
	public BallDatas ballDatas;
	public float passCooldown;


	private PlayerController linkedPlayerController;
	private DunkController linkedDunkController;
	private BallBehaviour ball;
	private LineRenderer lineRenderer;
	private List<Vector3> pathCoordinates;
	private bool passPreview;
	private float currentPassCooldown;

	private void Awake ()
	{
		lineRenderer = GetComponent<LineRenderer>();
		linkedPlayerController = GetComponent<PlayerController>();
		linkedDunkController = GetComponent<DunkController>();
	}
	private void Update ()
	{
		UpdateCooldowns();

		if (ballDatas == null) { return; }
		pathCoordinates = GetPathCoordinates(handTransform.position, transform.forward, ballDatas.maxPreviewDistance);

		if (passPreviewInEditor)
			PreviewPathInEditor(pathCoordinates);

		if (passPreview)
			PreviewPath(pathCoordinates, ballDatas);
	}
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
	public void Shoot()
	{
		linkedPlayerController.Vibrate(0.15f, VibrationForce.Medium);
		currentPassCooldown = passCooldown;
		BallBehaviour shootedBall = ball;
		ball = null;
		if (!passPreview)
		{
			PlayerController otherPlayer = null;
			foreach (PlayerController p in FindObjectsOfType<PlayerController>())
			{
				if (p != linkedPlayerController)
				{
					otherPlayer = p;
				}
			}
			if (otherPlayer != null)
				shootedBall.Shoot(handTransform.position, otherPlayer.transform.position - transform.position, linkedPlayerController, ballDatas);
		}
		else
		{
			shootedBall.Shoot(handTransform.position, transform.forward, linkedPlayerController, ballDatas);
		}
	}

	public void Receive (BallBehaviour _ball)
	{
		linkedPlayerController.Vibrate(0.15f, VibrationForce.Medium);
		ball = _ball;
		ball.GoToHands(handTransform, 0.2f,ballDatas) ;
		if (linkedDunkController != null) { linkedDunkController.OnBallReceive(); }
	}

	public void EnablePassPreview()
	{
		passPreview = true;
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
}
