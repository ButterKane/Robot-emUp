using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public enum MoveState
{
    Idle,
    Walk,
    Blocked,
	Jumping,
	Climbing
}

public enum SlowReason
{
	Link,
	Freeze,
}

public class SpeedCoef
{
	public SpeedCoef ( float _speedCoef, float _duration, SlowReason _reason, bool _stackable )
	{
		speedCoef = _speedCoef;
		duration = _duration;
		reason = _reason;
		stackable = _stackable;
	}
	public float speedCoef;
	public float duration;
	public SlowReason reason;
	public bool stackable;
}

public class PawnController : MonoBehaviour
{
    [Header("General settings")]
	public int maxHealth;
    public bool isInvincible;
    public float invicibilityTime = 1;

	[Space(2)]
	[Header("Movement settings")]
	public float jumpForce;
    public AnimationCurve accelerationCurve;

	[Tooltip("Minimum required speed to go to walking state")] public float minWalkSpeed = 0.1f;
	public float maxSpeed;
	public float maxAcceleration = 10;

    [Space(2)]
    public float movingDrag = .4f;
    public float idleDrag = .4f;
    public float onGroundGravityMultiplyer;
	public float deadzone = 0.2f;
	[Range(0.01f, 1f)] public float turnSpeed = .25f;

	[Header("Climb settings")]
	public float timeBeforeClimb = 0.2f;
	public float minDistanceToClimb = 1f;
	public float climbDuration = 0.5f;

	public float climbForwardPushForce = 450f;
	public float climbUpwardPushForce = 450f;

	[Space(2)]
    [Header("Debug (Don't change)")]
	[SerializeField] private MoveState moveState;
	private float accelerationTimer;
    protected Vector3 moveInput;
	protected Vector3 lookInput;
    private Quaternion turnRotation;
	private float customDrag;
	private float customGravity;
	private float speed;
	public int currentHealth;
	private List<SpeedCoef> speedCoefs = new List<SpeedCoef>();
	private bool grounded = false;
	private float timeInAir;
	private float climbingHoldTime;
	private Rigidbody rb;
	protected Animator animator;
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
        isInvincible = false;
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
			rb.AddForce(moveInput * (accelerationCurve.Evaluate(rb.velocity.magnitude / maxSpeed * GetSpeedCoef()) * maxAcceleration), ForceMode.Acceleration);
			customDrag = movingDrag;
		} else
		{
			accelerationTimer = 0;
		}
    }

	void UpdateSpeedCoef()
	{
		List<SpeedCoef> newCoefList = new List<SpeedCoef>();
		foreach (SpeedCoef coef in speedCoefs)
		{
			coef.duration -= Time.deltaTime;
			if (coef.duration > 0)
			{
				newCoefList.Add(coef);
			}
		}
		speedCoefs = newCoefList;
	}

    void Move()
    {
        if (moveState == MoveState.Blocked) { speed = 0; return; }
        Vector3 myVel = rb.velocity;
        myVel.y = 0;
        myVel = Vector3.ClampMagnitude(myVel, maxSpeed * GetSpeedCoef());
        myVel.y = rb.velocity.y;
        rb.velocity = myVel;
        speed = rb.velocity.magnitude;
    }

	public void SetLookInput(Vector3 _direction)
	{
		lookInput = _direction;
	}

	public void Climb()
	{
		Collider foundLedge = CheckForLedge();
		if (foundLedge != null)
		{
			climbingHoldTime += Time.deltaTime;
		} else
		{
			climbingHoldTime = 0;
		}
		if (climbingHoldTime >= timeBeforeClimb && foundLedge != null)
		{
			moveState = MoveState.Climbing;
			if (animator != null) { animator.SetTrigger("ClimbTrigger"); }
			StartCoroutine(ClimbLedge(foundLedge));
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
		float speedCoef = 1;
		foreach (SpeedCoef coef in speedCoefs)
		{
			speedCoef *= coef.speedCoef;
		}
		if (isPlayer)
		{
			speedCoef *= MomentumManager.GetValue(MomentumManager.datas.playerSpeedMultiplier);
		} else
		{
			speedCoef *= MomentumManager.GetValue(MomentumManager.datas.enemySpeedMultiplier);
		}
		return speedCoef;
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
        Destroy(this.gameObject);
    }

    public void Push(Vector3 _direction, float _magnitude, Vector3 explosionPoint)
    {
        _direction = _direction.normalized * _magnitude;
        rb.AddExplosionForce(_magnitude, explosionPoint, 0);
    }

	public virtual void Damage(int _amount)
	{
        StartCoroutine(InvicibleFrame());
		currentHealth -= _amount;
		if (_amount <= 0)
		{
			Destroy(this.gameObject);
		}
		float scaleForce = ((float)_amount / (float)maxHealth) * 3f;
		scaleForce = Mathf.Clamp(scaleForce, 0.3f, 1f);
		transform.DOShakeScale(1f, scaleForce).OnComplete(ResetScale);
		if (GetComponent<PlayerController>() != null)
		{
			MomentumManager.DecreaseMomentum(MomentumManager.datas.momentumLossOnDamage);
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

	public void SetUntargetable ()
	{
		PassController potentialPassController = GetComponentInChildren<PassController>();
		if (potentialPassController != null)
		{
			potentialPassController.DropBall();
		}
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

	#endregion

	#region Private functions

	private void UpdateAnimatorBlendTree()
    {
		if (animator != null)
		{
			animator.SetFloat("IdleRunningBlend", speed / maxSpeed);
		}
    }

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

	public void SetInvincible(bool _state)
	{
		isInvincible = _state;
	}
    private IEnumerator InvicibleFrame()
    {
        isInvincible = true;
        gameObject.layer = 0; // 0 = Default, which matrix doesn't interact with ennemies
        yield return new WaitForSeconds(invicibilityTime);
        isInvincible = false;
        gameObject.layer = 8; // 8 = Player Layer
    }

	private IEnumerator ClimbLedge(Collider _ledge)
	{
		Vector3 startPosition = transform.position;
		Vector3 endPosition = startPosition;
		endPosition.y = _ledge.transform.position.y + _ledge.bounds.extents.y + 1f;
		GameObject endPosGuizmo = new GameObject();
		endPosGuizmo.transform.position = endPosition;
		//Go to the correct Y position
		for (float i = 0; i < climbDuration; i+= Time.deltaTime)
		{
			transform.position = Vector3.Lerp(startPosition, endPosition, i / climbDuration);
			yield return new WaitForEndOfFrame();
		}
		transform.position = endPosition;
		rb.AddForce(Vector3.up * climbUpwardPushForce + transform.forward * climbForwardPushForce);
		moveState = MoveState.Idle;
		if (animator != null)
		{
			animator.ResetTrigger("ClimbTrigger");
		}
	}
    #endregion
}
