using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using MyBox;
using UnityEngine.AI;
using System.Reflection;

public enum MoveState
{
    Idle,
    Walk,
    Blocked,
    Bumped,
	Jumping,
	Climbing,
	Dead,
	Pushed
}

public enum SpeedMultiplierReason
{
	Link,
	Freeze,
	Reviving,
	Dash,
	PerfectReception,
	Pass,
	Environment,
	Dunk,
	ChangingFocus,
	Unknown
}

public enum PushType
{
	Heavy,
	Light
}

public class SpeedCoef
{
	public SpeedCoef ( float _speedCoef, float _duration, SpeedMultiplierReason _reason, bool _stackable )
	{
		speedCoef = _speedCoef;
		duration = _duration;
		reason = _reason;
		stackable = _stackable;
	}
	public float speedCoef;
	public float duration;
	public SpeedMultiplierReason reason;
	public bool stackable;
}

public class PawnController : MonoBehaviour
{
	[Separator("General settings")]
	public float maxHealth;
	public float totalHeight;
    public float invincibilityTime = 1;
    public bool ignoreEletricPlates = false;

    [Space(2)]
    [Separator("Movement settings")]
    public bool canMove;
    public PawnMovementValues pawnMovementValues;
    protected float effectiveSpeed;

    [Separator("Climb settings")]
    public bool canClimb = true;
    [ConditionalField(nameof(canClimb))] public float timeBeforeClimb = 0.2f;
    [ConditionalField(nameof(canClimb))] public float minDistanceToClimb = 1f;
    [ConditionalField(nameof(canClimb))] public float climbDuration = 0.5f;
    [ConditionalField(nameof(canClimb))] public float climbForwardPushForce = 450f;
    [ConditionalField(nameof(canClimb))] public float climbUpwardPushForce = 450f;

	[Space(2)]
    [Separator("Bumped Values")]
	public bool isBumpable = true;
    [ConditionalField(nameof(isBumpable))] public float maxGettingUpDuration = 0.6f;
    [ConditionalField(nameof(isBumpable))] [Range(0, 1)] public float whenToTriggerFallingAnim = 0.302f;

	[Space(2)]
	[Separator("Events")]
	public string eventOnBeingHit = "event.PlayerBeingHit";
	public string eventOnBeingBumpedAway = "event.PlayerBeingBumpedAway";
	public string eventOnBeingGrounded = "event.PlayerGrounded";
	public string eventOnDeath = "event.Player1Death";
	public string eventOnHealing = "event.PlayerHealing";

	//Movement variables
	[System.NonSerialized] public MoveState moveState;
	protected Vector3 moveInput;
	protected Vector3 lookInput;
    private Quaternion turnRotation;
	private float customDrag;
	private float customGravity;
	protected float currentSpeed;
	private List<SpeedCoef> speedCoefs = new List<SpeedCoef>();
	private bool grounded = false;
	private float timeInAir;
	private bool rotationForced;
	protected bool frozen;
	private PushDatas pushDatas;
	protected float damageAfterBump;

	//Health variables
	protected float currentHealth;
	private bool isInvincible;
	private bool isInvincibleWithCheat;
	private float invincibilityCooldown;

	//Other variables
	[HideInInspector] public float climbingDelay;
	private bool isPlayer;
	protected bool targetable;
	protected NavMeshAgent navMeshAgent;

	//State system variables
	protected PawnState currentPawnState = null;
	private PawnStates pawnStates;
	private Coroutine currentStateStartCoroutine;
	private StoppableCoroutine currentStateCoroutine;
	private IEnumerator currentStateStopCoroutine;

	//References
    [HideInInspector] public PassController passController;
	[HideInInspector] public Rigidbody rb;
	[HideInInspector] public Animator animator;

	public virtual void Awake()
    {
		//Retrieve references
		rb = GetComponent<Rigidbody>();
		animator = GetComponentInChildren<Animator>();
		passController = GetComponent<PassController>();
		navMeshAgent = GetComponent<NavMeshAgent>();
		pawnStates = Resources.Load<PawnStates>("PawnStateDatas");
		if (navMeshAgent == null) { navMeshAgent = GetComponentInChildren<NavMeshAgent>(); }
		if (navMeshAgent == null) { navMeshAgent = GetComponent<NavMeshAgent>(); }
		pushDatas = PushDatas.GetDatas();

		//Init variables
		isInvincible = false;
		customGravity = pawnMovementValues.onGroundGravityMultiplier * -9.81f;
        customDrag = pawnMovementValues.idleDrag;
		currentHealth = maxHealth;
		targetable = true;
		if (GetComponent<PlayerController>() != null) { isPlayer = true; }
		UpdateNavMeshAgent(navMeshAgent);
		moveState = MoveState.Idle;
		currentPawnState = null;
        effectiveSpeed = pawnMovementValues.moveSpeed;

    }
	protected virtual void FixedUpdate()
    {
		if (frozen) { return; }
        CheckMoveState();
		UpdateAcceleration();
        Move();
        ApplyDrag();
        ApplyCustomGravity();
        UpdateAnimatorBlendTree();
		UpdateSpeedCoef();
		CheckIfGrounded();
		UpdateNavMeshAgent(navMeshAgent);
        UpdateInvincibilityCooldown();
	}
	protected virtual void LateUpdate ()
	{
		if (frozen) { return; }
		Rotate();
	}

