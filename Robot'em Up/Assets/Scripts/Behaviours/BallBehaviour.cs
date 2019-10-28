using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BallState {
	Grounded, //The ball is on the ground
	Flying, //The ball is in the air
	Aimed, //The ball is still in the hands of the thrower, who's aiming
	Held //The ball is in the hands of a player
}
public class BallBehaviour : MonoBehaviour
{
	[Header("Ball informations (Only debug, don't change)")]
	[Space(2)]
	[SerializeField] private Vector3 currentDirection;
	[SerializeField] private float currentMaxDistance;
	[SerializeField] private float currentDistanceTravelled;
	[SerializeField] private float currentSpeed;
	[SerializeField] private BallState currentState;
	[SerializeField] private BallDatas currentBallDatas;
	[SerializeField] private int currentBounceCount;
	[SerializeField] private PawnController currentThrower;

	private Collider col;
	private Rigidbody rb;
	private int defaultLayer;
	private GameObject trailFX;
	private List<IHitable> hitGameObjects;
	private Vector3 previousPosition;

	private void Awake()
    {
		rb = GetComponent<Rigidbody>();
        defaultLayer = gameObject.layer;
		col = GetComponent<Collider>();
		hitGameObjects = new List<IHitable>();
    }

	private void FixedUpdate ()
	{
		UpdateBallPosition();
	}

	public void Shoot(Vector3 _startPosition, Vector3 _direction, PawnController _thrower, BallDatas _passDatas) //Shoot the ball toward a direction
	{
		transform.SetParent(null);
		transform.position = _startPosition;
		currentDirection = _direction;
		currentThrower = _thrower;
		currentSpeed = _passDatas.moveSpeed;
		currentMaxDistance = _passDatas.maxDistance;
		currentBallDatas = _passDatas;
		currentBounceCount = 0;

		hitGameObjects.Clear();
		ChangeState(BallState.Flying);
	}

	public void Bounce(Vector3 _newDirection, float _bounceSpeedMultiplier)
	{
		currentBounceCount++;
		currentDirection = _newDirection;
		currentSpeed = currentSpeed * _bounceSpeedMultiplier;
		hitGameObjects.Clear();
	}

	public void Attract(Vector3 _position, float attractionForce) //Attract the ball toward a specific point
	{
		_position.y = transform.position.y; //Ball will always stay in 2D plan
		currentDirection = Vector3.Lerp(currentDirection, _position - transform.position, attractionForce * Time.deltaTime);
		currentSpeed = 5f;
	}

	public void ChangeDirection(Vector3 _newDirection)
	{
		currentDirection = _newDirection;
	}

	public void GoToHands ( Transform _handTransform, float _travelDuration, BallDatas _passData )
	{
		currentBallDatas = _passData;
		ChangeState(BallState.Held);
		StartCoroutine(GoToPosition(_handTransform, _travelDuration));
		transform.SetParent(_handTransform);
	}

	public void CancelMovement()
	{
		ChangeState(BallState.Grounded);
	}

	public void MultiplySpeed(float _coef)
	{
		currentSpeed *= _coef;
	}

	public void ChangeMaxDistance(int _newMaxDistance) //Changing the distance will cause the ball to stop after travelling a certain distance
	{
		currentMaxDistance = _newMaxDistance;
	}

	public void ChangeSpeed(float _newSpeed)
	{
		currentSpeed = _newSpeed;
	}

	public void Explode (bool _lightExplosion)
	{
		if (_lightExplosion)
		{
			FXManager.InstantiateFX(currentBallDatas.LightExplosion, transform.position, false, Vector3.forward, Vector3.one, null);
		} else
		{
			FXManager.InstantiateFX(currentBallDatas.HeavyExplosion, transform.position, false, Vector3.forward, Vector3.one, null);
		}
	}

	public float GetCurrentDistanceTravelled()
	{
		return currentDistanceTravelled;
	}

