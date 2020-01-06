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
	 public bool canBounce;
	[SerializeField] private bool canHitWalls;
	[SerializeField] private List<Vector3> currentCurve;
	[SerializeField] private Vector3 initialLookDirection;
	[SerializeField] private List<DamageModifier> currentDamageModifiers;
	[SerializeField] private List<SpeedCoef> currentSpeedModifiers;
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

		currentSpeedModifiers = new List<SpeedCoef>();
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
		UpdateModifiers();
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
		SoundManager.PlaySound("BallRebound", transform.position, transform);
		FeedbackManager.SendFeedback("event.ReboundOnWalls", this);
		CursorManager.SetBallPointerParent(transform);
		currentCurve = null;
		currentDistanceTravelled = 0;
		currentBounceCount++;
		currentDirection = _newDirection;
		currentDirection.y = 0;
		currentSpeed = currentSpeed * _bounceSpeedMultiplier;
		teleguided = false;
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

	void UpdateModifiers ()
	{
		List<DamageModifier> internal_newDamageModifierList = new List<DamageModifier>();
		foreach (DamageModifier damageModifier in currentDamageModifiers)
		{
			if (damageModifier.duration <= -1) { internal_newDamageModifierList.Add(damageModifier); continue; }
			damageModifier.duration -= Time.deltaTime;
			if (damageModifier.duration > 0)
			{
				internal_newDamageModifierList.Add(damageModifier);
			}
		}
		currentDamageModifiers = internal_newDamageModifierList;

		List<SpeedCoef> internal_newSpeedModifierList = new List<SpeedCoef>();
		foreach (SpeedCoef speedModifier in currentSpeedModifiers)
		{
			if (speedModifier.duration <= -1) { internal_newSpeedModifierList.Add(speedModifier); continue; }
			speedModifier.duration -= Time.deltaTime;
			if (speedModifier.duration > 0)
			{
				internal_newSpeedModifierList.Add(speedModifier);
			}
		}
		currentSpeedModifiers = internal_newSpeedModifierList;
	}

	void UpdateColor ()
	{
		if (currentBallDatas != null)
		{
			float internal_lerpValue = (GetCurrentDamageModifier()-1) / (currentBallDatas.maxDamageModifierOnPerfectReception - 1);
			Color internal_newColor = currentBallDatas.colorOverDamage.Evaluate(internal_lerpValue);
			SetColor(internal_newColor);
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
			FXManager.InstantiateFX(currentBallDatas.lightExplosion, transform.position, false, Vector3.forward, Vector3.one, null);
		} else
		{
			FXManager.InstantiateFX(currentBallDatas.heavyExplosion, transform.position, false, Vector3.forward, Vector3.one, null);
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
		float internal_damages = currentBallDatas.damages;
		return Mathf.RoundToInt(internal_damages * GetCurrentDamageModifier());
	}

	public float GetCurrentDamageModifier()
	{
		float internal_perfectReceptionModifier = 1f;
		float internal_otherModifier = 1;
		foreach (DamageModifier modifier in currentDamageModifiers)
		{
			if (modifier.source == DamageModifierSource.PerfectReception)
			{
				internal_perfectReceptionModifier *= modifier.multiplyCoef;
			}
			else
			{
				internal_otherModifier *= modifier.multiplyCoef;
			}
		}
		internal_perfectReceptionModifier = Mathf.Clamp(internal_perfectReceptionModifier, 0, currentBallDatas.maxDamageModifierOnPerfectReception);
		return (internal_perfectReceptionModifier * internal_otherModifier);
	}

	public float GetCurrentSpeedModifier()
	{
		float internal_otherModifier = 1f;
		float internal_perfectReceptionModifier = 1f;
		foreach (SpeedCoef modifier in currentSpeedModifiers)
		{
			if (modifier.reason == SpeedMultiplierReason.PerfectReception)
			{
				internal_perfectReceptionModifier *= modifier.speedCoef;
			} else
			{
				internal_otherModifier *= modifier.speedCoef;
			}
		}
		internal_perfectReceptionModifier = Mathf.Clamp(internal_perfectReceptionModifier, 0, currentBallDatas.maxSpeedMultiplierOnPerfectReception);
		return internal_perfectReceptionModifier * internal_otherModifier;
	}

	void SetColor(Color _newColor)
	{
		if (trailFX != null)
		{
			ParticleColorer.ReplaceParticleColor(trailFX, new Color(122f / 255f, 0, 122f / 255f), _newColor);
		}
        ParticleColorer.ReplaceParticleColor(gameObject, currentColor, _newColor);
        currentColor = _newColor;
	}

	public SpeedCoef AddNewSpeedModifier(SpeedCoef _newModifier)
	{
		currentSpeedModifiers.Add(_newModifier);
		return _newModifier;
	}

	public void RemoveSpeedModifier(SpeedMultiplierReason _source)
	{
		List<SpeedCoef> internal_newModifierList = new List<SpeedCoef>();
		foreach (SpeedCoef modifier in currentSpeedModifiers)
		{
			if (modifier.reason != _source)
			{
				internal_newModifierList.Add(modifier);
			}
		}
		currentSpeedModifiers = internal_newModifierList;
	}
	public DamageModifier AddNewDamageModifier(DamageModifier _newModifier)
	{
		currentDamageModifiers.Add(_newModifier);
		UpdateColor();
		return _newModifier;
	}

	public void RemoveDamageModifier(DamageModifierSource _source)
	{
		List<DamageModifier> internal_newModifierList = new List<DamageModifier>();
		foreach (DamageModifier modifier in currentDamageModifiers)
		{
			if (modifier.source != _source)
			{
				internal_newModifierList.Add(modifier);
			}
		}
		currentDamageModifiers = internal_newModifierList;
		UpdateColor();
	}

	public void ChangeState(BallState _newState)
	{
		switch (_newState)
		{
			case BallState.Grounded:
				EnableGravity();
				EnableCollisions();
				Destroy(trailFX);
				rb.AddForce(currentDirection.normalized * currentSpeed * rb.mass, ForceMode.Impulse);
				CursorManager.SetBallPointerParent(transform);
				FeedbackManager.SendFeedback("event.BallFallOnGround", this);
				SoundManager.PlaySound("BallFallOnTheGround", transform.position, transform);
				LockManager.UnlockAll();
				break;
			case BallState.Aimed:
				DisableGravity();
				DisableCollisions();
				break;
			case BallState.Flying:
				CursorManager.SetBallPointerParent(null);
				DisableGravity();
				EnableCollisions();
				currentDistanceTravelled = 0;
				if (trailFX == null)
				{
					trailFX = FXManager.InstantiateFX(currentBallDatas.trail, Vector3.zero, true, Vector3.zero, Vector3.one, transform);
					UpdateColor();
					trailFX.name = "FX_CoreTrail";
				}
				break;
			case BallState.Held:
				DisableGravity();
				DisableCollisions();
				LockManager.UnlockAll();
				FXManager.InstantiateFX(currentBallDatas.receiveCore, Vector3.zero, true, Vector3.zero,Vector3.one, transform);
				Destroy(trailFX);
				break;
		}
		currentState = _newState;
	}

	public BallState GetState()
	{
		return currentState;
	}

	private void UpdateBallPosition()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			Bounce(Vector3.left * 10, currentBallDatas.speedMultiplierOnBounce);
		}
		switch (currentState)
		{
			case BallState.Flying:
				if (currentCurve != null)
				{
					AnimationCurve internal_curveX;
					AnimationCurve internal_curveY;
					AnimationCurve internal_curveZ;
					float internal_curveLength;
					PassController internal_currentPassController = GetCurrentThrower().GetComponent<PassController>();
					if (internal_currentPassController == null) { return; }
					List<Vector3> internal_pathCoordinates = internal_currentPassController.GetCurvedPathCoordinates(startPosition, internal_currentPassController.GetTarget().transform, initialLookDirection);
					ConvertCoordinatesToCurve(internal_pathCoordinates, out internal_curveX, out internal_curveY, out internal_curveZ, out internal_curveLength);
					currentMaxDistance = internal_curveLength;
					float internal_positionOnCurve = currentDistanceTravelled / currentMaxDistance;
					LockManager.LockTargetsInPath(internal_pathCoordinates, internal_positionOnCurve);
					if (internal_positionOnCurve >= 0.95f) { ChangeState(BallState.Grounded); LockManager.UnlockAll(); }
					Vector3 internal_nextPosition = new Vector3(internal_curveX.Evaluate(internal_positionOnCurve + 0.1f), internal_curveY.Evaluate(internal_positionOnCurve + 0.1f), internal_curveZ.Evaluate(internal_positionOnCurve + 0.1f));
					currentDirection = internal_nextPosition - transform.position;
				}
				if (teleguided)
				{
					PassController internal_currentPassController = GetCurrentThrower().GetComponent<PassController>();
					if (internal_currentPassController != null)
					{
						currentDirection = (internal_currentPassController.GetTarget().transform.position-transform.position).normalized;
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

					RaycastHit[] internal_hitColliders = Physics.RaycastAll(transform.position, currentDirection, currentSpeed * Time.deltaTime * MomentumManager.GetValue(MomentumManager.datas.ballSpeedMultiplier) * 1.2f * GetCurrentSpeedModifier());
					foreach (RaycastHit raycast in internal_hitColliders)
					{
						IHitable internal_potentialHitableObjectFound = raycast.transform.GetComponent<IHitable>();
						if (internal_potentialHitableObjectFound != null && !hitGameObjects.Contains(internal_potentialHitableObjectFound))
						{
							hitGameObjects.Add(internal_potentialHitableObjectFound);
							internal_potentialHitableObjectFound.OnHit(this, currentDirection * currentSpeed, currentThrower, GetCurrentDamages(), DamageSource.Ball);
						}
						if (raycast.collider.GetComponentInParent<Shield>() != null) {
							Debug.Log("Shield"); 
						}
						if (raycast.collider.isTrigger || raycast.collider.gameObject.layer != LayerMask.NameToLayer("Environment")) { break; }
						if (currentBounceCount < currentBallDatas.maxBounces && canBounce && canHitWalls)
						{
							Vector3 internal_hitNormal = raycast.normal;
							internal_hitNormal.y = 0;
							Vector3 internal_newDirection = Vector3.Reflect(currentDirection, internal_hitNormal);
							internal_newDirection.y = -currentDirection.y;
							Bounce(internal_newDirection, currentBallDatas.speedMultiplierOnBounce);
							FXManager.InstantiateFX(currentBallDatas.wallHit, transform.position, false, -currentDirection, Vector3.one * 2.75f);
							FeedbackManager.SendFeedback("event.WallHitByBall", raycast.collider.gameObject);
							return;
						}
						else if (canHitWalls)
						{
							ChangeState(BallState.Grounded);
							MomentumManager.DecreaseMomentum(MomentumManager.datas.momentumLossWhenBallHitTheGround);
							return;
						}
					}
					RaycastHit[] internal_previousColliders = Physics.RaycastAll(transform.position, -currentDirection, currentSpeed * Time.deltaTime * MomentumManager.GetValue(MomentumManager.datas.ballSpeedMultiplier) * 1.2f);
					foreach (RaycastHit raycast in internal_previousColliders)
					{
						IHitable internal_potentialHitableObjectFound = raycast.transform.GetComponent<IHitable>();
						if (internal_potentialHitableObjectFound != null && !hitGameObjects.Contains(internal_potentialHitableObjectFound))
						{
							hitGameObjects.Add(internal_potentialHitableObjectFound);
							internal_potentialHitableObjectFound.OnHit(this, currentDirection * currentSpeed, currentThrower, GetCurrentDamages(), DamageSource.Ball);
						}
					}
				}
				transform.position += currentDirection.normalized * currentSpeed * Time.deltaTime * MomentumManager.GetValue(MomentumManager.datas.ballSpeedMultiplier) * GetCurrentSpeedModifier();
				currentDistanceTravelled += currentSpeed * Time.deltaTime * MomentumManager.GetValue(MomentumManager.datas.ballSpeedMultiplier) * GetCurrentSpeedModifier();
				if (currentCurve == null && !teleguided && currentDistanceTravelled >= currentMaxDistance)
				{
					ChangeState(BallState.Grounded);
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

	public void PrintDecal(RaycastHit _ray, Vector3 _direction )
	{
		Debug.Log("Spawning decal");
	}

	public void ConvertCoordinatesToCurve(List<Vector3> _coordinates, out AnimationCurve _curveX, out AnimationCurve _curveY, out AnimationCurve _curveZ, out float _curveLength)
	{
		AnimationCurve internal_curveX = new AnimationCurve();
		AnimationCurve internal_curveY = new AnimationCurve();
		AnimationCurve internal_curveZ = new AnimationCurve();
		float internal_curveLength = 0;
		for (int i = 0; i < _coordinates.Count; i++)
		{
			float internal_time = (float)i / (float)_coordinates.Count;
			Keyframe internal_keyX = new Keyframe();
			internal_keyX.value = _coordinates[i].x;
			internal_keyX.time = internal_time;
			internal_curveX.AddKey(internal_keyX);

			Keyframe internal_keyY = new Keyframe();
			internal_keyY.value = _coordinates[i].y;
			internal_keyY.time = internal_time;
			internal_curveY.AddKey(internal_keyY);

			Keyframe internal_keyZ = new Keyframe();
			internal_keyZ.value = _coordinates[i].z;
			internal_keyZ.time = internal_time;
			internal_curveZ.AddKey(internal_keyZ);

			if (i < _coordinates.Count - 1)
			{
				internal_curveLength += Vector3.Distance(_coordinates[i], _coordinates[i + 1]);
			}
		}
		_curveX = internal_curveX;
		_curveY = internal_curveY;
		_curveZ = internal_curveZ;
		_curveLength = internal_curveLength;
	}
}
