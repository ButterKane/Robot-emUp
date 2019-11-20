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
	[SerializeField] private bool canBounce;
	[SerializeField] private bool canHitWalls;
	[SerializeField] private List<Vector3> currentCurve;
	[SerializeField] private Vector3 initialLookDirection;
	[SerializeField] private List<DamageModifier> currentDamageModifiers;
	[SerializeField] private Color currentColor;
	[SerializeField] private bool teleguided;
	[SerializeField] private float currentTimeFlying;

	private Collider col;
	private Rigidbody rb;
	private int defaultLayer;
	private GameObject trailFX;
	private List<IHitable> hitGameObjects;
	private Vector3 previousPosition;
	private Coroutine ballCoroutine;
	public static BallBehaviour instance;

	private Vector3 currentPosition;
	private Vector3 startPosition;

	private void Awake()
    {
		if (instance == null)
		{
			instance = this;
		}

		currentDamageModifiers = new List<DamageModifier>();
		rb = GetComponent<Rigidbody>();
        defaultLayer = gameObject.layer;
		col = GetComponent<Collider>();
		hitGameObjects = new List<IHitable>();
		currentColor = new Color(122f/255f, 0, 122f/255f);
		UpdateColor();
	}

	private void Update ()
	{
		if (currentState == BallState.Flying)
		{
			currentTimeFlying += Time.deltaTime;
		}
		UpdateBallPosition();
		UpdateDamageModifiers();
		if (Input.GetKeyDown(KeyCode.K))
		{
			AddNewDamageModifier(new DamageModifier(1.2f, -1f, DamageModifierSource.PerfectReception));
		}
	}

    public void CurveShoot(PassController _passController, PawnController _thrower, Transform _target, BallDatas _passDatas, Vector3 _lookDirection) //Shoot a curve ball to reach a point
    {
		startPosition = _passController.GetHandTransform().position;
		transform.SetParent(null, true);
		transform.localScale = Vector3.one;
		currentThrower = _thrower;
		currentSpeed = _passDatas.moveSpeed;
		currentMaxDistance = Mathf.Infinity;
		currentBallDatas = _passDatas;
		currentBounceCount = 0;
		canBounce = true;
		canHitWalls = true;
		currentCurve = _passController.GetCurvedPathCoordinates(startPosition, _target, _lookDirection);
		currentTimeFlying = 0;
		initialLookDirection = _lookDirection;
		teleguided = false;

		hitGameObjects.Clear();
		ChangeState(BallState.Flying);
		UpdateColor();
	}


    public void Shoot(Vector3 _startPosition, Vector3 _direction, PawnController _thrower, BallDatas _passDatas, bool _teleguided) //Shoot the ball toward a direction
	{
		transform.SetParent(null, true);
		transform.localScale = Vector3.one;
		transform.position = _startPosition;
		currentDirection = _direction;
		currentThrower = _thrower;
		currentSpeed = _passDatas.moveSpeed;
		currentBallDatas = _passDatas;
		currentBounceCount = 0;
		currentTimeFlying = 0;
		currentCurve = null;
		canBounce = true;
		canHitWalls = true;
		teleguided = _teleguided;

		hitGameObjects.Clear();
		ChangeState(BallState.Flying);
		UpdateColor();
	}

	public void Bounce(Vector3 _newDirection, float _bounceSpeedMultiplier)
	{
		currentBounceCount++;
		currentDirection = _newDirection;
		currentDirection.y = 0;
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
		currentCurve = null;
		ChangeState(BallState.Held);
		ballCoroutine = StartCoroutine(GoToPosition(_handTransform, _travelDuration));
		transform.SetParent(_handTransform, true);
	}

	public void CancelMovement()
	{
		ChangeState(BallState.Grounded);
	}


    public void ResetBounds()
    {
        currentBounceCount = 0;
    }
    

    public void MultiplySpeed(float _coef)
	{
		currentSpeed *= _coef;
	}

	void UpdateDamageModifiers ()
	{
		List<DamageModifier> newDamageModifierList = new List<DamageModifier>();
		foreach (DamageModifier modifier in currentDamageModifiers)
		{
			if (modifier.duration <= -1) { newDamageModifierList.Add(modifier); continue; }
			modifier.duration -= Time.deltaTime;
			if (modifier.duration > 0)
			{
				newDamageModifierList.Add(modifier);
			}
		}
		currentDamageModifiers = newDamageModifierList;
	}

	void UpdateColor ()
	{
		if (currentBallDatas != null)
		{
			float lerpValue = GetCurrentDamageModifier()-1 / currentBallDatas.maxDamageModifier-1;
			Color newColor = currentBallDatas.colorOverDamage.Evaluate(lerpValue);
			SetColor(newColor);
		}
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

	public int GetCurrentBounceCount()
	{
		return currentBounceCount;
	}

	public float GetTimeFlying()
	{
		return currentTimeFlying;
	}

	public int GetCurrentDamages()
	{
		float damages = currentBallDatas.damages;
		float totalModifier = 1;
		foreach (DamageModifier modifier in currentDamageModifiers)
		{
			totalModifier *= modifier.multiplyCoef;
		}
		totalModifier = Mathf.Clamp(totalModifier, 0, currentBallDatas.maxDamageModifier);
		return Mathf.RoundToInt(damages * totalModifier);
	}

	public float GetCurrentDamageModifier ()
	{
		float totalModifier = 1;
		foreach (DamageModifier modifier in currentDamageModifiers)
		{
			totalModifier *= modifier.multiplyCoef;
		}
		return totalModifier;
	}

	void SetColor(Color _newColor)
	{
        // ParticleColorer.ReplaceParticleColor(gameObject, currentColor, _newColor);
        currentColor = _newColor;
	}

	public DamageModifier AddNewDamageModifier(DamageModifier _newModifier)
	{
		currentDamageModifiers.Add(_newModifier);
		UpdateColor();
		return _newModifier;
	}

	public void RemoveDamageModifier(DamageModifierSource _source)
	{
		List<DamageModifier> newModifierList = new List<DamageModifier>();
		foreach (DamageModifier modifier in currentDamageModifiers)
		{
			if (modifier.source != _source)
			{
				newModifierList.Add(modifier);
			}
		}
		currentDamageModifiers = newModifierList;
		UpdateColor();
	}

	public void ChangeState(BallState newState)
	{
		switch (newState)
		{
			case BallState.Grounded:
				EnableGravity();
				EnableCollisions();
				Destroy(trailFX);
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
				if (currentCurve != null)
				{
					AnimationCurve curveX;
					AnimationCurve curveY;
					AnimationCurve curveZ;
					float curveLength;
					PassController currentPassController = GetCurrentThrower().GetComponent<PassController>();
					if (currentPassController == null) { return; }
					ConvertCoordinatesToCurve(currentPassController.GetCurvedPathCoordinates(startPosition, currentPassController.GetTarget().transform, initialLookDirection), out curveX, out curveY, out curveZ, out curveLength);
					currentMaxDistance = curveLength;
					float positionOnCurve = currentDistanceTravelled / currentMaxDistance;
					if (positionOnCurve >= 0.95f) { ChangeState(BallState.Grounded); }
					Vector3 nextPosition = new Vector3(curveX.Evaluate(positionOnCurve + 0.1f), curveY.Evaluate(positionOnCurve + 0.1f), curveZ.Evaluate(positionOnCurve + 0.1f));
					currentDirection = nextPosition - transform.position;
				}
				if (teleguided)
				{
					PassController currentPassController = GetCurrentThrower().GetComponent<PassController>();
					if (currentPassController != null)
					{
						currentDirection = (currentPassController.GetTarget().transform.position-transform.position).normalized;
					}
				}

				if (currentSpeed <= 0)
				{
					currentCurve = null;
					ChangeState(BallState.Grounded);
				}
				else
				{
					//Ball is going to it's destination, checking for collisions
					if (previousPosition == Vector3.zero) { previousPosition = transform.position; }
					Debug.DrawRay(transform.position, currentDirection.normalized * currentSpeed * Time.deltaTime, Color.red);

					RaycastHit[] hitColliders = Physics.RaycastAll(transform.position, currentDirection, currentSpeed * Time.deltaTime * MomentumManager.GetValue(MomentumManager.datas.ballSpeedMultiplier) * 1.2f);
					foreach (RaycastHit raycast in hitColliders)
					{
						IHitable potentialHitableObjectFound = raycast.transform.GetComponent<IHitable>();
						if (potentialHitableObjectFound != null && !hitGameObjects.Contains(potentialHitableObjectFound))
						{
							hitGameObjects.Add(potentialHitableObjectFound);
							potentialHitableObjectFound.OnHit(this, currentDirection * currentSpeed, currentThrower, GetCurrentDamages(), DamageSource.Ball);
						}
						if (raycast.collider.isTrigger || raycast.collider.gameObject.layer == LayerMask.NameToLayer("Enemy") || raycast.collider.gameObject.layer == LayerMask.NameToLayer("Player")) { break; }

						if (!raycast.collider.isTrigger)
						{
							if (currentBounceCount < currentBallDatas.maxBounces && canBounce && canHitWalls)
							{
								if (currentCurve != null)
								{
									currentCurve = null;
									currentDistanceTravelled = 0;
								}
								Vector3 hitNormal = raycast.normal;
								hitNormal.y = 0;
								Vector3 newDirection = Vector3.Reflect(currentDirection, hitNormal);
								newDirection.y = -currentDirection.y;
								Bounce(newDirection, currentBallDatas.speedMultiplierOnBounce);
								FXManager.InstantiateFX(currentBallDatas.WallHit, transform.position, false, Vector3.zero, Vector3.one);
								return;
							}
							else if (canHitWalls)
							{
								ChangeState(BallState.Grounded);
								MomentumManager.DecreaseMomentum(MomentumManager.datas.momentumLossWhenBallHitTheGround);
								return;
							}
						}
					}
					RaycastHit[] previousColliders = Physics.RaycastAll(transform.position, -currentDirection, currentSpeed * Time.deltaTime * MomentumManager.GetValue(MomentumManager.datas.ballSpeedMultiplier) * 1.2f);
					foreach (RaycastHit raycast in previousColliders)
					{
						IHitable potentialHitableObjectFound = raycast.transform.GetComponent<IHitable>();
						if (potentialHitableObjectFound != null && !hitGameObjects.Contains(potentialHitableObjectFound))
						{
							hitGameObjects.Add(potentialHitableObjectFound);
							potentialHitableObjectFound.OnHit(this, currentDirection * currentSpeed, currentThrower, GetCurrentDamages(), DamageSource.Ball);
						}
					}
				}
				transform.position += currentDirection.normalized * currentSpeed * Time.deltaTime * MomentumManager.GetValue(MomentumManager.datas.ballSpeedMultiplier);
				currentDistanceTravelled += currentSpeed * Time.deltaTime * MomentumManager.GetValue(MomentumManager.datas.ballSpeedMultiplier);
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
		ballCoroutine = null;
	}

	public void ConvertCoordinatesToCurve(List<Vector3> _coordinates, out AnimationCurve _curveX, out AnimationCurve _curveY, out AnimationCurve _curveZ, out float _curveLength)
	{
		AnimationCurve curveX = new AnimationCurve();
		AnimationCurve curveY = new AnimationCurve();
		AnimationCurve curveZ = new AnimationCurve();
		float curveLength = 0;
		for (int i = 0; i < _coordinates.Count; i++)
		{
			float time = (float)i / (float)_coordinates.Count;
			Keyframe keyX = new Keyframe();
			keyX.value = _coordinates[i].x;
			keyX.time = time;
			curveX.AddKey(keyX);

			Keyframe keyY = new Keyframe();
			keyY.value = _coordinates[i].y;
			keyY.time = time;
			curveY.AddKey(keyY);

			Keyframe keyZ = new Keyframe();
			keyZ.value = _coordinates[i].z;
			keyZ.time = time;
			curveZ.AddKey(keyZ);

			if (i < _coordinates.Count - 1)
			{
				curveLength += Vector3.Distance(_coordinates[i], _coordinates[i + 1]);
			}
		}
		_curveX = curveX;
		_curveY = curveY;
		_curveZ = curveZ;
		_curveLength = curveLength;
	}
}
