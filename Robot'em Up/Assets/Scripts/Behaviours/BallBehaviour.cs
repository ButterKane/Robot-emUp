using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public enum BallState {
	Grounded, //The ball is on the ground
	Flying, //The ball is in the air
	Aimed, //The ball is still in the hands of the thrower, who's aiming
	Held //The ball is in the hands of a player
}
public class BallBehaviour : MonoBehaviour
{
	[Separator("Ball variables")]
	[Space(2)]
	[ReadOnly] [SerializeField] private Vector3 currentDirection;
	[ReadOnly] [SerializeField] private float currentMaxDistance;
	[ReadOnly] [SerializeField] private float currentDistanceTravelled;
	[ReadOnly] [SerializeField] private float currentSpeed;
	[ReadOnly] [SerializeField] private BallState currentState;
	[ReadOnly] [SerializeField] private BallDatas currentBallDatas;
	[ReadOnly] [SerializeField] private int currentBounceCount;
	[ReadOnly] [SerializeField] private PawnController currentThrower;
	[ReadOnly] public bool canBounce;
	[ReadOnly] [SerializeField] private bool canHitWalls;
	[ReadOnly] [SerializeField] private List<Vector3> currentCurve;
	[ReadOnly] [SerializeField] private Vector3 initialLookDirection;
	[ReadOnly] [SerializeField] private List<DamageModifier> currentDamageModifiers;
	[ReadOnly] [SerializeField] private List<SpeedCoef> currentSpeedModifiers;
	[ReadOnly] [SerializeField] private Color currentColor;
	[ReadOnly] [SerializeField] private bool teleguided;
	[ReadOnly] [SerializeField] private float currentTimeFlying;

	private Collider col;
	private Rigidbody rb;
	private int defaultLayer;
	private List<IHitable> hitGameObjects;
	private Vector3 previousPosition;
	private Coroutine ballCoroutine;
	public static BallBehaviour instance;
	private GameObject ballTrail;
	private Coroutine destroyTrailFX;

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
		if (ballCoroutine != null) { StopCoroutine(ballCoroutine); }
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
		if (ballCoroutine != null) { StopCoroutine(ballCoroutine); }
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
		List<DamageModifier> i_newDamageModifierList = new List<DamageModifier>();
		foreach (DamageModifier damageModifier in currentDamageModifiers)
		{
			if (damageModifier.duration <= -1) { i_newDamageModifierList.Add(damageModifier); continue; }
			damageModifier.duration -= Time.deltaTime;
			if (damageModifier.duration > 0)
			{
				i_newDamageModifierList.Add(damageModifier);
			}
		}
		currentDamageModifiers = i_newDamageModifierList;

