using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BallState {
	Grounded, //The ball is on the ground
	Flying, //The ball is in the air
	Aimed, //The ball is still in the hands of the thrower, who's aiming
}
public class BallBehaviour : MonoBehaviour
{
	[Header("Ball informations (Only debug, don't change)")]
	[SerializeField] private Vector3 currentDirection;
	[SerializeField] private float currentMaxDistance;
	[SerializeField] private float currentDistanceTravelled;
	[SerializeField] private float currentSpeed;
	[SerializeField] private BallState currentState;
	[SerializeField] private PassDatas currentPassDatas;
	[SerializeField] private int currentBounceCount;


	private Rigidbody rb;
	private int defaultLayer;

	private void Awake()
    {
		rb = GetComponent<Rigidbody>();
        defaultLayer = gameObject.layer;
    }

	private void Update ()
	{
		UpdateBallPosition();
	}

	public void Shoot(Vector3 _startPosition, Vector3 _direction, PassDatas _passDatas) //Shoot the ball toward a direction
	{
		transform.position = _startPosition;
		currentDirection = _direction;
		currentSpeed = _passDatas.moveSpeed;
		currentMaxDistance = _passDatas.maxDistance;
		currentPassDatas = _passDatas;
		ChangeState(BallState.Flying);
	}

	public void Bounce(Vector3 _newDirection, float _bounceSpeedMultiplier)
	{
		currentBounceCount++;
		currentDirection = _newDirection;
		currentSpeed = currentSpeed * _bounceSpeedMultiplier;
	}

	public void Attract(Vector3 _position, float attractionForce) //Attract the ball toward a specific point
	{

	}

	public void ChangeState(BallState newState)
	{
		switch (newState)
		{
			case BallState.Grounded:
				EnableGravity();
				EnableCollisions();
				break;
			case BallState.Aimed:
				DisableGravity();
				DisableCollisions();
				break;
			case BallState.Flying:
				DisableGravity();
				EnableCollisions();
				currentDistanceTravelled = 0;
				break;
		}
		currentState = newState;
	}

	private void UpdateBallPosition()
	{
		switch (currentState)
		{
			case BallState.Flying:
				if (currentSpeed <= 0)
				{
					ChangeState(BallState.Grounded);
				}
				transform.position += currentDirection.normalized * currentSpeed * Time.deltaTime;
				currentDistanceTravelled += currentSpeed * Time.deltaTime;
				if (currentDistanceTravelled >= currentMaxDistance)
				{
					//Ball has arrived to it's destination
					ChangeState(BallState.Grounded);
				}
				else
				{
					//Ball is going to it's destination, checking for collisions
					RaycastHit hit;
					if (Physics.Raycast(transform.position, currentDirection, out hit, 1))
					{
						Vector3 newDirection = Vector3.Reflect(currentDirection, hit.normal);
						Bounce(newDirection, currentPassDatas.speedMultiplierOnBounce);
					}
				}
				break;
		}
	}
	private void EnableGravity ()
	{
		rb.useGravity = true;
	}

	private void DisableGravity ()
	{
		rb.useGravity = false;
	}

	private void EnableCollisions ()
	{
		rb.isKinematic = false;
		gameObject.layer = defaultLayer;
	}
	private void DisableCollisions ()
	{
		rb.isKinematic = true;
		gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
	}
}
