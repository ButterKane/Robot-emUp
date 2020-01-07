using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using MyBox;

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
	PerfectReception
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
   public bool isInvincible_access
    {
        get { return _isInvincible; }
        set
        {
            _isInvincible = value;
        }
    }

	private bool _isInvincible;
    public float invincibilityTime = 1;
    private IEnumerator invincibilityCoroutine;
	public string onHitSound = "";
	public string onDeathSound = "";

	[Space(2)]
	[Separator("Movement settings")]
	public float jumpForce;
    public AnimationCurve accelerationCurve;

	[Tooltip("Minimum required speed to go to walking state")] public float minWalkSpeed = 0.1f;
	public float moveSpeed = 10;
	public float acceleration = 10;

    [Space(2)]
    public float movingDrag = .4f;
    public float idleDrag = .4f;
    public float onGroundGravityMultiplyer;
	public float deadzone = 0.2f;
	[Range(0.01f, 1f)] public float turnSpeed = .25f;

	[Separator("Climb settings")]
	public float timeBeforeClimb = 0.2f;
	public float minDistanceToClimb = 1f;
	public float climbDuration = 0.5f;

	public float climbForwardPushForce = 450f;
	public float climbUpwardPushForce = 450f;

	[Separator("FX")]
	public GameObject deathParticlePrefab;
	public float deathParticleScale = 2;
	public GameObject hitParticlePrefab;
	public float hitParticleScale = 3;

	[Space(2)]
    [Separator("Bumped Values")]
	public bool isBumpable = true;
	public float maxGettingUpDuration = 0.6f;
    public AnimationCurve bumpDistanceCurve;
    protected float bumpDistance;
	protected float bumpDuration;
	protected float restDuration;
	protected float gettingUpDuration;
	protected Vector3 bumpInitialPosition;
	protected Vector3 bumpDestinationPosition;
	protected Vector3 bumpDirection;
	protected float bumpTimeProgression;
    [Range(0, 1)] public float whenToTriggerFallingAnim = 0.302f;
	protected bool fallingTriggerLaunched;
    public float bumpRaycastDistance = 1;
	protected bool mustCancelBump;

    [Space(2)]
    [Header("Debug (Don't change)")]
    [System.NonSerialized] public MoveState moveState;
	private float accelerationTimer;
    protected Vector3 moveInput;
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
	public Rigidbody rb;
	[System.NonSerialized] public Animator animator;
	private Vector3 initialScale;
	private bool frozen;
	private bool isPlayer;
	protected bool targetable;

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
		currentHealth = maxHealth;
		targetable = true;
		if (GetComponent<PlayerController>() != null)
		{
			isPlayer = true;
		}
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
        if (moveInput.magnitude >= 0.1f)
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

	void UpdateSpeedCoef()
	{
		List<SpeedCoef> internal_newCoefList = new List<SpeedCoef>();
		foreach (SpeedCoef coef in speedCoefs)
		{
			coef.duration -= Time.deltaTime;
			if (coef.duration > 0)
			{
				internal_newCoefList.Add(coef);
			}
		}
		speedCoefs = internal_newCoefList;
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

	public void Climb()
	{
		Collider internal_foundLedge = CheckForLedge();
		if (internal_foundLedge != null)
		{
			climbingHoldTime += Time.deltaTime;
		} else
		{
			climbingHoldTime = 0;
		}
		if (climbingHoldTime >= timeBeforeClimb && internal_foundLedge != null)
		{
			moveState = MoveState.Climbing;
			if (animator != null) { animator.SetTrigger("ClimbTrigger"); }
			StartCoroutine(ClimbLedge_C(internal_foundLedge));
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

	public float GetSpeedCoef()
	{
		float internal_speedCoef = 1;
		foreach (SpeedCoef coef in speedCoefs)
		{
			internal_speedCoef *= coef.speedCoef;
		}
		if (isPlayer)
		{
			internal_speedCoef *= MomentumManager.GetValue(MomentumManager.datas.playerSpeedMultiplier);
		} else
		{
			internal_speedCoef *= MomentumManager.GetValue(MomentumManager.datas.enemySpeedMultiplier);
		}
		return internal_speedCoef;
	}

	public int GetHealth()
	{
		return currentHealth;
	}

	public int GetMaxHealth()
	{
		return maxHealth;
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

    public virtual void Kill()
    {
		if (onDeathSound != "")
		{
			SoundManager.PlaySound(onDeathSound, transform.position);
		}
		LockManager.UnlockTarget(transform);
		Destroy(this.gameObject);
    }

    public void Push(Vector3 _direction, float _magnitude, Vector3 explosionPoint)
    {
        _direction = _direction.normalized * _magnitude;
        rb.AddExplosionForce(_magnitude, explosionPoint, 0);
    }

	public virtual void Heal(int _amount)
	{
		int internal_newHealth = currentHealth + _amount;
		currentHealth = Mathf.Clamp(internal_newHealth, 0, GetMaxHealth());
	}

	public virtual void Damage(int _amount)
	{
        if (!isInvincible_access && invincibilityCoroutine == null)
        {
            invincibilityCoroutine = InvicibleFrame_C();
            StartCoroutine(invincibilityCoroutine);
            currentHealth -= _amount;
			if (onHitSound != "")
			{
				SoundManager.PlaySound(onHitSound, transform.position, transform);
			}
            if (currentHealth <= 0)
            {
                Kill();
            }
            float internal_scaleForce = ((float)_amount / (float)maxHealth) * 3f;
            internal_scaleForce = Mathf.Clamp(internal_scaleForce, 0.3f, 1f);
			transform.DOShakeScale(1f, internal_scaleForce).OnComplete(ResetScale);
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
		PassController internal_potentialPassController = GetComponentInChildren<PassController>();
		if (internal_potentialPassController != null)
		{
			internal_potentialPassController.DropBall();
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
		return transform.position + Vector3.up * 1;
	}

	public Vector3 GetHeadPosition()
	{
		return transform.position + Vector3.up * 1.8f;
	}

	public float GetHeight()
	{
		return 2f;
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
        bumpDistance = _bumpDistance + Random.Range(-_randomDistanceMod, _randomDistanceMod);
        bumpDuration = _bumpDuration + Random.Range(-_randomDurationMod, _randomDurationMod);
        restDuration = _restDuration + Random.Range(-_randomRestDurationMod, _randomRestDurationMod);
        bumpDirection = _bumpDirection;

        bumpInitialPosition = transform.position;
        bumpDestinationPosition = transform.position + bumpDirection * bumpDistance;

        transform.rotation = Quaternion.LookRotation(-bumpDirection);
        gettingUpDuration = maxGettingUpDuration;
        fallingTriggerLaunched = false;

        StartCoroutine(Bump_C());
    }
    #endregion

    #region Private functions


	Collider CheckForLedge()
	{
		if (!CanClimb()) { return null; }
		RaycastHit hit;
		if (Physics.Raycast(GetHeadPosition(), transform.forward, out hit, minDistanceToClimb, LayerMask.GetMask("Environment")))
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
		Vector3 internal_startPosition = transform.position;
		Vector3 internal_endPosition = internal_startPosition;
		internal_endPosition.y = _ledge.transform.position.y + _ledge.bounds.extents.y + 1f;
		GameObject internal_endPosGuizmo = new GameObject();
		internal_endPosGuizmo.transform.position = internal_endPosition;
		//Go to the correct Y position
		for (float i = 0; i < climbDuration; i+= Time.deltaTime)
		{
			transform.position = Vector3.Lerp(internal_startPosition, internal_endPosition, i / climbDuration);
			yield return new WaitForEndOfFrame();
		}
		transform.position = internal_endPosition;
		rb.AddForce(Vector3.up * climbUpwardPushForce + transform.forward * climbForwardPushForce);
		moveState = MoveState.Idle;
		if (animator != null)
		{
			animator.ResetTrigger("ClimbTrigger");
		}
	}

    private IEnumerator Bump_C()
    {
        float internal_bumpTimeProgression = 0;
        bool internal_mustCancelBump = false;

        while (internal_bumpTimeProgression < 1)
        {
            internal_bumpTimeProgression += Time.deltaTime / bumpDuration;

            //must stop ?
            int bumpRaycastMask = 1 << LayerMask.NameToLayer("Environment");
            if (Physics.Raycast(transform.position, bumpDirection, 1, bumpRaycastMask) && !internal_mustCancelBump)
            {
                internal_mustCancelBump = true;
                internal_bumpTimeProgression = whenToTriggerFallingAnim;
            }

            //move !
            if (!internal_mustCancelBump)
            {
                transform.position = Vector3.Lerp(bumpInitialPosition, bumpDestinationPosition, bumpDistanceCurve.Evaluate(internal_bumpTimeProgression));
            }

            //trigger end anim
            if (internal_bumpTimeProgression >= whenToTriggerFallingAnim && !fallingTriggerLaunched)
            {
                fallingTriggerLaunched = true;
                //Animator.SetTrigger("FallingTrigger");
            }
            yield return null;
        }

        //when arrived on ground
        if (restDuration > 0)
        {
            restDuration -= Time.deltaTime;
            if (restDuration <= 0)
            {
                //Animator.SetTrigger("StandingUpTrigger");
            }
        }

        //time to get up
        if (gettingUpDuration > 0)
        {
            gettingUpDuration -= Time.deltaTime;
            //if (gettingUpDuration <= 0)
            //ChangingState(EnemyState.Following);
        }

        yield return new WaitForSeconds(1);
    }
    #endregion
}
