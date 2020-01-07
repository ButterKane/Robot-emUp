using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AttractBall : MonoBehaviour
{
	[Header("Settings")]
	public AnimationCurve attractionCurve;
	public float magnetRadius;

	public GameObject magnetFX;

	private Vector3 ballInitialDirection;
	private BallBehaviour ballInside;
	private LineRenderer lineRenderer;
	private PassController passController;
	private PlayerController playerController;

	private void Awake ()
	{
		playerController = GetComponentInParent<PlayerController>();
		lineRenderer = GetComponent<LineRenderer>();
		passController = GetComponentInParent<PassController>();
	}
	private void Update ()
	{
		if (!playerController.enableMagnet)
		{
			return;
		}
		UpdateRadius();
		UpdateFX();
		if (passController.GetBall() != null)
		{
			ballInside = null;
		}
	}
	private void UpdateRadius()
	{
		GetComponent<SphereCollider>().radius = magnetRadius;
		magnetFX.transform.localScale = new Vector3(magnetRadius * 3.2f, magnetRadius * 3.2f, magnetFX.transform.localScale.z);
	}

	private void OnTriggerEnter ( Collider _other )
	{
		if (!playerController.enableMagnet || !passController.CanReceive())
		{
			return;
		}
		if (_other.tag == "Ball")
		{
			BallBehaviour internal_ball = _other.GetComponent<BallBehaviour>();
			if (internal_ball.GetCurrentDistanceTravelled() <= magnetRadius + 2 || internal_ball.GetState() != BallState.Flying) { return; }
			if (internal_ball.GetCurrentThrower() == playerController && (internal_ball.GetCurrentBounceCount() < passController.minBouncesBeforePickingOwnBall || internal_ball.GetTimeFlying() < passController.delayBeforePickingOwnBall )) { return; }

			ballInside = internal_ball;
			ballInitialDirection = internal_ball.GetCurrentDirection();
		}
	}
	private void OnTriggerStay(Collider _other)
    {
		if (!playerController.enableMagnet || !passController.CanReceive())
		{
			return;
		}
		if (_other.tag == "Ball")
        {
			BallBehaviour internal_ball = _other.GetComponent<BallBehaviour>();
			if (internal_ball.GetCurrentDistanceTravelled() <= magnetRadius + 2 || internal_ball.GetState() != BallState.Flying) { return; }
			if (internal_ball.GetCurrentThrower() == playerController && (internal_ball.GetCurrentBounceCount() < passController.minBouncesBeforePickingOwnBall || internal_ball.GetTimeFlying() < passController.delayBeforePickingOwnBall)) { return; }

			float internal_attractionForce = Vector3.Distance(_other.transform.position, transform.position);
			internal_attractionForce = internal_attractionForce / magnetRadius; //Normalize attractionForce
			internal_attractionForce = 1 - attractionCurve.Evaluate(internal_attractionForce);

			Vector3 internal_newDirection = Vector3.Lerp(ballInitialDirection, transform.position - internal_ball.transform.position, internal_attractionForce);
			internal_ball.ChangeDirection(internal_newDirection);
		}
    }

	private void OnTriggerExit ( Collider _other )
	{
		if (!playerController.enableMagnet)
		{
			return;
		}
		if (_other.tag == "Ball")
		{
			BallBehaviour internal_ball = _other.GetComponent<BallBehaviour>();
			if (ballInside == internal_ball)
			{
				ballInside = null;
			}
		}
	}

	private void UpdateFX()
	{
		if (ballInside != null && lineRenderer != null)
		{
			magnetFX.SetActive(true);
			//Draw FX between ball and this

			lineRenderer.positionCount = 2;
			lineRenderer.SetPosition(0, transform.position);
			lineRenderer.SetPosition(1, ballInside.transform.position);
		} else
		{
			magnetFX.SetActive(false);
			lineRenderer.positionCount = 0;
		}
	}
}
