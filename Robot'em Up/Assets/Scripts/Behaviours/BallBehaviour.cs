using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public enum BallState {
	Grounded, //The ball is on the ground
	Flying, //The ball is in the air
	Aimed, //The ball is still in the hands of the thrower, who's aiming
	Held //The ball is in the hands of a player
}
public class BallBehaviour : MonoBehaviour
{

    public bool isGhostBall = false;
    public class BallInformations
	{
		public Vector3 direction;
		public float maxDistance;
		public float distanceTravelled;
		public float moveSpeed;
		public BallState state;
		public BallDatas ballDatas;
		public int bounceCount;
		public PawnController thrower;
		public bool canBounce;
		public bool canHitWalls;
		public List<Vector3> curve;
		public Vector3 initialLookDirection;
		public List<DamageModifier> damageModifiers;
		public List<SpeedCoef> speedModifiers;
		public Color color;
		public bool isTeleguided;
		public float timeFlying;
	}

	public static BallBehaviour instance;
	public BallInformations ballInformations;

	private Collider col;
	private Rigidbody rb;
	private int defaultLayer;
	private List<IHitable> hitGameObjects;
	private Coroutine ballCoroutine;
	private GameObject ballTrail;
	private Coroutine destroyTrailFX; 
	private float outOfScreenTime;

	private AnimationCurve curveX;
	private AnimationCurve curveY;
	private AnimationCurve curveZ;

	private Vector3 startPosition;

	private void Awake()
    {
		if (instance == null)
		{
			instance = this;
		}
		InitBallInformation();
		rb = GetComponent<Rigidbody>();
        defaultLayer = gameObject.layer;
		col = GetComponent<Collider>();
		hitGameObjects = new List<IHitable>();
		UpdateColor();
	}
	private void FixedUpdate ()
	{
		UpdateBallPosition();
		UpdateModifiers();
        if (!isGhostBall)
        {
            CheckIfOutOfScreen();
        }
	}