		List<SpeedCoef> i_newSpeedModifierList = new List<SpeedCoef>();
		foreach (SpeedCoef speedModifier in currentSpeedModifiers)
		{
			if (speedModifier.duration <= -1) { i_newSpeedModifierList.Add(speedModifier); continue; }
			speedModifier.duration -= Time.deltaTime;
			if (speedModifier.duration > 0)
			{
				i_newSpeedModifierList.Add(speedModifier);
			}
		}
		currentSpeedModifiers = i_newSpeedModifierList;
	}

	void UpdateColor ()
	{
		if (currentBallDatas != null)
		{
			float i_lerpValue = (GetCurrentDamageModifier()-1) / (currentBallDatas.maxDamageModifierOnPerfectReception - 1);
			Color i_newColor = currentBallDatas.colorOverDamage.Evaluate(i_lerpValue);
			SetColor(i_newColor);
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
		float i_damages = currentBallDatas.damages;
		return Mathf.RoundToInt(i_damages * GetCurrentDamageModifier());
	}

	public float GetCurrentDamageModifier()
	{
		float i_perfectReceptionModifier = 1f;
		float i_otherModifier = 1;
		foreach (DamageModifier modifier in currentDamageModifiers)
		{
			if (modifier.source == DamageModifierSource.PerfectReception)
			{
				i_perfectReceptionModifier *= modifier.multiplyCoef;
			}
			else
			{
				i_otherModifier *= modifier.multiplyCoef;
			}
		}
		i_perfectReceptionModifier = Mathf.Clamp(i_perfectReceptionModifier, 0, currentBallDatas.maxDamageModifierOnPerfectReception);
		return (i_perfectReceptionModifier * i_otherModifier);
	}
	
	public float GetPerfectReceptionDamageModifier()
	{
		float i_perfectReceptionModifier = 1f;
		foreach (DamageModifier modifier in currentDamageModifiers)
		{
			if (modifier.source == DamageModifierSource.PerfectReception)
			{
				i_perfectReceptionModifier *= modifier.multiplyCoef;
			}
		}
		i_perfectReceptionModifier = Mathf.Clamp(i_perfectReceptionModifier, 0, currentBallDatas.maxDamageModifierOnPerfectReception);
		return i_perfectReceptionModifier;
	}

	public float GetCurrentSpeedModifier()
	{
		float i_otherModifier = 1f;
		float i_perfectReceptionModifier = 1f;
		foreach (SpeedCoef modifier in currentSpeedModifiers)
		{
			if (modifier.reason == SpeedMultiplierReason.PerfectReception)
			{
				i_perfectReceptionModifier *= modifier.speedCoef;
			} else
			{
				i_otherModifier *= modifier.speedCoef;
			}
		}
		i_perfectReceptionModifier = Mathf.Clamp(i_perfectReceptionModifier, 0, currentBallDatas.maxSpeedMultiplierOnPerfectReception);
		return i_perfectReceptionModifier * i_otherModifier;
	}

	void SetColor(Color _newColor)
	{
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
		List<SpeedCoef> i_newModifierList = new List<SpeedCoef>();
		foreach (SpeedCoef modifier in currentSpeedModifiers)
		{
			if (modifier.reason != _source)
			{
				i_newModifierList.Add(modifier);
			}
		}
		currentSpeedModifiers = i_newModifierList;
	}
	public DamageModifier AddNewDamageModifier(DamageModifier _newModifier)
	{
		currentDamageModifiers.Add(_newModifier);
		UpdateColor();
		return _newModifier;
	}

	public void RemoveDamageModifier(DamageModifierSource _source)
	{
		List<DamageModifier> i_newModifierList = new List<DamageModifier>();
		foreach (DamageModifier modifier in currentDamageModifiers)
		{
			if (modifier.source != _source)
			{
				i_newModifierList.Add(modifier);
			}
		}
		currentDamageModifiers = i_newModifierList;
		UpdateColor();
	}

	public void ChangeState(BallState _newState)
	{
		switch (_newState)
		{
			case BallState.Grounded:
				if (destroyTrailFX != null) { StopCoroutine(destroyTrailFX); Destroy(ballTrail); }
				if (ballTrail) { destroyTrailFX = StartCoroutine(DisableEmitterThenDestroyAfterDelay(ballTrail.GetComponent<ParticleSystem>(), 0.5f)); }
				AnalyticsManager.IncrementData("GroundedBallAmount");
				FeedbackManager.SendFeedback("event.BallGrounded", this);
				EnableGravity();
				EnableCollisions();
				rb.AddForce(currentDirection.normalized * currentSpeed * rb.mass, ForceMode.Impulse);
				CursorManager.SetBallPointerParent(transform);
				LockManager.UnlockAll();
				break;
			case BallState.Aimed:
				DisableGravity();
				DisableCollisions();
				break;
			case BallState.Flying:
				Highlighter.DetachBallFromPlayer();
				CursorManager.SetBallPointerParent(null);
				ballTrail = FeedbackManager.SendFeedback("event.BallFlying", this).GetVFX();
				Vector3 newBallScale = ballTrail.transform.localScale;
				newBallScale *= Mathf.Lerp(1f, currentBallDatas.maxFXSizeMultiplierOnPerfectReception, (GetPerfectReceptionDamageModifier() / currentBallDatas.maxDamageModifierOnPerfectReception));
				ballTrail.transform.localScale = newBallScale;
				DisableGravity();
				EnableCollisions();
				col.isTrigger = true;
				currentDistanceTravelled = 0;
				break;
			case BallState.Held:
				if (destroyTrailFX != null) { StopCoroutine(destroyTrailFX); Destroy(ballTrail); }
				if (ballTrail) { destroyTrailFX = StartCoroutine(DisableEmitterThenDestroyAfterDelay(ballTrail.GetComponent<ParticleSystem>(), 0.5f)); }
				DisableGravity();
				DisableCollisions();
				LockManager.UnlockAll();
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
		switch (currentState)
		{
			case BallState.Flying:
				if (currentCurve != null)
				{
					AnimationCurve i_curveX;
					AnimationCurve i_curveY;
					AnimationCurve i_curveZ;
					float i_curveLength;
					PassController i_currentPassController = GetCurrentThrower().GetComponent<PassController>();
					if (i_currentPassController == null) { return; }
					List<Vector3> i_pathCoordinates = i_currentPassController.GetCurvedPathCoordinates(startPosition, i_currentPassController.GetTarget().transform, initialLookDirection);
					ConvertCoordinatesToCurve(i_pathCoordinates, out i_curveX, out i_curveY, out i_curveZ, out i_curveLength);
					currentMaxDistance = i_curveLength;
					float i_positionOnCurve = currentDistanceTravelled / currentMaxDistance;
					LockManager.LockTargetsInPath(i_pathCoordinates, i_positionOnCurve);
					if (i_positionOnCurve >= 0.95f) { ChangeState(BallState.Grounded); LockManager.UnlockAll(); }
					Vector3 i_nextPosition = new Vector3(i_curveX.Evaluate(i_positionOnCurve + 0.1f), i_curveY.Evaluate(i_positionOnCurve + 0.1f), i_curveZ.Evaluate(i_positionOnCurve + 0.1f));
					currentDirection = i_nextPosition - transform.position;
				}
				if (teleguided)
				{
					PassController i_currentPassController = GetCurrentThrower().GetComponent<PassController>();
					if (i_currentPassController != null)
					{
						currentDirection = (i_currentPassController.GetTarget().transform.position-transform.position).normalized;
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

					RaycastHit[] i_hitColliders = Physics.RaycastAll(transform.position, currentDirection, currentSpeed * Time.deltaTime * MomentumManager.GetValue(MomentumManager.datas.ballSpeedMultiplier) * 1.2f * GetCurrentSpeedModifier());
					foreach (RaycastHit raycast in i_hitColliders)
					{
						IHitable i_potentialHitableObjectFound = raycast.transform.GetComponent<IHitable>();
						if (i_potentialHitableObjectFound != null && !hitGameObjects.Contains(i_potentialHitableObjectFound))
						{
							hitGameObjects.Add(i_potentialHitableObjectFound);
							i_potentialHitableObjectFound.OnHit(this, currentDirection * currentSpeed, currentThrower, GetCurrentDamages(), DamageSource.Ball);
						}
						if (raycast.collider.GetComponentInParent<Shield>() != null) {
							Debug.Log("Shield"); 
						}
						if (raycast.collider.isTrigger || raycast.collider.gameObject.layer != LayerMask.NameToLayer("Environment")) { break; }
						FeedbackManager.SendFeedback("event.WallHitByBall", raycast.transform, raycast.point, currentDirection, raycast.normal);
						if (currentBounceCount < currentBallDatas.maxBounces && canBounce && canHitWalls)
						{
							AnalyticsManager.IncrementData("BallBounceOnWallCount");
							Vector3 i_hitNormal = raycast.normal;
							i_hitNormal.y = 0;
							Vector3 i_newDirection = Vector3.Reflect(currentDirection, i_hitNormal);
							i_newDirection.y = -currentDirection.y;
							Bounce(i_newDirection, currentBallDatas.speedMultiplierOnBounce);
							return;
						}
						else if (canHitWalls)
						{
							ChangeState(BallState.Grounded);
							MomentumManager.DecreaseMomentum(MomentumManager.datas.momentumLossWhenBallHitTheGround);
							return;
						}
					}
					RaycastHit[] i_previousColliders = Physics.RaycastAll(transform.position, -currentDirection, currentSpeed * Time.deltaTime * MomentumManager.GetValue(MomentumManager.datas.ballSpeedMultiplier) * 1.2f);
					foreach (RaycastHit raycast in i_previousColliders)
					{
						IHitable i_potentialHitableObjectFound = raycast.transform.GetComponent<IHitable>();
						if (i_potentialHitableObjectFound != null && !hitGameObjects.Contains(i_potentialHitableObjectFound))
						{
							hitGameObjects.Add(i_potentialHitableObjectFound);
							i_potentialHitableObjectFound.OnHit(this, currentDirection * currentSpeed, currentThrower, GetCurrentDamages(), DamageSource.Ball);
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
		AnimationCurve i_curveX = new AnimationCurve();
		AnimationCurve i_curveY = new AnimationCurve();
		AnimationCurve i_curveZ = new AnimationCurve();
		float i_curveLength = 0;
		for (int i = 0; i < _coordinates.Count; i++)
		{
			float i_time = (float)i / (float)_coordinates.Count;
			Keyframe i_keyX = new Keyframe();
			i_keyX.value = _coordinates[i].x;
			i_keyX.time = i_time;
			i_curveX.AddKey(i_keyX);

			Keyframe i_keyY = new Keyframe();
			i_keyY.value = _coordinates[i].y;
			i_keyY.time = i_time;
			i_curveY.AddKey(i_keyY);

			Keyframe i_keyZ = new Keyframe();
			i_keyZ.value = _coordinates[i].z;
			i_keyZ.time = i_time;
			i_curveZ.AddKey(i_keyZ);

			if (i < _coordinates.Count - 1)
			{
				i_curveLength += Vector3.Distance(_coordinates[i], _coordinates[i + 1]);
			}
		}
		_curveX = i_curveX;
		_curveY = i_curveY;
		_curveZ = i_curveZ;
		_curveLength = i_curveLength;
	}

	IEnumerator DisableEmitterThenDestroyAfterDelay ( ParticleSystem _ps, float _delay)
	{
		var emission = _ps.emission;
		emission.enabled = false;
		yield return new WaitForSeconds(_delay);
		if (_ps != null)
		{
			Destroy(_ps.gameObject);
		}
	}
}