	public Vector3 GetCurrentDirection()
	{
		return currentDirection;
	}

	public PawnController GetCurrentThrower()
	{
		return currentThrower;
	}

	public void ChangeState(BallState newState)
	{
		switch (newState)
		{
			case BallState.Grounded:
				EnableGravity();
				EnableCollisions();
				rb.AddForce(currentDirection.normalized * currentSpeed * rb.mass, ForceMode.Impulse);
				break;
			case BallState.Aimed:
				DisableGravity();
				DisableCollisions();
				break;
			case BallState.Flying:
				DisableGravity();
				EnableCollisions();
				currentDistanceTravelled = 0;
				if (trailFX == null)
				{
					trailFX = FXManager.InstantiateFX(currentBallDatas.Trail, Vector3.zero, true, Vector3.zero, Vector3.one, transform);
				}
				break;
			case BallState.Held:
				DisableGravity();
				DisableCollisions();
				FXManager.InstantiateFX(currentBallDatas.ReceiveCore, Vector3.zero, true, Vector3.zero,Vector3.one, transform);
				Destroy(trailFX);
				break;
		}
		currentState = newState;
	}

	public BallState GetState()
	{
		return currentState;
	}

	private void UpdateBallPosition()
	{
		switch (currentState)
		{
			case BallState.Flying:
				transform.position += currentDirection.normalized * currentSpeed * Time.deltaTime;
				currentDistanceTravelled += currentSpeed * Time.deltaTime;
				if (currentDistanceTravelled >= currentMaxDistance || currentSpeed <= 0)
				{
					//Ball has arrived to it's destination
					ChangeState(BallState.Grounded);
				}
				else
				{
					//Ball is going to it's destination, checking for collisions
					if (previousPosition == Vector3.zero) { previousPosition = transform.position; }
					RaycastHit hit;
					Debug.DrawRay(transform.position, currentDirection.normalized * currentSpeed * Time.deltaTime, Color.red);
					if (Physics.SphereCast(transform.position, 1f, currentDirection, out hit, currentSpeed * Time.deltaTime))
					{
						//transform.position = hit.point;
						IHitable potentialHitableObjectFound = hit.transform.GetComponent<IHitable>();
						if (potentialHitableObjectFound != null && !hitGameObjects.Contains(potentialHitableObjectFound))
						{
							hitGameObjects.Add(potentialHitableObjectFound);
							potentialHitableObjectFound.OnHit(this, currentDirection * currentSpeed, currentThrower, currentBallDatas.damages, DamageSource.Ball);
						}
						if (hit.collider.isTrigger || hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Player")) { break; }
						if (currentBounceCount < currentBallDatas.maxBounces)
						{
							Vector3 hitNormal = hit.normal;
							hitNormal.y = 0;
							Vector3 newDirection = Vector3.Reflect(currentDirection, hitNormal);
							newDirection.y = -currentDirection.y;
							Bounce(newDirection, currentBallDatas.speedMultiplierOnBounce);
							FXManager.InstantiateFX(currentBallDatas.WallHit, transform.position, false, Vector3.zero, Vector3.one);
						} else
						{
							ChangeState(BallState.Grounded);
						}
					}
				}
				previousPosition = transform.position;
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
		gameObject.layer = defaultLayer;
		col.isTrigger = false;
		rb.isKinematic = false;
	}
	private void DisableCollisions ()
	{
		gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
		col.isTrigger = true;
		rb.isKinematic = true;
	}

	IEnumerator GoToPosition(Transform _transform, float _travelDuration)
	{
		for (float i = 0; i < _travelDuration; i+=Time.deltaTime)
		{
			transform.position = Vector3.Lerp(transform.position, _transform.position, i / _travelDuration);
			yield return new WaitForEndOfFrame();
		}
		transform.position = _transform.position;
		transform.localScale = Vector3.one;
	}
}