	#region Public functions
	public void ForceLookAt ( Vector3 _point ) //Forces the pawn to look at a specific point, must be called at each frame
	{
		Vector3 i_direction = _point - GetCenterPosition();
		turnRotation = Quaternion.Euler(0, Mathf.Atan2(i_direction.x, i_direction.z) * 180 / Mathf.PI, 0);
		rotationForced = true;
	}
	public void ForceRotate () //Forces the pawn to rotate toward looked direction, must be called at each frame
	{
		if (lookInput.magnitude >= 0.1f)
		{
			turnRotation = Quaternion.Euler(0, Mathf.Atan2(lookInput.x, lookInput.z) * 180 / Mathf.PI, 0);
			rotationForced = true;
		}
	}
	public void SetLookInput ( Vector3 _direction ) //Set the pawn looked direction
	{
		lookInput = _direction;
	} 
	public NavMeshAgent GetNavMesh ()
	{
		return navMeshAgent;
	}
	public bool IsTargetable ()
	{
		return targetable;
	}
	public void ChangePawnState(string _newStateName, IEnumerator _coroutineToStart, IEnumerator _coroutineToCancel = null) //Check "Pawn state: manual" on google drive for more informations
	{
		PawnState i_newState = pawnStates.GetPawnStateByName(_newStateName);
		bool canOverrideState;
		if (currentPawnState != null)
		{
			if (pawnStates.IsStateOverriden(currentPawnState, i_newState) || currentStateCoroutine == null)
			{
				//Must cancel current state and replace by new state
				StopCurrentState();
				canOverrideState = true;
			} else
			{
				//Can't override current state, cancel action
				return;
			}
		} else
		{
			canOverrideState = true;
		}
		if (canOverrideState)
		{
			if (_coroutineToCancel != null)
			{
				currentStateStopCoroutine = _coroutineToCancel;
			}
			currentStateStartCoroutine = StartCoroutine(StartStateCoroutine(_coroutineToStart, i_newState));
			PawnState i_newStateInstance = new PawnState();
			i_newStateInstance.allowBallReception = i_newState.allowBallReception;
			i_newStateInstance.allowBallThrow = i_newState.allowBallThrow;
			currentPawnState = i_newState;
		}
	}
	public bool IsInvincible()
	{
		return isInvincible;
	}
	public void SetInvincible ( bool _state )
	{
		isInvincible = _state;
		isInvincibleWithCheat = _state;
	}
	public PawnState GetCurrentPawnState()
	{
		return currentPawnState;
	}
	public float GetHealth()
	{
		return currentHealth;
	}
	public float GetMaxHealth()
	{
		return maxHealth;
	}
    public float GetSpeedCoef()
    {
        float i_speedCoef = 1;
        foreach (SpeedCoef i_coef in speedCoefs)
        {
            i_speedCoef *= i_coef.speedCoef;
        }
        if (isPlayer)
        {
            i_speedCoef *= MomentumManager.GetValue(MomentumManager.datas.playerSpeedMultiplier);
        }
        else
        {
            i_speedCoef *= MomentumManager.GetValue(MomentumManager.datas.enemySpeedMultiplier);
        }
        return i_speedCoef;
    }
    public void AddSpeedModifier(SpeedCoef _speedCoef )
	{
		if (!_speedCoef.stackable)
		{
			foreach (SpeedCoef i_coef in speedCoefs)
			{
				if (i_coef.reason == _speedCoef.reason)
				{
					return;
				}
			}
		}
		speedCoefs.Add(_speedCoef);
	}
    public virtual void Kill()
    {
		LockManager.UnlockTarget(transform);
		FeedbackManager.SendFeedback(eventOnDeath, this);
		Destroy(this.gameObject);
    }
	public virtual void Heal(int _amount)
	{
		float i_newHealth = currentHealth + _amount;
		currentHealth = Mathf.Clamp(i_newHealth, 0, GetMaxHealth());
		FeedbackManager.SendFeedback(eventOnHealing, this);
	}
	public bool CanDamage()
	{
		if (isInvincible) { return false; }
		if (currentPawnState != null && currentPawnState.invincibleDuringState) { return false; }
		return true;
	}
	public virtual void Damage(float _amount, bool enableInvincibilityFrame = false)
	{
		if (!CanDamage()){ return; }
		if (enableInvincibilityFrame) { SetInvincible(); }
		FeedbackManager.SendFeedback(eventOnBeingHit, this, transform.position, transform.up, transform.up);

        currentHealth -= _amount;

        if (currentHealth <= 0)
        {
            Kill();
        }
		if (GetComponent<PlayerController>() != null)
        {
            MomentumManager.DecreaseMomentum(MomentumManager.datas.momentumLossOnDamage);
        }
	}
	public Animator GetAnimator ()
	{
		return animator;
	}
	public void Freeze()
	{
		rb.isKinematic = true;
		rb.useGravity = false;
		frozen = true;
	}
	public void UnFreeze()
	{
		rb.isKinematic = false;
		rb.useGravity = true;
		frozen = false;
	}
	public void Hide ()
	{
		foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
		{
			renderer.enabled = false;
		}
	}
	public void UnHide()
	{
		foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
		{
			renderer.enabled = true;
		}
	}
	public void SetUntargetable ()
	{
		foreach (Collider i_collider in GetComponentsInChildren<Collider>())
		{
			i_collider.enabled = false;
		}
		targetable = false;
	}
	public void SetTargetable ()
	{
		foreach (Collider i_collider in GetComponentsInChildren<Collider>())
		{
			i_collider.enabled = true;
		}
		targetable = true;
	}
	public Vector3 GetCenterPosition ()
	{
		return transform.position + Vector3.up * (totalHeight / 2f);
	}
	public Vector3 GetHeadPosition ()
	{
		return transform.position + Vector3.up * totalHeight;
	}
	public float GetHeight ()
	{
		return totalHeight;
	}
	public virtual void BumpMe( Vector3 _bumpDirectionFlat, BumpForce _force)
    {
		if (!isBumpable) { return; }
		ChangePawnState("Bumped", Bump_C(_bumpDirectionFlat, _force), CancelBump_C());
    }
	public void PushLightCustom( Vector3 _pushDirectionFlat, float _pushDistance, float _pushDuration, float _pushHeight )
	{
        if (!isBumpable) { return; }
		ChangePawnState("PushedLight", CustomLightPush_C(_pushDirectionFlat, _pushDistance, _pushDuration, _pushHeight), CancelPush_C());
	}
	public virtual void Push ( PushType _forceType, Vector3 _pushDirectionFlat, PushForce _force)
	{
		if (!isBumpable) { return; }
		RaycastHit i_hit;
		if (Physics.Raycast(GetCenterPosition(), _pushDirectionFlat, out i_hit, 1f, LayerMask.GetMask("Environment")))
		{
			return;
		}
		switch (_forceType)
		{
			case PushType.Light:
				PushLight(_pushDirectionFlat, _force);
				break;
			case PushType.Heavy:
				Vector3 forwardDirection = _pushDirectionFlat.normalized;
				transform.forward = forwardDirection;
				ChangePawnState("PushedHeavy", PushHeavy_C(_pushDirectionFlat, _force), CancelPush_C());
				break;
		}
	}
	#endregion

