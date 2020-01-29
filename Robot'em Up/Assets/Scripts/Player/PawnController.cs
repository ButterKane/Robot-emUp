using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using MyBox;
using UnityEngine.AI;

public enum MoveState
{
    Idle,
    Walk,
    Blocked,
    Bumped,
	Jumping,
	Climbing,
	Dead
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
	Unknown
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
    public bool ignoreEletricPlates = false;
    private IEnumerator invincibilityCoroutine;

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
    protected float bumpDistance;
	protected float bumpDuration;
	protected float restDuration;
	protected float gettingUpDuration;
	protected Vector3 bumpInitialPosition;
	protected Vector3 bumpDestinationPosition;
	protected Vector3 bumpDirection;
	protected float bumpTimeProgression;
	protected bool fallingTriggerLaunched;
	protected bool mustCancelBump;

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
	protected int damageAfterBump;
	protected NavMeshAgent navMeshAgent;

	protected PassController passController;

	//Events
	private static System.Action onShootEnd;

	public virtual void Awake()
    {
		initialScale = transform.localScale;
        isInvincible_access = false;
        invincibilityCoroutine = null;
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
		if (GetComponent<PlayerController>() != null)
		{
			isPlayer = true;
		}
        moveState = MoveState.Idle;
    }

    private void FixedUpdate()
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
	}

    #region Movement
    void CheckMoveState()
    {
        if (moveState == MoveState.Blocked) { return; }

        else if (rb.velocity.magnitude <= minWalkSpeed)
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
			StartCoroutine(ClimbLedge_C(i_foundLedge));
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

	public virtual void Damage(int _amount)
	{
        if (!isInvincible_access && invincibilityCoroutine == null)
        {
            invincibilityCoroutine = InvicibleFrame_C();
            StartCoroutine(invincibilityCoroutine);
			FeedbackManager.SendFeedback(eventOnBeingHit, this, transform.position, transform.up, transform.up);
			currentHealth -= _amount;
            if (currentHealth <= 0)
            {
                Kill();
            }
            float i_scaleForce = ((float)_amount / (float)maxHealth) * 3f;
            i_scaleForce = Mathf.Clamp(i_scaleForce, 0.3f, 1f);
			transform.DOShakeScale(1f, i_scaleForce).OnComplete(ResetScale);
			if (GetComponent<PlayerController>() != null)
            {
                MomentumManager.DecreaseMomentum(MomentumManager.datas.momentumLossOnDamage);
            }
        }
	}

	private void ResetScale()
	{
		transform.DOScale(initialScale, 0.1f);
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

	public Vector3 GetCenterPosition()
	{
		return transform.position + Vector3.up * (totalHeight / 2f);
	}

	public Vector3 GetHeadPosition()
	{
		return transform.position + Vector3.up * totalHeight;
	}

	public float GetHeight()
	{
		return totalHeight;
	}

    public void SetInvincible(bool _state)
    {
        isInvincible_access = _state;
    }
	public virtual void UpdateAnimatorBlendTree ()
	{
		if (animator == null) { return; }
	}

	public virtual void BumpMe(float _bumpDistance, float _bumpDuration, float _restDuration, Vector3 _bumpDirection, float _randomDistanceMod, float _randomDurationMod, float _randomRestDurationMod)
    {
		if (!isBumpable) { return; }
		FeedbackManager.SendFeedback(eventOnBeingBumpedAway, this);

		bumpDistance = _bumpDistance + Random.Range(-_randomDistanceMod, _randomDistanceMod);
        bumpDuration = _bumpDuration + Random.Range(-_randomDurationMod, _randomDurationMod);
        restDuration = _restDuration + Random.Range(-_randomRestDurationMod, _randomRestDurationMod);
        bumpDirection = _bumpDirection;

		bumpTimeProgression = 0;
		bumpInitialPosition = transform.position;
        bumpDestinationPosition = transform.position + bumpDirection * bumpDistance;

        transform.rotation = Quaternion.LookRotation(-bumpDirection);
        gettingUpDuration = maxGettingUpDuration;
        fallingTriggerLaunched = false;

        StartCoroutine(Bump_C());
    }

	public virtual void Push(Vector3 _pushDirection, float _pushForce, float _pushHeight)
	{
		FeedbackManager.SendFeedback("event.PlayerBeingHit", this, transform.position, transform.up, transform.up);
		_pushDirection.y = _pushHeight;
		//_pushDirection.y = Mathf.Clamp((_pushForce/10f),0.1f, 0.75f);
		rb.AddForce(_pushDirection.normalized * _pushForce, ForceMode.Impulse);
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

	
    private IEnumerator InvicibleFrame_C()
    {
        isInvincible_access = true;
        gameObject.layer = 0; // 0 = Default, which matrix doesn't interact with ennemies
        yield return new WaitForSeconds(invincibilityTime);
        isInvincible_access = false;
        invincibilityCoroutine = null;
        gameObject.layer = 8; // 8 = Player Layer
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

    private IEnumerator Bump_C()
    {
        float i_bumpTimeProgression = 0;
        bool i_mustCancelBump = false;

        while (i_bumpTimeProgression < 1)
        {
            i_bumpTimeProgression += Time.deltaTime / bumpDuration;

            //must stop ?
            int bumpRaycastMask = 1 << LayerMask.NameToLayer("Environment");
            if (Physics.Raycast(transform.position, bumpDirection, 1, bumpRaycastMask) && !i_mustCancelBump)
            {
                i_mustCancelBump = true;
                i_bumpTimeProgression = whenToTriggerFallingAnim;
            }

            //move !
            if (!i_mustCancelBump)
            {
                rb.MovePosition(Vector3.Lerp(bumpInitialPosition, bumpDestinationPosition, bumpDistanceCurve.Evaluate(i_bumpTimeProgression)));
            }

            //trigger end anim
            if (i_bumpTimeProgression >= whenToTriggerFallingAnim && !fallingTriggerLaunched)
            {
                fallingTriggerLaunched = true;
                animator.SetTrigger("FallingTrigger");
				if (damageAfterBump > 0)
				{
					currentHealth -= damageAfterBump;
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
				EnemyBehaviour enemy = GetComponent<EnemyBehaviour>();
				enemy.ChangeState(EnemyState.Following);
				enemy.ExitBumpedState();
			}
			yield return null;
		}

        yield return new WaitForSeconds(1);
    }
    #endregion
}