	#region Public functions
	public void CurveShoot ( PassController _passController, PawnController _thrower, PawnController _target, BallDatas _passDatas, Vector3 _lookDirection ) //Shoot a curve ball to reach a point
	{
		if (ballCoroutine != null) { StopCoroutine(ballCoroutine); }
		startPosition = _passController.GetHandTransform().position;
		transform.SetParent(null, true);
		transform.localScale = Vector3.one;
		ballInformations.thrower = _thrower;
		ballInformations.moveSpeed = _passDatas.moveSpeed;
		ballInformations.maxDistance = Mathf.Infinity;
		ballInformations.ballDatas = _passDatas;
		ballInformations.bounceCount = 0;
		ballInformations.canBounce = true;
		ballInformations.canHitWalls = true;
		ballInformations.curve = _passController.GetCurvedPathCoordinates(startPosition, _target, _lookDirection, out float d);
		ballInformations.timeFlying = 0;
		ballInformations.initialLookDirection = _lookDirection;
		ballInformations.isTeleguided = false;
		hitGameObjects.Clear();
		ChangeState(BallState.Flying);
		UpdateColor();
	}
	public void Shoot ( Vector3 _startPosition, Vector3 _direction, PawnController _thrower, BallDatas _passDatas, bool _teleguided ) //Shoot the ball toward a direction
	{
		if (ballCoroutine != null) { StopCoroutine(ballCoroutine); }
		transform.SetParent(null, true);
		transform.localScale = Vector3.one;
		transform.position = _startPosition;
		ballInformations.direction = _direction;
		ballInformations.thrower = _thrower;
		ballInformations.moveSpeed = _passDatas.moveSpeed;
		ballInformations.ballDatas = _passDatas;
		ballInformations.bounceCount = 0;
		ballInformations.timeFlying = 0;
		ballInformations.curve = null;
		ballInformations.canBounce = true;
		ballInformations.canHitWalls = true;
		ballInformations.isTeleguided = _teleguided;
		hitGameObjects.Clear();
		ChangeState(BallState.Flying);
		UpdateColor();
	}
	public void Bounce ( Vector3 _newDirection, float _bounceSpeedMultiplier )
	{
		CursorManager.SetBallPointerParent(transform);
		ballInformations.curve = null;
		ballInformations.distanceTravelled = 0;
		ballInformations.bounceCount++;
		ballInformations.direction = _newDirection;
		ballInformations.direction.y = 0;
		ballInformations.moveSpeed = ballInformations.moveSpeed * _bounceSpeedMultiplier;
		ballInformations.isTeleguided = false;
		hitGameObjects.Clear();
	}
	public void ChangeDirection ( Vector3 _newDirection )
	{
		ballInformations.direction = _newDirection;
	}
	public void GoToHands ( Transform _handTransform, float _travelDuration, BallDatas _passData )
	{
		ballInformations.ballDatas = _passData;
		ballInformations.curve = null;
		ChangeState(BallState.Held);
		ballCoroutine = StartCoroutine(GoToPosition(_handTransform, _travelDuration));
		transform.SetParent(_handTransform, true);
	}
	public void CancelMovement ()
	{
		ballInformations.moveSpeed = 0;
		ChangeState(BallState.Grounded);
	}
	public void ResetBounceCount ()
	{
		ballInformations.bounceCount = 0;
	}
	public void MultiplySpeed ( float _coef )
	{
		ballInformations.moveSpeed *= _coef;
	}
	public void ChangeMaxDistance ( int _newMaxDistance ) //Changing the distance will cause the ball to stop after travelling a certain distance
	{
		ballInformations.maxDistance = _newMaxDistance;
	}
	public void ChangeSpeed ( float _newSpeed )
	{
		ballInformations.moveSpeed = _newSpeed;
	}
	public float GetCurrentSpeed ()
	{
		return ballInformations.moveSpeed;
	}
	public float GetCurrentDistanceTravelled ()
	{
		return ballInformations.distanceTravelled;
	}
	public Vector3 GetCurrentDirection ()
	{
		return ballInformations.direction;
	}
	public BallDatas GetCurrentBallDatas ()
	{
		return ballInformations.ballDatas;
	}
	public PawnController GetCurrentThrower ()
	{
		return ballInformations.thrower;
	}
	public int GetCurrentBounceCount ()
	{
		return ballInformations.bounceCount;
	}
	public float GetTimeFlying ()
	{
		return ballInformations.timeFlying;
	}
	public int GetCurrentDamages ()
    {
        if (isGhostBall)
        {
            return (0);
        }
        float i_damages = ballInformations.ballDatas.damages;
		return Mathf.RoundToInt(i_damages * GetCurrentDamageModifier());
	}
	public float GetCurrentDamageModifier ()
	{
		float i_perfectReceptionModifier = 1f;
		float i_otherModifier = 1;
		foreach (DamageModifier modifier in ballInformations.damageModifiers)
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
		if (ballInformations != null && ballInformations.ballDatas != null)
		{
			i_perfectReceptionModifier = Mathf.Clamp(i_perfectReceptionModifier, 0, ballInformations.ballDatas.maxDamageModifierOnPerfectReception);
		}
		return (i_perfectReceptionModifier * i_otherModifier);
	}
	public float GetPerfectReceptionDamageModifier ()
	{
		float i_perfectReceptionModifier = 1f;
		foreach (DamageModifier modifier in ballInformations.damageModifiers)
		{
			if (modifier.source == DamageModifierSource.PerfectReception)
			{
				i_perfectReceptionModifier *= modifier.multiplyCoef;
			}
		}
		i_perfectReceptionModifier = Mathf.Clamp(i_perfectReceptionModifier, 0, ballInformations.ballDatas.maxDamageModifierOnPerfectReception);
		return i_perfectReceptionModifier;
	}
	public float GetCurrentSpeedModifier ()
	{
		float i_otherModifier = 1f;
		float i_perfectReceptionModifier = 1f;
		foreach (SpeedCoef modifier in ballInformations.speedModifiers)
		{
			if (modifier.reason == SpeedMultiplierReason.PerfectReception)
			{
				i_perfectReceptionModifier *= modifier.speedCoef;
			}
			else
			{
				i_otherModifier *= modifier.speedCoef;
			}
		}
		i_perfectReceptionModifier = Mathf.Clamp(i_perfectReceptionModifier, 0, ballInformations.ballDatas.maxSpeedMultiplierOnPerfectReception);
		return i_perfectReceptionModifier * i_otherModifier;
	}
	public SpeedCoef AddNewSpeedModifier ( SpeedCoef _newModifier )
	{
		ballInformations.speedModifiers.Add(_newModifier);
		return _newModifier;
	}
	public void RemoveSpeedModifier ( SpeedMultiplierReason _source )
	{
		List<SpeedCoef> i_newModifierList = new List<SpeedCoef>();
		foreach (SpeedCoef modifier in ballInformations.speedModifiers)
		{
			if (modifier.reason != _source)
			{
				i_newModifierList.Add(modifier);
			}
		}
		ballInformations.speedModifiers = i_newModifierList;
	}
	public DamageModifier AddNewDamageModifier ( DamageModifier _newModifier )
	{
		ballInformations.damageModifiers.Add(_newModifier);
		UpdateColor();
		return _newModifier;
	}
	public void RemoveDamageModifier ( DamageModifierSource _source )
	{
		List<DamageModifier> i_newModifierList = new List<DamageModifier>();
		foreach (DamageModifier modifier in ballInformations.damageModifiers)
		{
			if (modifier.source != _source)
			{
				i_newModifierList.Add(modifier);
			}
		}
		ballInformations.damageModifiers = i_newModifierList;
		UpdateColor();
	}
	public void ChangeState ( BallState _newState )
	{
		if (_newState == ballInformations.state) { return; }
		switch (_newState)
		{
			case BallState.Grounded:
				Analytics.CustomEvent("BallGrounded", new Dictionary<string, object> { { "Zone", GameManager.GetCurrentZoneName() }, });
				if (destroyTrailFX != null) { StopCoroutine(destroyTrailFX); Destroy(ballTrail); }
				if (ballTrail) { destroyTrailFX = StartCoroutine(DisableEmitterThenDestroyAfterDelay(ballTrail.GetComponent<ParticleSystem>(), 0.5f)); }
				FeedbackManager.SendFeedback("event.BallGrounded", this);
				EnableGravity();
				EnableCollisions();
				rb.AddForce(ballInformations.direction.normalized * ballInformations.moveSpeed * rb.mass, ForceMode.Impulse);
				CursorManager.SetBallPointerParent(transform);
				if (ballInformations.thrower != null)
				{
					if (ballInformations.thrower.isPlayer) { LockManager.UnlockAll(); }
				}
				break;
			case BallState.Aimed:
				DisableGravity();
				DisableCollisions();
				break;
			case BallState.Flying:
				Highlighter.DetachBallFromPlayer();
				if (ballInformations.thrower.isPlayer) { CursorManager.SetBallPointerParent(null); }
				ballTrail = FeedbackManager.SendFeedback("event.BallFlying", this).GetVFX();
				Vector3 newBallScale = ballTrail.transform.localScale;
				newBallScale *= Mathf.Lerp(1f, ballInformations.ballDatas.maxFXSizeMultiplierOnPerfectReception, (GetPerfectReceptionDamageModifier() / ballInformations.ballDatas.maxDamageModifierOnPerfectReception));
				ballTrail.transform.localScale = newBallScale;
				DisableGravity();
				EnableCollisions();
				col.isTrigger = true;
				col.enabled = true;
				ballInformations.distanceTravelled = 0;
				break;
			case BallState.Held:
				if (destroyTrailFX != null) { StopCoroutine(destroyTrailFX); Destroy(ballTrail); }
				if (ballTrail) { destroyTrailFX = StartCoroutine(DisableEmitterThenDestroyAfterDelay(ballTrail.GetComponent<ParticleSystem>(), 0.5f)); }
				DisableGravity();
				DisableCollisions();
				if (ballInformations.thrower != null && ballInformations.thrower.isPlayer) { LockManager.UnlockAll(); }
				break;
		}
		ballInformations.state = _newState;
	}
	public BallState GetState ()
	{
		return ballInformations.state;
	}
	public bool HasTarget()
	{
		if (ballInformations.curve != null || ballInformations.isTeleguided)
		{
			return true;
		} else
		{
			return false;
		}
	}
	#endregion