	#region Private OR protected OR herited functions
	private void CheckMoveState ()
	{
		if (moveState == MoveState.Blocked || moveState == MoveState.Pushed) { return; }

		if (rb.velocity.magnitude <= pawnMovementValues.minWalkSpeed)
		{
			if (moveState != MoveState.Idle)
			{
				rb.velocity = new Vector3(0, rb.velocity.y, 0);
			}
			customDrag = pawnMovementValues.idleDrag;
			moveState = MoveState.Idle;
		}
	}
	private void Rotate ()
	{
		if (moveState == MoveState.Blocked || moveState == MoveState.Pushed) { return; }
		//Rotation while moving
		if (moveInput.magnitude >= 0.5f && !rotationForced)
			turnRotation = Quaternion.Euler(0, Mathf.Atan2(moveInput.x, moveInput.z) * 180 / Mathf.PI, 0);

		//Rotation while aiming or shooting
		if (passController != null)
		{
			if (lookInput.magnitude >= 0.1f && passController.passState == PassState.Aiming || passController.passState == PassState.Shooting)
				turnRotation = Quaternion.Euler(0, Mathf.Atan2(lookInput.x, lookInput.z) * 180 / Mathf.PI, 0);
		}

		if (rotationForced)
		{
			transform.rotation = turnRotation;
		}
		else
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, turnRotation, pawnMovementValues.turnSpeed);
		}
		rotationForced = false;
	}
	private void UpdateAcceleration ()
	{
		if (moveInput.magnitude != 0)
		{
			if (moveState == MoveState.Blocked) { return; }
			rb.AddForce(moveInput * (pawnMovementValues.accelerationCurve.Evaluate(rb.velocity.magnitude / pawnMovementValues.moveSpeed) * pawnMovementValues.acceleration * GetSpeedCoef()), ForceMode.Acceleration);
			customDrag = pawnMovementValues.movingDrag;
		}
	}
	private void UpdateNavMeshAgent ( NavMeshAgent _agent )
	{
		if (_agent == null) { return; }
		_agent.speed = currentSpeed;
		_agent.angularSpeed = pawnMovementValues.turnSpeed;
	}
	private void Move ()
	{
		if (moveState == MoveState.Blocked) { currentSpeed = 0; return; }
		if (moveState == MoveState.Pushed) { return; }
		if (navMeshAgent != null)
		{
			currentSpeed = effectiveSpeed * GetSpeedCoef();
		}
		else
		{
			Vector3 i_myVel = rb.velocity;
			i_myVel.y = 0;
			i_myVel = Vector3.ClampMagnitude(i_myVel, effectiveSpeed * GetSpeedCoef());
			i_myVel.y = rb.velocity.y;
			rb.velocity = i_myVel;
			currentSpeed = rb.velocity.magnitude;
		}
	}
	protected void Climb ()
	{
		Collider i_foundLedge = CheckForLedge();
		if (i_foundLedge != null)
		{
			climbingDelay += Time.deltaTime;
		}
		else
		{
			climbingDelay = 0;
		}
		if (climbingDelay >= timeBeforeClimb && i_foundLedge != null)
		{
			moveState = MoveState.Climbing;
			if (animator != null) { animator.SetTrigger("ClimbTrigger"); }
			ChangePawnState("Climbing", ClimbLedge_C(i_foundLedge));
		}
	}
	private bool CanClimb ()
	{
		if (moveState == MoveState.Blocked || moveState == MoveState.Climbing)
		{
			return false;
		}
		return true;
	}
	private void CheckIfGrounded ()
	{
		if (!grounded)
		{
			timeInAir += Time.deltaTime;
			if (timeInAir >= 0.2f)
			{
				if (Physics.Raycast(transform.position, Vector3.down, 0.1f, LayerMask.GetMask("Environment")))
				{
					FeedbackManager.SendFeedback(eventOnBeingGrounded, this);
					grounded = true;
					timeInAir = 0;
					if (moveState == MoveState.Jumping)
					{
						moveState = MoveState.Idle;
					}
				}
			}
		}
	}
	private void ApplyDrag ()
	{
		if (navMeshAgent != null) { return; }
		Vector3 i_myVel = rb.velocity;
		i_myVel.x *= 1 - customDrag;
		i_myVel.z *= 1 - customDrag;
		rb.velocity = i_myVel;
	}
	private void ApplyCustomGravity ()
	{
		rb.AddForce(new Vector3(0, customGravity, 0));
	}
	private void UpdateSpeedCoef ()
	{
		List<SpeedCoef> i_newCoefList = new List<SpeedCoef>();
		foreach (SpeedCoef coef in speedCoefs)
		{
			coef.duration -= Time.deltaTime;
			if (coef.duration > 0)
			{
				i_newCoefList.Add(coef);
			}
		}
		speedCoefs = i_newCoefList;
	}
	public virtual void UpdateAnimatorBlendTree ()
	{
		if (animator == null) { return; }
	}
	public virtual void HeavyPushAction () //Will be removed ASAP
	{
		// Filled in each of the children behaviour;
	}
	public virtual void DestroySpawnedAttackUtilities () //Will be removed ASAP
	{
		// Usually filled within the inherited scripts
	}
	private Collider CheckForLedge()
	{
		if (!CanClimb()) { return null; }
		RaycastHit hit;
		if (Physics.SphereCast(GetCenterPosition() - transform.forward * 2f, 0.5f, transform.forward, out hit, minDistanceToClimb + 2f, LayerMask.GetMask("Environment")))
		{
			if (hit.transform.tag == "Ledge")
			{
				return hit.collider;
			}
		}
		return null;
	}
	private void UpdateInvincibilityCooldown()
	{
		if (isInvincibleWithCheat)
		{
			isInvincible = true; return;
		}
		if (invincibilityCooldown > 0)
		{
			invincibilityCooldown -= Time.deltaTime;
		}
        else
		{
			isInvincible = false;
			gameObject.layer = 8; // 8 = Player Layer
		}
	}
	private void SetInvincible()
    {
		isInvincible = true;
        gameObject.layer = 0; // 0 = Default, which matrix doesn't interact with ennemies
		invincibilityCooldown = invincibilityTime;
    }
	private Collider CheckForCollision()
	{
		Collider i_closestCollider = null;
		float closestDistance = 50;
		foreach (Collider c in Physics.OverlapSphere(GetCenterPosition(), GetHeight() / 3f, LayerMask.GetMask("Environment")))
		{
			Vector3 i_collisionPoint = c.ClosestPoint(GetCenterPosition());
			float distance = Vector3.Distance(i_collisionPoint, GetCenterPosition());
			if (distance < closestDistance)
			{
				i_closestCollider = c;
				closestDistance = distance;
			}
		}
		return i_closestCollider;
	}
	private void StopCurrentState ()
	{
		if (currentPawnState == null) { return; }
		if (currentPawnState.invincibleDuringState)
		{
			SetInvincible(false);
		}
		if (currentStateCoroutine != null)
		{
			currentStateCoroutine.Stop();
			if (currentStateStartCoroutine != null)
			{
				StopCoroutine(currentStateStartCoroutine);
			}
		}

		if (currentStateCoroutine != null)
		{
			currentStateStopCoroutine.StartCoroutine();
		}
		currentPawnState = null;
	}
	private void WallSplat ( WallSplatForce _force, Vector3 _normalDirection)
	{
		if (currentPawnState != null && currentPawnState.name == "WallSplatted") { return; }
		Vector3 i_normalDirectionNormalized = _normalDirection.normalized;
		if (Mathf.Abs(i_normalDirectionNormalized.y )> (Mathf.Abs(i_normalDirectionNormalized.x) + Mathf.Abs(i_normalDirectionNormalized.z)))
		{
			Debug.Log("Tried wallsplat on ground: Return"); return;
		}
		ChangePawnState("WallSplatted", WallSplat_C(_force, _normalDirection), CancelWallSplat_C());
	}
	private void PushLight ( Vector3 _pushFlatDirection, PushForce _force )
	{
		float _pushDistance = 0;
		float _pushDuration = 0;
		float _pushHeight = 0;
		switch (_force)
		{
			case PushForce.Force1:
				if (isPlayer)
				{
					_pushDistance = pushDatas.lightPushPlayerForce1Distance;
					_pushDuration = pushDatas.lightPushPlayerForce1Duration;
					_pushHeight = pushDatas.lightPushPlayerForce1Height;
				}
				else
				{
					_pushDistance = pushDatas.lightPushForce1Distance;
					_pushDuration = pushDatas.lightPushForce1Duration;
					_pushHeight = pushDatas.lightPushForce1Height;
				}
				break;
			case PushForce.Force2:
				if (isPlayer)
				{
					_pushDistance = pushDatas.lightPushPlayerForce2Distance;
					_pushDuration = pushDatas.lightPushPlayerForce2Duration;
					_pushHeight = pushDatas.lightPushPlayerForce2Height;
				} else
				{
					_pushDistance = pushDatas.lightPushForce2Distance;
					_pushDuration = pushDatas.lightPushForce2Duration;
					_pushHeight = pushDatas.lightPushForce2Height;
				}
				break;
			case PushForce.Force3:
				if (isPlayer)
				{
					_pushDistance = pushDatas.lightPushPlayerForce3Distance;
					_pushDuration = pushDatas.lightPushPlayerForce3Duration;
					_pushHeight = pushDatas.lightPushPlayerForce3Height;
				} else
				{
					_pushDistance = pushDatas.lightPushForce3Distance;
					_pushDuration = pushDatas.lightPushForce3Duration;
					_pushHeight = pushDatas.lightPushForce3Height;
				}
				break;
		}
		ChangePawnState("PushedLight", CustomLightPush_C(_pushFlatDirection, _pushDistance, _pushDuration, _pushHeight), CancelPush_C());
	}
	#endregion

	#region IEnumerator functions
	private IEnumerator ClimbLedge_C ( Collider _ledge )
	{
		Vector3 i_startPosition = transform.position;
		Vector3 i_endPosition = i_startPosition;
		i_endPosition.y = _ledge.transform.position.y + _ledge.bounds.extents.y + 1f;
		GameObject i_endPosGuizmo = new GameObject();
		i_endPosGuizmo.transform.position = i_endPosition;
		//Go to the correct Y position
		for (float i = 0; i < climbDuration; i += Time.deltaTime)
		{
			transform.position = Vector3.Lerp(i_startPosition, i_endPosition, i / climbDuration);
			yield return new WaitForEndOfFrame();
		}
		transform.position = i_endPosition;
		rb.AddForce(Vector3.up * climbUpwardPushForce + transform.forward * climbForwardPushForce);
		moveState = MoveState.Idle;
		if (animator != null)
		{
			animator.ResetTrigger("ClimbTrigger");
		}
	}
	private IEnumerator PushHeavy_C ( Vector3 _pushFlatDirection, PushForce _force )
	{
        float i_pushDistance = 0;
		float i_pushDuration = 0;
		float i_pushHeight = 0;
		switch (_force)
		{
			case PushForce.Force1:
				if (isPlayer)
				{
					i_pushDistance = pushDatas.heavyPushPlayerForce1Distance;
					i_pushDuration = pushDatas.heavyPushPlayerForce1Duration;
					i_pushHeight = pushDatas.heavyPushPlayerForce1Height;
				} else
				{
					i_pushDistance = pushDatas.heavyPushForce1Distance;
					i_pushDuration = pushDatas.heavyPushForce1Duration;
					i_pushHeight = pushDatas.heavyPushForce1Height;
				}
				break;					  
			case PushForce.Force2:
				if (isPlayer)
				{
					i_pushDistance = pushDatas.heavyPushPlayerForce2Distance;
					i_pushDuration = pushDatas.heavyPushPlayerForce2Duration;
					i_pushHeight = pushDatas.heavyPushPlayerForce2Height;
				} else
				{
					i_pushDistance = pushDatas.heavyPushForce2Distance;
					i_pushDuration = pushDatas.heavyPushForce2Duration;
					i_pushHeight = pushDatas.heavyPushForce2Height;
				}
				break;					  
			case PushForce.Force3:
				if (isPlayer)
				{
					i_pushDistance = pushDatas.heavyPushPlayerForce3Distance;
					i_pushDuration = pushDatas.heavyPushPlayerForce3Duration;
					i_pushHeight = pushDatas.heavyPushPlayerForce3Height;
				} else
				{
					i_pushDistance = pushDatas.heavyPushForce3Distance;
					i_pushDuration = pushDatas.heavyPushForce3Duration;
					i_pushHeight = pushDatas.heavyPushForce3Height;
				}
				break;
		}
		animator.SetBool("PushedBool", true);

        DestroySpawnedAttackUtilities();

        FeedbackManager.SendFeedback("event.PlayerBeingHit", this, transform.position, transform.up, transform.up);
		moveState = MoveState.Pushed;
		_pushFlatDirection.y = 0;
		_pushFlatDirection = _pushFlatDirection.normalized * i_pushDistance;
		Vector3 moveDirection = _pushFlatDirection;
		moveDirection.y = i_pushHeight;
		transform.forward = moveDirection;
		Vector3 initialPosition = transform.position;
		Vector3 endPosition = initialPosition + moveDirection;
		Vector3 moveOffset = Vector3.zero;
		for (float i = 0f; i < i_pushDuration; i += Time.deltaTime)
		{
			moveState = MoveState.Pushed;
			moveOffset += new Vector3(moveInput.x, 0, moveInput.z) * Time.deltaTime * pushDatas.heavyPushAirControlSpeed;
			Vector3 newPosition = Vector3.Lerp(initialPosition, endPosition, pushDatas.heavyPushSpeedCurve.Evaluate(i / i_pushDuration)) + moveOffset;

			//check of collision
			Collider hitCollider = CheckForCollision();
			if (hitCollider != null)
			{
				if (isPlayer)
				{
					WallSplat(WallSplatForce.Light, transform.position - hitCollider.ClosestPoint(transform.position));
				}
				else
				{
					WallSplat(WallSplatForce.Light, transform.position - hitCollider.ClosestPoint(transform.position));
				}
			}

			transform.position = newPosition;
			yield return null;
		}
		moveState = MoveState.Idle;
		animator.SetBool("PushedBool", false);

		yield return null;
        //----Custom code in child script---------
        HeavyPushAction();
    }
	private IEnumerator CustomLightPush_C ( Vector3 _pushFlatDirection, float _pushDistance, float _pushDuration, float _pushHeight )
	{
		moveState = MoveState.Pushed;
		_pushFlatDirection.y = 0;
		_pushFlatDirection = _pushFlatDirection.normalized * _pushDistance;
		Vector3 i_moveDirection = _pushFlatDirection;
		i_moveDirection.y = _pushHeight;
		Vector3 i_initialPosition = transform.position;
		Vector3 i_endPosition = i_initialPosition + i_moveDirection;
		Vector3 i_moveOffset = Vector3.zero;
		for (float i = 0f; i < _pushDuration; i += Time.deltaTime)
		{
			moveState = MoveState.Pushed;
			i_moveOffset += new Vector3(moveInput.x, 0, moveInput.z) * Time.deltaTime * pushDatas.lightPushAircontrolSpeed;
			Vector3 i_newPosition = Vector3.Lerp(i_initialPosition, i_endPosition, pushDatas.lightPushSpeedCurve.Evaluate(i / _pushDuration)) + i_moveOffset;
			Vector3 i_newDirection = i_newPosition - transform.position;

			//check of collision
			Collider i_hitCollider = CheckForCollision();
			if (i_hitCollider != null)
			{
				if (isPlayer)
				{
					StopCurrentState();
				}
				else
				{
					StopCurrentState();
				}
			}
			transform.position = i_newPosition;
			yield return null;
		}
		moveState = MoveState.Idle;
	}
	private IEnumerator StartStateCoroutine ( IEnumerator _coroutine, PawnState _state )
	{
		if (_state.invincibleDuringState)
		{
			SetInvincible(true);
		}
		currentStateCoroutine = MonoBehaviourExtension.StartCoroutineEx(this, _coroutine);
		yield return currentStateCoroutine.WaitFor();
		currentPawnState = null;
		yield return null;
		if (_state.invincibleDuringState)
		{
			SetInvincible(false);
		}
		currentStateCoroutine = null;
	}
	private IEnumerator CancelPush_C()
	{
		animator.SetBool("PushedBool", false);
		moveState = MoveState.Idle;
		yield return null;
	}
	private IEnumerator WallSplat_C (WallSplatForce _force, Vector3 _normalDirection)
	{
		animator.SetTrigger("FallingTrigger");
		moveState = MoveState.Pushed;
		if (isPlayer)
		{
			FeedbackManager.SendFeedback("event.PlayerWallSplatHit", this);
		} else
		{
			FeedbackManager.SendFeedback("event.EnemyWallSplatHit", this);
		}
		transform.forward = _normalDirection;
		Vector3 i_initialPosition = transform.position;
		float i_damages = pushDatas.wallSplatDamages;
		if (isPlayer) { i_damages = pushDatas.wallSplatPlayerDamages; }
		Damage(i_damages);
		switch (_force)
		{
			case WallSplatForce.Light:
				animator.SetTrigger("WallSplatTrigger");
				animator.SetTrigger("StandingUpTrigger");
				float i_wallSplatLightRecoverTime = pushDatas.wallSplatLightRecoverTime + Random.Range(pushDatas.randomWallSplatLightRecoverTimeAddition.x, pushDatas.randomWallSplatLightRecoverTimeAddition.y) ;
				if (isPlayer) { i_wallSplatLightRecoverTime = pushDatas.wallSplatPlayerLightRecoverTime; }
				yield return new WaitForSeconds(i_wallSplatLightRecoverTime);
				break;
			case WallSplatForce.Heavy:
				animator.SetTrigger("WallSplatTrigger");
				float i_wallSplatForward = pushDatas.wallSplatHeavyForwardPush;
				float i_wallSplatFallSpeed = pushDatas.wallSplatHeavyFallSpeed;
				float i_wallSplatHeavyRecoverTime = pushDatas.wallSplatHeavyRecoverTime + Random.Range(pushDatas.randomWallSplatHeavyRecoverTimeAddition.x, pushDatas.randomWallSplatHeavyRecoverTimeAddition.y);
				AnimationCurve i_wallSplatSpeedCurve = pushDatas.wallSplatHeavySpeedCurve;
				AnimationCurve i_wallSplatHeightCurve = pushDatas.wallSplatHeavyHeightCurve;
				if (isPlayer) { 
					i_wallSplatForward = pushDatas.wallSplatPlayerHeavyForwardPush;
					i_wallSplatFallSpeed = pushDatas.wallSplatPlayerHeavyFallSpeed;
					i_wallSplatHeavyRecoverTime = pushDatas.wallSplatPlayerHeavyRecoverTime;
					i_wallSplatSpeedCurve = pushDatas.wallSplatPlayerHeavySpeedCurve;
					i_wallSplatHeightCurve = pushDatas.wallSplatPlayerHeavyHeightCurve;
				}
				Vector3 i_endPosition = transform.position + (_normalDirection * i_wallSplatForward);
				RaycastHit i_hit;
				if (Physics.Raycast(i_endPosition, Vector3.down, out i_hit, 1000, LayerMask.GetMask("Environment")))
				{
					i_endPosition = i_hit.point;
				}
				for (float i = 0; i < 1f; i+= Time.deltaTime * i_wallSplatFallSpeed)
				{
					moveState = MoveState.Pushed;
					Vector3 i_newPosition = Vector3.Lerp(i_initialPosition, i_endPosition, i_wallSplatSpeedCurve.Evaluate(i / 1f));
					i_newPosition.y = Mathf.Lerp(i_initialPosition.y, i_endPosition.y, 1f - i_wallSplatHeightCurve.Evaluate(i / 1f));
					transform.position = i_newPosition;
					yield return null;
				}
				transform.position = i_endPosition;
				animator.SetTrigger("StandingUpTrigger");
				moveState = MoveState.Idle;
				yield return new WaitForSeconds(i_wallSplatHeavyRecoverTime);
				break;
		}
		moveState = MoveState.Idle;
		animator.ResetTrigger("FallingTrigger");
		animator.ResetTrigger("StandingUpTrigger");
		yield return null;
	}
	private IEnumerator CancelWallSplat_C()
	{
		moveState = MoveState.Idle;
		animator.SetTrigger("StandingUpTrigger");
		animator.ResetTrigger("FallingTrigger");
		animator.ResetTrigger("StandingUpTrigger");
		yield return null;
	}
	private IEnumerator Bump_C( Vector3 _bumpDirectionFlat, BumpForce _force )
    {
		animator.SetTrigger("FallingTrigger");
		animator.SetTrigger("BumpTrigger");
		moveState = MoveState.Pushed;
		FeedbackManager.SendFeedback(eventOnBeingBumpedAway, this);

		float bumpDistance = 0;
		float bumpDuration = 0;
		switch (_force)
		{
			case BumpForce.Force1:
				if (isPlayer)
				{
					bumpDistance = pushDatas.BumpPlayerForce1Distance;
					bumpDuration = pushDatas.BumpPlayerForce1Duration;
				} else
				{
					bumpDistance = pushDatas.BumpForce1Distance;
					bumpDuration = pushDatas.BumpForce1Duration;
				}
				break;
			case BumpForce.Force2:
				if (isPlayer)
				{
					bumpDistance = pushDatas.BumpPlayerForce2Distance;
					bumpDuration = pushDatas.BumpPlayerForce2Duration;
				} else
				{
					bumpDistance = pushDatas.BumpForce2Distance;
					bumpDuration = pushDatas.BumpForce2Duration;
				}
				break;
			case BumpForce.Force3:
				if (isPlayer)
				{
					bumpDistance = pushDatas.BumpPlayerForce3Distance;
					bumpDuration = pushDatas.BumpPlayerForce3Duration;
				} else
				{
					bumpDistance = pushDatas.BumpForce3Distance;
					bumpDuration = pushDatas.BumpForce3Duration;
				}
				break;
		}
		bumpDistance = bumpDistance + Random.Range(pushDatas.bumpRandomRangeModifier.x, pushDatas.bumpRandomRangeModifier.y);
		bumpDuration = bumpDuration + Random.Range(pushDatas.bumpRandomDurationModifier.x, pushDatas.bumpRandomDurationModifier.y);
		float i_restDuration = pushDatas.bumpRestDuration;
		if (isPlayer) { i_restDuration = pushDatas.bumpPlayerRestDuration; }
	     i_restDuration = i_restDuration + Random.Range(pushDatas.bumpRandomRestModifier.x, pushDatas.bumpRandomRestModifier.y);
		Vector3 i_bumpDuration = _bumpDirectionFlat;
		i_bumpDuration.y = 0;
		Vector3 i_bumpInitialPosition = transform.position;
		Vector3 i_bumpDestinationPosition = transform.position + i_bumpDuration * bumpDistance;

		transform.rotation = Quaternion.LookRotation(-i_bumpDuration);
		float i_gettingUpDuration = maxGettingUpDuration;

		EnemyBehaviour i_enemy = GetComponent<EnemyBehaviour>();
        if (i_enemy != null) { i_enemy.ChangeState(EnemyState.Bumped); }

        float i_bumpTimeProgression = 0;
        bool i_playedEndOfFall = false;

        while (i_bumpTimeProgression < 1)
        {
			moveState = MoveState.Pushed;

			i_bumpTimeProgression += Time.deltaTime / bumpDuration;

            //move !
			Vector3 i_newPosition = Vector3.Lerp(i_bumpInitialPosition, i_bumpDestinationPosition, pushDatas.bumpSpeedCurve.Evaluate(i_bumpTimeProgression));

			//check of collision
			Collider i_hitCollider = CheckForCollision();
			if (i_hitCollider != null)
			{
				if (isPlayer)
				{
					WallSplat(WallSplatForce.Heavy, transform.position - i_hitCollider.ClosestPoint(transform.position));
				}
				else
				{
					WallSplat(WallSplatForce.Heavy, transform.position - i_hitCollider.ClosestPoint(transform.position));
				}
			}

			rb.MovePosition(i_newPosition);

			//trigger end anim
			if (i_bumpTimeProgression >= whenToTriggerFallingAnim && i_playedEndOfFall == false)
            {
                animator.SetTrigger("FallingTrigger");
				if (damageAfterBump > 0)
				{
                    Damage(damageAfterBump);
                    if (currentHealth <=0)
                    {
                        Kill();
                    }
				}
                i_playedEndOfFall = true;
            }
            yield return null;
        }

		//when arrived on ground
		while (i_restDuration > 0)
		{
			i_restDuration -= Time.deltaTime;
			if (i_restDuration <= 0)
			{
				animator.SetTrigger("StandingUpTrigger");
			}
			yield return null;
		}

        //time to get up
        while (i_gettingUpDuration > 0)
        {
			i_gettingUpDuration -= Time.deltaTime;
            if (i_gettingUpDuration <= 0 && GetComponent<EnemyBehaviour>() != null)
			{
                i_enemy.ChangeState(EnemyState.Following);
			}
			yield return null;
		}
		moveState = MoveState.Idle;
	}
	private IEnumerator CancelBump_C()
	{
		animator.ResetTrigger("FallingTrigger");
		animator.ResetTrigger("StandingUpTrigger");
		moveState = MoveState.Pushed;
		float i_restDuration = pushDatas.bumpRestDuration;
		if (isPlayer) { i_restDuration = pushDatas.bumpPlayerRestDuration; }
		i_restDuration = i_restDuration + Random.Range(pushDatas.bumpRandomRestModifier.x, pushDatas.bumpRandomRestModifier.y);
		float i_gettingUpDuration = maxGettingUpDuration;

		//if (animator != null) { animator.SetTrigger("FallingTrigger"); }
		if (damageAfterBump > 0)
		{
			Damage(damageAfterBump);
		}
		//when arrived on ground
		while (i_restDuration > 0)
		{
			i_restDuration -= Time.deltaTime;
			if (i_restDuration <= 0 && animator != null)
			{
				//animator.SetTrigger("StandingUpTrigger");
			}
			yield return null;
		}

		//time to get up
		if (transform != null)
		{
			EnemyBehaviour i_enemy = GetComponent<EnemyBehaviour>();
			while (i_gettingUpDuration > 0)
			{
				i_gettingUpDuration -= Time.deltaTime;
				if (i_gettingUpDuration <= 0 && i_enemy != null)
				{
					i_enemy.ChangeState(EnemyState.Following);
				}
				yield return null;
			}
		}
		moveState = MoveState.Idle;
	}
	#endregion
}
