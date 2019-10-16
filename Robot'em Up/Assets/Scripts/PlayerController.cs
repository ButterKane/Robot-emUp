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
}
public enum ActionState
{
	None,
	Aiming,
	Shooting,
	Receiving,
	Magnet
}

public class PlayerController : MonoBehaviour
{
    [Header("General settings")]
	public PlayerIndex playerIndex;

	[Space(2)]
    [Header("Movement settings")]
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

    [Space(2)]
    [Header("Debug")]
	[SerializeField] private MoveState moveState;
	[SerializeField] private ActionState actionState;
	private PassController passController;
	private float accelerationTimer;
    private Vector3 moveInput;
	private Vector3 lookInput;
    private Quaternion turnRotation;
    private bool inputDisabled;
	private float customDrag;
	private float customGravity;
	private float speed;

	//xInput refs
	GamePadState state;
	GamePadState prevState;

	//Automatically found references
	private Camera cam;
	private Rigidbody rb;
	private Animator animator;

	//Events
	private static System.Action onShootEnd;

	private void Awake()
    {
        customGravity = onGroundGravityMultiplyer;
        customDrag = idleDrag;
		cam = Camera.main;
		rb = GetComponent<Rigidbody>();
		animator = GetComponentInChildren<Animator>();
		passController = GetComponent<PassController>();
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

    void GamepadInput()
    {
		prevState = state;
		state = GamePad.GetState(playerIndex);
		moveInput = (state.ThumbSticks.Left.X * cam.transform.right) + (state.ThumbSticks.Left.Y * cam.transform.forward) ;
		moveInput.y = 0;
        moveInput = moveInput.normalized * ((moveInput.magnitude - deadzone) / (1 - deadzone));
		lookInput = (state.ThumbSticks.Right.X * cam.transform.right) + (state.ThumbSticks.Right.Y * cam.transform.forward);
		if (lookInput.magnitude > 0.1 && passController.CanShoot())
		{
			ChangeActionState(ActionState.Aiming);
		} else if (actionState == ActionState.Aiming)
		{
			ChangeActionState(ActionState.None);
		}
		if (state.Buttons.RightShoulder == ButtonState.Pressed && actionState == ActionState.Aiming)
		{
			ChangeActionState(ActionState.Shooting);
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
		if (Input.GetMouseButton(0))
		{
			ChangeActionState(ActionState.Shooting);
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
			rb.AddForce(moveInput * (accelerationCurve.Evaluate(rb.velocity.magnitude / maxSpeed) * maxAcceleration), ForceMode.Acceleration);
			customDrag = movingDrag;
		} else
		{
			accelerationTimer = 0;
		}
    }

    void Move()
    {
        if (moveState == MoveState.Blocked) { speed = 0; return; }
        Vector3 myVel = rb.velocity;
        myVel.y = 0;
        myVel = Vector3.ClampMagnitude(myVel, maxSpeed);
        myVel.y = rb.velocity.y;
        rb.velocity = myVel;
        speed = rb.velocity.magnitude;
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

	public void ChangeActionState(ActionState newState)
	{
		switch (newState)
		{
			case ActionState.Aiming:
				if (!passController.CanShoot()) { Debug.Log("Player can't shoot"); return; }
				passController.EnablePassPreview();
				break;
			case ActionState.Shooting:
				if (!passController.CanShoot()) { Debug.Log("Player can't shoot");  return; }
				passController.Shoot();
				passController.DisablePassPreview();
				ChangeActionState(ActionState.None);
				return;
			case ActionState.Magnet:
				break;
			case ActionState.None:
				passController.DisablePassPreview();
				break;
		}
		actionState = newState;
	}

    #region Public functions
    public void DisableInput()
    {
        inputDisabled = true;
    }

    public void EnableInput()
    {
        inputDisabled = false;
    }

    public void Kill()
    {
        Destroy(this.gameObject);
    }

    public void Push(Vector3 direction, float magnitude)
    {

        direction = direction.normalized * magnitude;
        rb.AddForce(Vector3.up * 2, ForceMode.Impulse);
        rb.AddForce(direction, ForceMode.Impulse);
    }

    #endregion

    #region Private functions

    private void UpdateAnimatorBlendTree()
    {
        animator.SetFloat("IdleRunningBlend", speed / maxSpeed);
    }

	#endregion
}
