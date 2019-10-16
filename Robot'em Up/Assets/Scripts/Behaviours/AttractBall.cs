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


	private void Update ()
	{
		UpdateRadius();
	}
	private void UpdateRadius()
	{
		GetComponent<SphereCollider>().radius = magnetRadius;
		magnetFX.transform.localScale = new Vector3(magnetRadius * 3.2f, magnetRadius * 3.2f, magnetFX.transform.localScale.z);
	}

	private void OnTriggerEnter ( Collider other )
	{
		if (other.tag == "Ball")
		{
			BallBehaviour ball = other.GetComponent<BallBehaviour>();
			if (ball.GetCurrentDistanceTravelled() <= magnetRadius + 1) { return; }

			ballInitialDirection = ball.GetCurrentDirection();
		}
	}
	private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Ball")
        {
			BallBehaviour ball = other.GetComponent<BallBehaviour>();
			if (ball.GetCurrentDistanceTravelled() <= magnetRadius + 1) { return; }

			float attractionForce = Vector3.Distance(other.transform.position, transform.position);
			attractionForce = attractionForce / magnetRadius; //Normalize attractionForce
			attractionForce = 1 - attractionCurve.Evaluate(attractionForce);

			Vector3 newDirection = Vector3.Lerp(ballInitialDirection, transform.position - ball.transform.position, attractionForce);
			ball.ChangeDirection(newDirection);
		}
    }
}