	#region Private functions
	private void InitBallInformation ()
	{
		ballInformations = new BallInformations();
		ballInformations.speedModifiers = new List<SpeedCoef>();
		ballInformations.damageModifiers = new List<DamageModifier>();
		ballInformations.color = new Color(122f / 255f, 0, 122f / 255f);
	}
	private void CheckIfOutOfScreen ()
	{
		Vector3 viewportPos = GameManager.mainCamera.WorldToViewportPoint(transform.position);
		if ((viewportPos.x > 1 || viewportPos.x < 0 || viewportPos.y > 1 || viewportPos.y < 0) && transform.parent == null)
		{
			outOfScreenTime += Time.deltaTime;
		}
		else
		{
			outOfScreenTime = 0;
		}
		if (ballInformations.ballDatas != null && outOfScreenTime > ballInformations.ballDatas.maxTimeOutOfScreen && ballCoroutine == null)
		{
			ballCoroutine = StartCoroutine(GoToNearestPlayer_C());
		}
	}
	private void UpdateModifiers ()
	{
		List<DamageModifier> i_newDamageModifierList = new List<DamageModifier>();
		foreach (DamageModifier damageModifier in ballInformations.damageModifiers)
		{
			if (damageModifier.duration <= -1) { i_newDamageModifierList.Add(damageModifier); continue; }
			damageModifier.duration -= Time.deltaTime;
			if (damageModifier.duration > 0)
			{
				i_newDamageModifierList.Add(damageModifier);
			}
		}
		ballInformations.damageModifiers = i_newDamageModifierList;

		List<SpeedCoef> i_newSpeedModifierList = new List<SpeedCoef>();
		foreach (SpeedCoef speedModifier in ballInformations.speedModifiers)
		{
			if (speedModifier.duration <= -1) { i_newSpeedModifierList.Add(speedModifier); continue; }
			speedModifier.duration -= Time.deltaTime;
			if (speedModifier.duration > 0)
			{
				i_newSpeedModifierList.Add(speedModifier);
			}
		}
		ballInformations.speedModifiers = i_newSpeedModifierList;
	}
	private void UpdateColor ()
	{
		if (ballInformations.ballDatas != null)
		{
			float i_lerpValue = (GetCurrentDamageModifier() - 1) / (ballInformations.ballDatas.maxDamageModifierOnPerfectReception - 1);
			Color i_newColor = ballInformations.ballDatas.colorOverDamage.Evaluate(i_lerpValue);
			SetColor(i_newColor);
		}
	}
	private void SetColor ( Color _newColor )
	{
		ParticleColorer.ReplaceParticleColor(gameObject, ballInformations.color, _newColor);
		ballInformations.color = _newColor;
	}
	private void UpdateBallPosition ()
	{
		switch (ballInformations.state)
		{
			case BallState.Flying:
				ballInformations.timeFlying += Time.deltaTime;
				if (ballInformations.isTeleguided)
				{
					PassController i_currentPassController = GetCurrentThrower().GetComponent<PassController>();
					if (i_currentPassController != null)
					{
						ballInformations.direction = (i_currentPassController.GetTarget().GetCenterPosition() - transform.position).normalized;
					}
				}
				else if (ballInformations.curve != null)
				{
					PassController i_currentPassController = GetCurrentThrower().GetComponent<PassController>();
					List<Vector3> i_pathCoordinates = i_currentPassController.GetCurvedPathCoordinates(startPosition, i_currentPassController.GetTarget(), ballInformations.initialLookDirection, out float d);
					float i_curveLength;
					if (i_currentPassController == null) { return; }
					ConvertCoordinatesToCurve(i_pathCoordinates, out curveX, out curveY, out curveZ, out i_curveLength);
					ballInformations.maxDistance = i_curveLength;
					float i_positionOnCurve = ballInformations.distanceTravelled / ballInformations.maxDistance;
					if (ballInformations.thrower.isPlayer)
					{
						LockManager.LockTargetsInPath(i_pathCoordinates, i_positionOnCurve);
						if (i_positionOnCurve >= 0.95f) { ChangeState(BallState.Grounded); LockManager.UnlockAll(); }
					}
					Vector3 i_nextPosition = new Vector3(curveX.Evaluate(i_positionOnCurve + 0.1f), curveY.Evaluate(i_positionOnCurve + 0.1f), curveZ.Evaluate(i_positionOnCurve + 0.1f));
					ballInformations.direction = i_nextPosition - transform.position;
				}

				if (ballInformations.moveSpeed <= 0)
				{
					ballInformations.curve = null;
					ChangeState(BallState.Grounded);
				}
				else
				{
					//Ball is going to it's destination, checking for collisions
					RaycastHit[] i_hitColliders = Physics.RaycastAll(transform.position, ballInformations.direction, ballInformations.moveSpeed * Time.deltaTime);
					foreach (RaycastHit raycast in i_hitColliders)
					{
						EnemyShield i_selfRef = raycast.collider.GetComponentInParent<EnemyShield>();
						if (i_selfRef != null)
						{
							if (i_selfRef.shield.transform.InverseTransformPoint(transform.position).z > 0.0)
							{
								FeedbackManager.SendFeedback("event.ShieldHitByBall", this);
								Vector3 i_newDirection = Vector3.Reflect(ballInformations.direction, i_selfRef.shield.transform.forward);
								Bounce(i_newDirection, 1);
							}
						}

						IHitable i_potentialHitableObjectFound = raycast.collider.GetComponent<IHitable>();
						if (i_potentialHitableObjectFound != null && !hitGameObjects.Contains(i_potentialHitableObjectFound) && !isGhostBall)
						{
							hitGameObjects.Add(i_potentialHitableObjectFound);
							i_potentialHitableObjectFound.OnHit(this, ballInformations.direction * ballInformations.moveSpeed, ballInformations.thrower, GetCurrentDamages(), DamageSource.Ball);
							SlowTimeScale();
						}

						if (raycast.collider.isTrigger || raycast.collider.gameObject.layer != LayerMask.NameToLayer("Environment")) { break; }
						FeedbackManager.SendFeedback("event.WallHitByBall", raycast.transform, raycast.point, ballInformations.direction, raycast.normal);
						if (!ballInformations.canHitWalls) { return; }
						if (ballInformations.bounceCount < ballInformations.ballDatas.maxBounces && ballInformations.canBounce) //Ball can bounce: Bounce
						{
							Analytics.CustomEvent("BallBounce", new Dictionary<string, object> { { "Zone", GameManager.GetCurrentZoneName() }, });
							Vector3 i_hitNormal = raycast.normal;
							i_hitNormal.y = 0;
							Vector3 i_newDirection = Vector3.Reflect(ballInformations.direction, i_hitNormal);
							i_newDirection.y = -ballInformations.direction.y;
							Bounce(i_newDirection, ballInformations.ballDatas.speedMultiplierOnBounce);
							return;
						}
						else //Ball can't bounce: Stop
						{
							ChangeState(BallState.Grounded);
							MomentumManager.DecreaseMomentum(MomentumManager.datas.momentumLossWhenBallHitTheGround);
							return;
						}
					}
				}
				transform.position += ballInformations.direction.normalized * ballInformations.moveSpeed * Time.deltaTime * MomentumManager.GetValue(MomentumManager.datas.ballSpeedMultiplier) * GetCurrentSpeedModifier();
				ballInformations.distanceTravelled += ballInformations.moveSpeed * Time.deltaTime * MomentumManager.GetValue(MomentumManager.datas.ballSpeedMultiplier) * GetCurrentSpeedModifier();
				if (ballInformations.curve == null && !ballInformations.isTeleguided && ballInformations.distanceTravelled >= ballInformations.maxDistance)
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
	private void SlowTimeScale ()
	{
		StartCoroutine(SlowTimeScale_C());
	}
	private void ConvertCoordinatesToCurve ( List<Vector3> _coordinates, out AnimationCurve _curveX, out AnimationCurve _curveY, out AnimationCurve _curveZ, out float _curveLength )
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

	#endregion

	#region Coroutines
	IEnumerator GoToPosition ( Transform _transform, float _travelDuration )
	{
		for (float i = 0; i < _travelDuration; i += Time.deltaTime)
		{
			transform.position = Vector3.Lerp(transform.position, _transform.position, i / _travelDuration);
			yield return null;
		}
		transform.position = _transform.position;
		transform.localScale = Vector3.one;
		ballCoroutine = null;
	}
	IEnumerator DisableEmitterThenDestroyAfterDelay ( ParticleSystem _ps, float _delay )
	{
		var emission = _ps.emission;
		emission.enabled = false;
		yield return new WaitForSeconds(_delay);
		if (_ps != null)
		{
			Destroy(_ps.gameObject);
		}
	}
	IEnumerator GoToNearestPlayer_C ()
	{
		ChangeState(BallState.Aimed);
		float minDist = Vector3.Distance(transform.position, GameManager.alivePlayers[0].transform.position);
		PlayerController nearestPlayer = GameManager.alivePlayers[0];
		foreach (PlayerController p in GameManager.alivePlayers)
		{
			float dist = Vector3.Distance(transform.position, p.transform.position);
			if (dist < minDist)
			{
				minDist = dist;
				nearestPlayer = p;
			}
		}
		Vector3 startPosition = transform.position;
		for (float i = 0; i < minDist; i += Time.deltaTime * ballInformations.ballDatas.comingBackToScreenSpeed)
		{
			transform.position = Vector3.Lerp(startPosition, nearestPlayer.transform.position, i / minDist);
			yield return null;
		}
		ballCoroutine = null;
		nearestPlayer.passController.Receive(this);
	}
	IEnumerator SlowTimeScale_C ()
	{
		Time.timeScale = Mathf.Min(ballInformations.ballDatas.timescaleOnHit, PlayerPrefs.GetFloat("REU_GameSpeed", GameManager.i.gameSpeed)/100);
		yield return new WaitForSeconds(ballInformations.ballDatas.timescaleDurationOnHit);
		Time.timeScale = PlayerPrefs.GetFloat("REU_GameSpeed", GameManager.i.gameSpeed)/100;
    }
	#endregion
}
