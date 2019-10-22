using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XInputDotNetPure;

public enum MoveState
{
    Idle,
    Walk,
    Blocked,
	Jumping,
	Climbing
}
public enum ActionState
{
	None,
	Aiming,
	Shooting,
	Receiving
}

public enum SlowReason
{
	Link,
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

public class PlayerController : MonoBehaviour
{
    [Header("General settings")]
	public PlayerIndex playerIndex;
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
	public float heightTreshold = 0.2f;
	public float zAngleTolerance = 10f;
	public float timeBeforeClimb = 0.2f;
	public float minDistanceToClimb = 1f;
	public float climbDuration = 0.5f;

	[Space(2)]
	[Header("Input settings")]
	public float triggerTreshold = 0.1f;

	[Space(2)]
    [Header("Debug")]
	[SerializeField] private MoveState moveState;
	[SerializeField] private ActionState actionState;
	private float accelerationTimer;
    private Vector3 moveInput;
	private Vector3 lookInput;
    private Quaternion turnRotation;
    private bool inputDisabled;
	private float customDrag;
	private float customGravity;
	private float speed;
	private int currentHealth;
	private List<SpeedCoef> speedCoefs = new List<SpeedCoef>();
	private bool grounded = false;
	private float timeInAir;
	private float climbingHoldTime;

	//xInput refs
	GamePadState state;
	GamePadState prevState;

	//Automatically found references
	private Camera cam;
	private Rigidbody rb;
	private Animator animator;

	private PassController passController;
	private DunkController dunkController;
	private DashController dashController;

	//Events
	private static System.Action onShootEnd;

	private void Awake()
    {
        isInvincible = false;
        customGravity = onGroundGravityMultiplyer;
        customDrag = idleDrag;
		cam = Camera.main;
		rb = GetComponent<Rigidbody>();
		animator = GetComponentInChildren<Animator>();
		passController = GetComponent<PassController>();
		dunkController = GetComponent<DunkController>();
		dashController = GetComponent<DashController>();

		currentHealth = maxHealth;
    }

    void Update()
    {
		if (inputDisabled) { return; }
		GetInput();
    }

