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

public enum PushForce
{
	Heavy,
	Light
}

public enum WallSplatForce
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
	public int maxHealth;
	public float totalHeight;
	public bool isInvincible_access
    {
        get { return isInvincible; }
        set
        {
            isInvincible = value;
        }
    }

	private bool isInvincible;
    public float invincibilityTime = 1;
    private bool isInvincibleWithCheat;
    public bool ignoreEletricPlates = false;
    private Coroutine invincibilityCoroutine;
	public PawnState currentState = null;

    [Space(2)]
    [Separator("Movement settings")]
    public bool canMove;
    [ConditionalField(nameof(canMove))] public float jumpForce;
    [ConditionalField(nameof(canMove))] public AnimationCurve accelerationCurve;

    [ConditionalField(nameof(canMove))]
    [Tooltip("Minimum required speed to go to walking state")] public float minWalkSpeed = 0.1f;
    [ConditionalField(nameof(canMove))] public float moveSpeed = 15;
    [ConditionalField(nameof(canMove))] public float acceleration = 200;

    [ConditionalField(nameof(canMove))] public float movingDrag = .4f;
    [ConditionalField(nameof(canMove))] public float idleDrag = .4f;
    [ConditionalField(nameof(canMove))] public float onGroundGravityMultiplyer;
    [ConditionalField(nameof(canMove))] public float deadzone = 0.2f;
    [ConditionalField(nameof(canMove))] [Range(0.01f, 1f)] public float turnSpeed = .25f;

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
    [ConditionalField(nameof(isBumpable))] public AnimationCurve bumpDistanceCurve;
    [ConditionalField(nameof(isBumpable))] public float bumpRaycastDistance = 1;
    [ConditionalField(nameof(isBumpable))] [Range(0, 1)] public float whenToTriggerFallingAnim = 0.302f;

	[Space(2)]
	[Separator("Events")]
	public string eventOnBeingHit = "event.PlayerBeingHit";
	public string eventOnBeingBumpedAway = "event.PlayerBeingBumpedAway";
	public string eventOnBeingGrounded = "event.PlayerGrounded";
	public string eventOnDeath = "event.Player1Death";
	public string eventOnHealing = "event.PlayerHealing";

    [Space(2)]
    [Separator("Debug (Don't change)")]
    [System.NonSerialized] public Rigidbody rb;
    [System.NonSerialized] public MoveState moveState;
	private float accelerationTimer;
    public Vector3 moveInput;
	protected Vector3 lookInput;
    private Quaternion turnRotation;
	private float customDrag;
	private float customGravity;
	protected float currentSpeed;
    [System.NonSerialized] public int currentHealth;
	private List<SpeedCoef> speedCoefs = new List<SpeedCoef>();
	private bool grounded = false;
	private float timeInAir;
	private float climbingHoldTime;
	[System.NonSerialized] public Animator animator;
	private Vector3 initialScale;
	private bool frozen;
	private bool isPlayer;
	protected bool targetable;
	protected float damageAfterBump;
	protected NavMeshAgent navMeshAgent;
	private float invincibilityCooldown;
    private float accumulatedDamage;    // Stores the damage that is not an integer. Useful for things like continuous damage
	private PawnStates pawnStates;
	private Coroutine pawnStateCoroutine;
	private Coroutine currentStateCoroutine;
	private IEnumerator currentStateStopCoroutine;
	private PushDatas pushDatas;


    [HideInInspector] public PassController passController;

	//Events
	private static System.Action onShootEnd;

	public virtual void Awake()
    {
		initialScale = transform.localScale;
        isInvincible_access = false;
        customGravity = onGroundGravityMultiplyer;
        customDrag = idleDrag;
		rb = GetComponent<Rigidbody>();
		animator = GetComponentInChildren<Animator>();
		passController = GetComponent<PassController>();
		navMeshAgent = GetComponent<NavMeshAgent>();
		if( navMeshAgent == null)
		{
			navMeshAgent = GetComponentInChildren<NavMeshAgent>();
		}
		currentHealth = maxHealth;
		targetable = true;
		pushDatas = PushDatas.GetDatas();
		if (GetComponent<PlayerController>() != null)
		{
			isPlayer = true;
		}
        moveState = MoveState.Idle;
        accumulatedDamage = 0;
		pawnStates = Resources.Load<PawnStates>("PawnStateDatas");
		currentState = null;
	}

	protected virtual void FixedUpdate()
    {
		if (frozen) { return; }
        CheckMoveState();
        Rotate();
		UpdateAcceleration();
		Move();
        ApplyDrag();
        ApplyCustomGravity();
        UpdateAnimatorBlendTree();
		UpdateSpeedCoef();
		CheckIfGrounded();
        if (!isInvincibleWithCheat)
        {
            UpdateInvincibilityCooldown();
        }
	}

    #region Movement
    void CheckMoveState()
    {
        if (moveState == MoveState.Blocked || moveState == MoveState.Pushed) { return; }

        if (rb.velocity.magnitude <= minWalkSpeed)
        {
            if (moveState != MoveState.Idle)
            {
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
            }
            customDrag = idleDrag;
            moveState = MoveState.Idle;
        }
    }

    void Rotate()
    {
		if (moveState == MoveState.Blocked || moveState == MoveState.Pushed) { return; }
		//Rotation while moving
		if (moveInput.magnitude >= 0.5f)
            turnRotation = Quaternion.Euler(0, Mathf.Atan2(moveInput.x, moveInput.z) * 180 / Mathf.PI, 0);

		//Rotation while aiming or shooting
		if (passController != null)
		{
			if (lookInput.magnitude >= 0.1f && passController.passState == PassState.Aiming || passController.passState == PassState.Shooting)
				turnRotation = Quaternion.Euler(0, Mathf.Atan2(lookInput.x, lookInput.z) * 180 / Mathf.PI, 0);
		}

		transform.rotation = Quaternion.Slerp(transform.rotation, turnRotation, turnSpeed);
	}

    void UpdateAcceleration()
    {
		if (moveInput.magnitude != 0)
		{
			accelerationTimer += Time.fixedDeltaTime;
			if (moveState == MoveState.Blocked) { return; }
			rb.AddForce(moveInput * (accelerationCurve.Evaluate(rb.velocity.magnitude / moveSpeed * GetSpeedCoef()) * acceleration), ForceMode.Acceleration);
			customDrag = movingDrag;
		} else
		{
			accelerationTimer = 0;
		}
    }


    void Move()
    {
        if (moveState == MoveState.Blocked) { currentSpeed = 0; return; }
		if (moveState == MoveState.Pushed) { return; }
        Vector3 myVel = rb.velocity;
        myVel.y = 0;
        myVel = Vector3.ClampMagnitude(myVel, moveSpeed * GetSpeedCoef());
        myVel.y = rb.velocity.y;
        rb.velocity = myVel;
        currentSpeed = rb.velocity.magnitude;
    }

	public void SetLookInput(Vector3 _direction)
	{
		lookInput = _direction;
	}

	public NavMeshAgent GetNavMesh()
	{
		return navMeshAgent;
	}

	public void Climb()
	{
		Collider i_foundLedge = CheckForLedge();
		if (i_foundLedge != null)
		{
			climbingHoldTime += Time.deltaTime;
		} else
		{
			climbingHoldTime = 0;
		}
		if (climbingHoldTime >= timeBeforeClimb && i_foundLedge != null)
		{
			moveState = MoveState.Climbing;
			if (animator != null) { animator.SetTrigger("ClimbTrigger"); }
			ChangeState("Climb", ClimbLedge_C(i_foundLedge));
		}
	}

	public bool IsTargetable ()
	{
		return targetable;
	}

	public bool CanJump()
	{
		if (grounded && moveState != MoveState.Blocked) { return true; }
		return false;
	}

	public bool CanClimb()
	{
		if (moveState == MoveState.Blocked || moveState == MoveState.Climbing)
		{
			return false;
		}
		return true;
	}

	public void Jump()
	{
		rb.AddForce(Vector3.up * jumpForce);
		moveState = MoveState.Jumping;
		grounded = false;
	}

	void CheckIfGrounded()
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
					if (moveState == MoveState.Jumping) { 
						moveState = MoveState.Idle; 
					}
				}
			}
		}
	}

    void ApplyDrag()
    {
        Vector3 myVel = rb.velocity;
        myVel.x *= 1 - customDrag;
        myVel.z *= 1 - customDrag;
        rb.velocity = myVel;
    }

    void ApplyCustomGravity()
    {
        rb.AddForce(new Vector3(0, -9.81f* customGravity, 0));
    }
    #endregion
    #region Public functions

	public void ChangeState(string _newStateName, IEnumerator _coroutineToStart, IEnumerator _coroutineToCancel = null)
	{
		PawnState newState = pawnStates.GetPawnStateByName(_newStateName);
		bool canOverrideState;
		if (currentState != null)
		{
			if (pawnStates.IsStateOverriden(currentState, newState) || currentStateCoroutine == null)
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
			currentStateCoroutine = StartCoroutine(StartStateCoroutine(_coroutineToStart, newState));
			PawnState newStateInstance = new PawnState();
			newStateInstance.allowBallReception = newState.allowBallReception;
			newStateInstance.allowBallThrow = newState.allowBallThrow;
			currentState = newState;
		}
	}

	void StopCurrentState()
	{
		if (currentState == null) { return; }
		if (currentState.invincibleDuringState)
		{
			SetInvincible(false);
		}
		if (currentStateCoroutine != null)
		{
			StopCoroutine(currentStateCoroutine);
		}

		if (currentStateCoroutine != null)
		{
			currentStateStopCoroutine.StartCoroutine();
		}
		currentState = null;
	}

	IEnumerator StartStateCoroutine(IEnumerator coroutine, PawnState state)
	{
		if (state.invincibleDuringState)
		{
			SetInvincible(true);
		}
		yield return coroutine;
		currentState = null;
		yield return null;
		if (state.invincibleDuringState)
		{
			SetInvincible(false);
		}
		currentStateCoroutine = null;
	}

	public int GetHealth()
	{
		return currentHealth;
	}

	public int GetMaxHealth()
	{
		return maxHealth;
	}

    public float GetSpeedCoef()
    {
        float i_speedCoef = 1;
        foreach (SpeedCoef coef in speedCoefs)
        {
            i_speedCoef *= coef.speedCoef;
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

    public void AddSpeedCoef(SpeedCoef _speedCoef )
	{
		if (!_speedCoef.stackable)
		{
			foreach (SpeedCoef coef in speedCoefs)
			{
				if (coef.reason == _speedCoef.reason)
				{
					return;
				}
			}
		}
		speedCoefs.Add(_speedCoef);
	}

    void UpdateSpeedCoef()
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


    public virtual void Kill()
    {
		LockManager.UnlockTarget(transform);
		FeedbackManager.SendFeedback(eventOnDeath, this);
		Destroy(this.gameObject);
    }

	public virtual void Heal(int _amount)
	{
		int i_newHealth = currentHealth + _amount;
		currentHealth = Mathf.Clamp(i_newHealth, 0, GetMaxHealth());
		FeedbackManager.SendFeedback(eventOnHealing, this);
	}

	public bool CanDamage()
	{
		if (invincibilityCoroutine != null || isInvincible_access) { return false; }
		if (currentState != null && currentState.invincibleDuringState) { return false; }
		return true;
	}
	public virtual void Damage(float _amount)
	{
		if (!CanDamage()){ return; }
		SetInvincible();
		FeedbackManager.SendFeedback(eventOnBeingHit, this, transform.position, transform.up, transform.up);

        int i_actualDamages = (int)_amount;
        accumulatedDamage += _amount - i_actualDamages;

        if (accumulatedDamage >= 1)
        {
            i_actualDamages += (int)accumulatedDamage;
            accumulatedDamage -= (int)accumulatedDamage;
        }

        currentHealth -= i_actualDamages;

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

	public void DropBall()
	{
		PassController i_potentialPassController = GetComponentInChildren<PassController>();
		if (i_potentialPassController != null)
		{
			i_potentialPassController.DropBall();
		}
	}
	public void SetUntargetable ()
	{
		foreach (Collider collider in GetComponentsInChildren<Collider>())
		{
			collider.enabled = false;
		}
		targetable = false;
	}
	public void SetTargetable ()
	{
		foreach (Collider collider in GetComponentsInChildren<Collider>())
		{
			collider.enabled = true;
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

	public void SetInvincible(bool _state)
    {
        isInvincible_access = _state;
        isInvincibleWithCheat = _state;
    }

	public virtual void UpdateAnimatorBlendTree ()
	{
		if (animator == null) { return; }
	}

	public virtual void BumpMe( Vector3 _bumpDirectionFlat, float _bumpDistance, float _bumpDuration, float _bumpHeight)
    {
		if (!isBumpable) { return; }
		ChangeState("Bump", Bump_C(_bumpDirectionFlat, _bumpDistance, _bumpDuration, _bumpHeight), CancelBump_C());
    }

	public virtual void Push ( PushForce _forceType, Vector3 _pushDirectionFlat, float _pushDistance, float _pushDuration, float _pushHeight)
	{
		switch (_forceType)
		{
			case PushForce.Light:
				ChangeState("PushLight", PushLight_C(_pushDirectionFlat, _pushDistance, _pushDuration, _pushHeight), CancelPush_C());
				break;
			case PushForce.Heavy:
				Vector3 forwardDirection = (_pushDirectionFlat.normalized * _pushDistance) + new Vector3(0,_pushHeight,0);
				transform.forward = forwardDirection;
				ChangeState("PushHeavy", PushHeavy_C(_pushDirectionFlat, _pushDistance, _pushDuration, _pushHeight), CancelPush_C());
				break;
		}
	}
    #endregion

    #region Private functions


	Collider CheckForLedge()
	{
		if (!CanClimb()) { return null; }
		RaycastHit hit;
		if (Physics.SphereCast(GetCenterPosition(), 1f, transform.forward, out hit, minDistanceToClimb, LayerMask.GetMask("Environment")))
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
		if (invincibilityCooldown > 0)
		{
			invincibilityCooldown -= Time.deltaTime;
		}
        else
		{
			isInvincible_access = false;
			gameObject.layer = 8; // 8 = Player Layer
		}
	}
	private void SetInvincible()
    {
        isInvincible_access = true;
        gameObject.layer = 0; // 0 = Default, which matrix doesn't interact with ennemies
		invincibilityCooldown = invincibilityTime;
    }

	private void OnCollisionEnter ( Collision collision )
	{
		//Check for wallsplat (For players)
		if (collision.gameObject.layer == LayerMask.NameToLayer("Environment"))
		{
			if (moveState == MoveState.Pushed)
			{
				WallSplat(WallSplatForce.Heavy, collision);
			}
		}
	}

	private Collider CheckForCollision()
	{
		Collider closestCollider = null;
		float closestDistance = 50;
		foreach (Collider c in Physics.OverlapSphere(transform.position, 0.5f, LayerMask.GetMask("Environment")))
		{
			Vector3 collisionPoint = c.ClosestPoint(transform.position);
			float distance = Vector3.Distance(collisionPoint, transform.position);
			if (distance < closestDistance)
			{
				closestCollider = c;
				closestDistance = distance;
			}
		}
		return closestCollider;
	}

	private void WallSplat( WallSplatForce _force, Collision _collision ) {
		if (currentState != null && currentState.name == "WallSplat") { return; }
		Vector3 _normalDirection = _collision.GetContact(0).normal;
		WallSplat(_force, _normalDirection);
	}

	private void WallSplat ( WallSplatForce _force, Vector3 _normalDirection)
	{
		if (currentState != null && currentState.name == "WallSplat") { return; }
		ChangeState("WallSplat", WallSplat_C(_force, _normalDirection));
	}

	private IEnumerator ClimbLedge_C(Collider _ledge)
	{
		Vector3 i_startPosition = transform.position;
		Vector3 i_endPosition = i_startPosition;
		i_endPosition.y = _ledge.transform.position.y + _ledge.bounds.extents.y + 1f;
		GameObject i_endPosGuizmo = new GameObject();
		i_endPosGuizmo.transform.position = i_endPosition;
		//Go to the correct Y position
		for (float i = 0; i < climbDuration; i+= Time.deltaTime)
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

	private IEnumerator PushLight_C ( Vector3 _pushFlatDirection, float _pushDistance, float _pushDuration, float _pushHeight )
	{
		moveState = MoveState.Pushed;
		_pushFlatDirection = _pushFlatDirection.normalized * _pushDistance;
		Vector3 moveDirection = _pushFlatDirection;
		moveDirection.y = _pushHeight;
		rb.useGravity = false;
		Vector3 initialPosition = transform.position;
		Vector3 endPosition = initialPosition + moveDirection;
		Vector3 moveOffset = Vector3.zero;
		for (float i = 0f; i < _pushDuration; i += Time.deltaTime)
		{
			moveState = MoveState.Pushed;
			moveOffset += new Vector3(moveInput.x, 0, moveInput.z) * Time.deltaTime * pushDatas.lightPushAircontrolSpeed;
			transform.position = Vector3.Lerp(initialPosition, endPosition, pushDatas.lightPushSpeedCurve.Evaluate(i / _pushDuration)) + moveOffset;
			yield return null;
		}
		rb.useGravity = true;
		moveState = MoveState.Idle;
	}

	private IEnumerator PushHeavy_C ( Vector3 _pushFlatDirection, float _pushDistance, float _pushDuration, float _pushHeight )
	{
		FeedbackManager.SendFeedback("event.PlayerBeingHit", this, transform.position, transform.up, transform.up);
		moveState = MoveState.Pushed;
		_pushFlatDirection = _pushFlatDirection.normalized * _pushDistance;
		Vector3 moveDirection = _pushFlatDirection;
		moveDirection.y = _pushHeight;
		transform.forward = moveDirection;
		rb.useGravity = false;
		Vector3 initialPosition = transform.position;
		Vector3 endPosition = initialPosition + moveDirection;
		Vector3 moveOffset = Vector3.zero;
		for (float i = 0f; i < _pushDuration; i += Time.deltaTime)
		{
			moveState = MoveState.Pushed;
			moveOffset += new Vector3(moveInput.x, 0, moveInput.z) * Time.deltaTime * pushDatas.heavyPushAirControlSpeed;
			Vector3 newPosition = Vector3.Lerp(initialPosition, endPosition, pushDatas.lightPushSpeedCurve.Evaluate(i / _pushDuration)) + moveOffset;


			//check of collision
			Collider hitCollider = CheckForCollision();
			if (hitCollider != null && !isPlayer)
			{
				WallSplat(WallSplatForce.Light, transform.position - hitCollider.ClosestPoint(transform.position));
			}

			transform.position = newPosition;
			yield return null;
		}
		rb.useGravity = true;
		moveState = MoveState.Idle;
	}

	private IEnumerator CancelPush_C()
	{
		rb.useGravity = true;
		moveState = MoveState.Idle;
		yield return null;
	}

	private IEnumerator WallSplat_C (WallSplatForce _force, Vector3 _normalDirection)
	{
		moveState = MoveState.Pushed;
		if (isPlayer)
		{
			FeedbackManager.SendFeedback("event.PlayerWallSplatHit", this);
		} else
		{
			FeedbackManager.SendFeedback("event.EnemyWallSplatHit", this);
		}
		transform.forward = _normalDirection;
		Vector3 initialPosition = transform.position;
		float damages = pushDatas.wallSplatDamages;
		if (isPlayer) { damages = pushDatas.wallSplatPlayerDamages; }
		Damage(pushDatas.wallSplatDamages);
		switch (_force)
		{
			case WallSplatForce.Light:
				animator.SetTrigger("WallSplatHit");
				animator.SetTrigger("WallSplatRecover");
				float wallSplatLightRecoverTime = pushDatas.wallSplatLightRecoverTime + Random.Range(pushDatas.randomWallSplatLightRecoverTimeAddition.x, pushDatas.randomWallSplatLightRecoverTimeAddition.y) ;
				if (isPlayer) { wallSplatLightRecoverTime = pushDatas.wallSplatPlayerLightRecoverTime; }
				yield return new WaitForSeconds(wallSplatLightRecoverTime);
				break;
			case WallSplatForce.Heavy:
				animator.SetTrigger("WallSplatHit");
				float wallSplatForward = pushDatas.wallSplatHeavyForwardPush;
				float wallSplatFallSpeed = pushDatas.wallSplatHeavyFallSpeed;
				float wallSplatHeavyRecoverTime = pushDatas.wallSplatHeavyRecoverTime + Random.Range(pushDatas.randomWallSplatHeavyRecoverTimeAddition.x, pushDatas.randomWallSplatHeavyRecoverTimeAddition.y);
				AnimationCurve wallSplatSpeedCurve = pushDatas.wallSplatHeavySpeedCurve;
				AnimationCurve wallSplatHeightCurve = pushDatas.wallSplatHeavyHeightCurve;
				if (isPlayer) { 
					wallSplatForward = pushDatas.wallSplatPlayerHeavyForwardPush;
					wallSplatFallSpeed = pushDatas.wallSplatPlayerHeavyFallSpeed;
					wallSplatHeavyRecoverTime = pushDatas.wallSplatPlayerHeavyRecoverTime;
					wallSplatSpeedCurve = pushDatas.wallSplatPlayerHeavySpeedCurve;
					wallSplatHeightCurve = pushDatas.wallSplatPlayerHeavyHeightCurve;
				}
				Vector3 endPosition = transform.position + (_normalDirection * wallSplatForward);
				RaycastHit hit;
				if (Physics.Raycast(endPosition, Vector3.down, out hit, 1000, LayerMask.GetMask("Environment")))
				{
					endPosition = hit.point;
				}
				for (float i = 0; i < 1f; i+= Time.deltaTime * wallSplatFallSpeed)
				{
					moveState = MoveState.Pushed;
					Vector3 newPosition = Vector3.Lerp(initialPosition, endPosition, wallSplatSpeedCurve.Evaluate(i / 1f));
					newPosition.y = Mathf.Lerp(initialPosition.y, endPosition.y, 1f - wallSplatHeightCurve.Evaluate(i / 1f));
					transform.position = newPosition;
					yield return null;
				}
				transform.position = endPosition;
				animator.SetTrigger("WallSplatRecover");
				moveState = MoveState.Idle;
				yield return new WaitForSeconds(wallSplatHeavyRecoverTime);
				break;
		}
		moveState = MoveState.Idle;
		yield return null;
	}


	private IEnumerator Bump_C( Vector3 _bumpDirectionFlat, float _bumpDistance, float _bumpDuration, float _bumpHeight )
    {
		moveState = MoveState.Pushed;
		FeedbackManager.SendFeedback(eventOnBeingBumpedAway, this);

		float bumpDistance = _bumpDistance + Random.Range(pushDatas.bumpRandomRangeModifier.x, pushDatas.bumpRandomRangeModifier.y);
		float bumpDuration = _bumpDuration + Random.Range(pushDatas.bumpRandomDurationModifier.x, pushDatas.bumpRandomDurationModifier.y);
		float restDuration = pushDatas.bumpRestDuration;
		if (isPlayer) { restDuration = pushDatas.bumpPlayerRestDuration; }
	     restDuration = restDuration + Random.Range(pushDatas.bumpRandomRestModifier.x, pushDatas.bumpRandomRestModifier.y);
		Vector3 bumpDirection = _bumpDirectionFlat;
		Vector3 bumpInitialPosition = transform.position;
		Vector3 bumpDestinationPosition = transform.position + bumpDirection * bumpDistance;

		transform.rotation = Quaternion.LookRotation(-bumpDirection);
		float gettingUpDuration = maxGettingUpDuration;
		//fallingTriggerLaunched = false;

		EnemyBehaviour enemy = GetComponent<EnemyBehaviour>();
        if (enemy != null) { enemy.ChangeState(EnemyState.Bumped); }

        float i_bumpTimeProgression = 0;

        while (i_bumpTimeProgression < 1)
        {
			moveState = MoveState.Pushed;

			i_bumpTimeProgression += Time.deltaTime / bumpDuration;

            //move !
			Vector3 newPosition = Vector3.Lerp(bumpInitialPosition, bumpDestinationPosition, bumpDistanceCurve.Evaluate(i_bumpTimeProgression));

			//check of collision
			Collider hitCollider = CheckForCollision();
			if (hitCollider != null && !isPlayer)
			{
				WallSplat(WallSplatForce.Heavy, transform.position - hitCollider.ClosestPoint(transform.position));
			}

			rb.MovePosition(newPosition);

			//trigger end anim
			if (i_bumpTimeProgression >= whenToTriggerFallingAnim )
            {
               // fallingTriggerLaunched = true;
                animator.SetTrigger("FallingTrigger");
				if (damageAfterBump > 0)
				{
                    Damage(damageAfterBump);
				}
			}
            yield return null;
        }

		//when arrived on ground
		while (restDuration > 0)
		{
			restDuration -= Time.deltaTime;
			if (restDuration <= 0)
			{
				animator.SetTrigger("StandingUpTrigger");
			}
			yield return null;
		}

        //time to get up
        while (gettingUpDuration > 0)
        {
			gettingUpDuration -= Time.deltaTime;
            if (gettingUpDuration <= 0 && GetComponent<EnemyBehaviour>() != null)
			{
				enemy.ChangeState(EnemyState.Following);
			}
			yield return null;
		}
		moveState = MoveState.Idle;
	}

	private IEnumerator CancelBump_C()
	{
		moveState = MoveState.Pushed;
		float restDuration = pushDatas.bumpRestDuration;
		if (isPlayer) { restDuration = pushDatas.bumpPlayerRestDuration; }
		restDuration = restDuration + Random.Range(pushDatas.bumpRandomRestModifier.x, pushDatas.bumpRandomRestModifier.y);
		float gettingUpDuration = maxGettingUpDuration;

		if (animator != null) { animator.SetTrigger("FallingTrigger"); }
		if (damageAfterBump > 0)
		{
			Damage(damageAfterBump);
		}
		//when arrived on ground
		while (restDuration > 0)
		{
			restDuration -= Time.deltaTime;
			if (restDuration <= 0 && animator != null)
			{
				animator.SetTrigger("StandingUpTrigger");
			}
			yield return null;
		}

		//time to get up
		if (transform != null)
		{
			EnemyBehaviour enemy = GetComponent<EnemyBehaviour>();
			while (gettingUpDuration > 0)
			{
				gettingUpDuration -= Time.deltaTime;
				if (gettingUpDuration <= 0 && enemy != null)
				{
					enemy.ChangeState(EnemyState.Following);
				}
				yield return null;
			}
		}
		moveState = MoveState.Idle;
	}
    #endregion
}
