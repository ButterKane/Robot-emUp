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

	private void OnTriggerEnter ( Collider other )
	{
		if (!playerController.enableMagnet || !passController.CanReceive())
		{
			return;
		}
		if (other.tag == "Ball")
		{
			BallBehaviour ball = other.GetComponent<BallBehaviour>();
			if (ball.GetCurrentDistanceTravelled() <= magnetRadius + 2 || ball.GetState() != BallState.Flying) { return; }
			if (ball.GetCurrentThrower() == playerController && (ball.GetCurrentBounceCount() < passController.minBouncesBeforePickingOwnBall || ball.GetTimeFlying() < passController.delayBeforePickingOwnBall )) { return; }

			ballInside = ball;
			ballInitialDirection = ball.GetCurrentDirection();
		}
	}
	private void OnTriggerStay(Collider other)
    {
		if (!playerController.enableMagnet || !passController.CanReceive())
		{
			return;
		}
		if (other.tag == "Ball")
        {
			BallBehaviour ball = other.GetComponent<BallBehaviour>();
			if (ball.GetCurrentDistanceTravelled() <= magnetRadius + 2 || ball.GetState() != BallState.Flying) { return; }
			if (ball.GetCurrentThrower() == playerController && (ball.GetCurrentBounceCount() < passController.minBouncesBeforePickingOwnBall || ball.GetTimeFlying() < passController.delayBeforePickingOwnBall)) { return; }

			float attractionForce = Vector3.Distance(other.transform.position, transform.position);
			attractionForce = attractionForce / magnetRadius; //Normalize attractionForce
			attractionForce = 1 - attractionCurve.Evaluate(attractionForce);

			Vector3 newDirection = Vector3.Lerp(ballInitialDirection, transform.position - ball.transform.position, attractionForce);
			ball.ChangeDirection(newDirection);
		}
    }

	private void OnTriggerExit ( Collider other )
	{
		if (!playerController.enableMagnet)
		{
			return;
		}
		if (other.tag == "Ball")
		{
			BallBehaviour ball = other.GetComponent<BallBehaviour>();
			if (ballInside == ball)
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