    private void FixedUpdate()
    {
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

    #region Input
    void GetInput()
    {
        if (inputDisabled) { moveInput = Vector3.zero; return; }

        if (HasGamepad())
        {
            GamepadInput();
        }
        else
        {
            KeyboardInput();
        }
    }

	public void Vibrate(float _duration, VibrationForce _force)
	{
		StartCoroutine(Vibrate_C(_duration, _force));
	}

    void GamepadInput()
    {
		prevState = state;
		state = GamePad.GetState(playerIndex);
		moveInput = (state.ThumbSticks.Left.X * cam.transform.right) + (state.ThumbSticks.Left.Y * cam.transform.forward) ;
		moveInput.y = 0;
        moveInput = moveInput.normalized * ((moveInput.magnitude - deadzone) / (1 - deadzone));
		lookInput = (state.ThumbSticks.Right.X * cam.transform.right) + (state.ThumbSticks.Right.Y * cam.transform.forward);
		if (lookInput.magnitude > 0.1f && passController.CanShoot())
		{
			ChangeActionState(ActionState.Aiming);
		} else if (actionState == ActionState.Aiming)
		{
			ChangeActionState(ActionState.None);
		}
		if (state.Triggers.Right > triggerTreshold && passController.CanShoot())
		{
			ChangeActionState(ActionState.Shooting);
		}
		if (state.Buttons.Y == ButtonState.Pressed && dunkController.CanDunk())
		{
			dunkController.Dunk();
		}
		if (state.Triggers.Left > triggerTreshold && dashController.CanDash())
		{
			dashController.Dash();
		}
		if (state.Buttons.A == ButtonState.Pressed && CanJump())
		{
			Jump();
		}
		if (Mathf.Abs(state.ThumbSticks.Left.X) > 0.5f || Mathf.Abs(state.ThumbSticks.Left.Y) > 0.5f)
		{
			Climb();
		}
	}

    void KeyboardInput()
    {
		if (playerIndex != PlayerIndex.One) { return; }
		Vector3 _inputX = Input.GetAxisRaw("Horizontal") * cam.transform.right;
        Vector3 _inputZ = Input.GetAxisRaw("Vertical") * cam.transform.forward;
        moveInput = _inputX + _inputZ;
        moveInput.y = 0;
        moveInput.Normalize();
		lookInput = MathHelper.GetMouseDirection(cam, transform.position);
		if (Input.GetMouseButton(1) && passController.CanShoot())
		{
			ChangeActionState(ActionState.Aiming);
		} else if (actionState == ActionState.Aiming)
		{
			ChangeActionState(ActionState.None);
		}
		if (Input.GetMouseButton(0))
		{
			ChangeActionState(ActionState.Shooting);
		}
		if (Input.GetMouseButton(2))
		{
			passController.Receive(FindObjectOfType<BallBehaviour>());
		}
		if (Input.GetKeyDown(KeyCode.Space) && dunkController.CanDunk())
		{
			dunkController.Dunk();
		}
		if (Input.GetKeyDown(KeyCode.E) && dashController.CanDash())
		{
			dashController.Dash();
		}
		if (Input.GetKeyDown(KeyCode.Space) && CanJump())
		{
			Jump();
		}
    }

    bool HasGamepad()
    {
        string[] names = Input.GetJoystickNames();
        for (int i = 0; i < names.Length; i++)
        {
            if (names[i].Length > 0)
            {
                return true;
            }
        }
        return false;
    }
    #endregion

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
		if (lookInput.magnitude >= 0.1f && actionState == ActionState.Aiming || actionState == ActionState.Shooting)
			turnRotation = Quaternion.Euler(0, Mathf.Atan2(lookInput.x, lookInput.z) * 180 / Mathf.PI, 0);

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

	void Climb()
	{
		Collider foundLedge = CheckForLedge();
		if (foundLedge != null)
		{
			//Debug.Log("Starting climb " + foundLedge);
			climbingHoldTime += Time.deltaTime;
		} else
		{
			climbingHoldTime = 0;
			//Debug.Log("Finished climb");
		}
		if (climbingHoldTime >= timeBeforeClimb && foundLedge != null)
		{
			//Debug.Log("ClimbingLedge");
			moveState = MoveState.Climbing;
			StartCoroutine(ClimbLedge(foundLedge));
		}
	}

	bool CanJump()
	{
		if (grounded && moveState != MoveState.Blocked) { return true; }
		return false;
	}

	bool CanClimb()
	{
		if (moveState == MoveState.Blocked || moveState == MoveState.Climbing)
		{
			return false;
		}
		return true;
	}
	void Jump()
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
				RaycastHit hit;
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

	public void ChangeActionState(ActionState _newState)
	{
		switch (_newState)
		{
			case ActionState.Aiming:
				if (!passController.CanShoot()) { return; }
				passController.EnablePassPreview();
				animator.SetTrigger("PrepareShootingTrigger");
				break;
			case ActionState.Shooting:
				if (!passController.CanShoot()) { return; }
				passController.Shoot();
				passController.DisablePassPreview();
				animator.SetTrigger("ShootingTrigger");
				ChangeActionState(ActionState.None);
				return;
			case ActionState.None:
				passController.DisablePassPreview();
				animator.ResetTrigger("PrepareShootingTrigger");
				animator.SetTrigger("ShootingMissedTrigger");
				break;
		}
		actionState = _newState;
	}

    #region Public functions
    public void DisableInput()
    {
        inputDisabled = true;
    }

	public float GetSpeedCoef()
	{
		float speedCoef = 1;
		foreach (SpeedCoef coef in speedCoefs)
		{
			speedCoef *= coef.speedCoef;
		}
		return speedCoef;
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

    public void EnableInput()
    {
        inputDisabled = false;
    }

    public void Kill()
    {
        Destroy(this.gameObject);
    }

    public void Push(Vector3 _direction, float _magnitude, Vector3 explosionPoint)
    {
        _direction = _direction.normalized * _magnitude;
        rb.AddExplosionForce(_magnitude, explosionPoint, 0);
    }

	public void DamagePlayer(int _amount)
	{
        StartCoroutine(InvicibleFrame());
		currentHealth -= _amount;
		if (_amount <= 0)
		{
			Destroy(this.gameObject);
		}
	}

	public Animator GetAnimator ()
	{
		return animator;
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
        animator.SetFloat("IdleRunningBlend", speed / maxSpeed);
    }

	IEnumerator Vibrate_C(float _duration, VibrationForce _force)
	{
		for (float i = 0; i < _duration; i+= Time.deltaTime)
		{
			switch (_force)
			{
				case VibrationForce.Light:
					GamePad.SetVibration(playerIndex, 0.1f, 0.1f);
					break;
				case VibrationForce.Medium:
					GamePad.SetVibration(playerIndex, 0.5f, 0.5f);
					break;
				case VibrationForce.Heavy:
					GamePad.SetVibration(playerIndex, 1f, 1f);
					break;
			}
			yield return new WaitForEndOfFrame();
		}
		GamePad.SetVibration(playerIndex, 0f, 0f);
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
        yield return new WaitForSeconds(invicibilityTime);
        isInvincible = false;
    }

	private IEnumerator ClimbLedge(Collider _ledge)
	{
		Vector3 startPosition = transform.position;
		//Vector3 endPosition = _ledge.ClosestPointOnBounds(startPosition);
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
		rb.AddForce(Vector3.up * 450 + transform.forward * 450);
		moveState = MoveState.Idle;
	}
    #endregion
}
