using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;
using MyBox;

public class PlayerController : PawnController
{
	[Space(2)]
	[Separator("Player settings")]
	public PlayerIndex playerIndex;
	public float triggerTreshold = 0.1f;
	GamePadState state;
	private Camera cam;
	private bool inputDisabled;

	public bool enableDash;
	public bool enableJump;
	public bool enableDunk;
	public bool enablePickOwnBall;
	public bool enableMagnet;

	[Separator("Revive settings")]
	public GameObject FX_hit;
	public GameObject FX_death;
	public GameObject FX_revive;

	public float deathExplosionRadius = 5;
	public float deathExplosionDamage = 10;
	public float reviveExplosionRadius = 5;
	public float reviveExplosionDamage = 10;

	public float minAngleBetweenParts = 20;
	public float revivePartsCount = 3;
	public Vector2 minMaxProjectionForce = new Vector2(9, 11);
	public float reviveHoldDuration = 3;
	public float reviveFreezeDuration = 1;

	private DunkController dunkController;
	private DashController dashController;
	private ExtendingArmsController extendingArmsController;

	public override void Awake ()
	{
		base.Awake();
		cam = Camera.main;
		dunkController = GetComponent<DunkController>();
		dashController = GetComponent<DashController>();
		extendingArmsController = GetComponent<ExtendingArmsController>();
	}
	private void Update ()
	{
		if (Input.GetKeyDown(KeyCode.K))
		{
			Kill();
		}
		if (Input.GetKeyDown(KeyCode.L))
		{
			Revive();
		}
		if (!inputDisabled) { GetInput(); }
	}
	void GetInput ()
	{
		if (HasGamepad())
		{
			GamepadInput();
		}
		else
		{
			KeyboardInput();
		}
	}

	public void Vibrate ( float _duration, VibrationForce _force )
	{
		StartCoroutine(Vibrate_C(_duration, _force));
	}

	void GamepadInput ()
	{
		state = GamePad.GetState(playerIndex);
		moveInput = (state.ThumbSticks.Left.X * cam.transform.right) + (state.ThumbSticks.Left.Y * cam.transform.forward);
		moveInput.y = 0;
		moveInput = moveInput.normalized * ((moveInput.magnitude - deadzone) / (1 - deadzone));
		lookInput = (state.ThumbSticks.Right.X * cam.transform.right) + (state.ThumbSticks.Right.Y * cam.transform.forward);
		if (lookInput.magnitude > 0.1f)
		{
			passController.Aim();
		} else
		{
			passController.StopAim();
		}

		if (extendingArmsController != null)
		{
			if (lookInput.magnitude > 0.1f)
			{
				extendingArmsController.SetThrowDirection(lookInput);
			} else
			{
				extendingArmsController.DisableThrowDirectionIndicator();
			}
		}
		if (state.Triggers.Right > triggerTreshold)
		{
			passController.TryReception();
			passController.Shoot();
		}
		if (state.Buttons.Y == ButtonState.Pressed && enableDunk)
		{
			dunkController.Dunk();
		}
		if (state.Triggers.Left > triggerTreshold && enableDash)
		{
			//extendingArmsController.ExtendArm();
			dashController.Dash();
		}
		if (state.Buttons.A == ButtonState.Pressed && CanJump() && enableJump)
		{
			Jump();
		}
		if (Mathf.Abs(state.ThumbSticks.Left.X) > 0.5f || Mathf.Abs(state.ThumbSticks.Left.Y) > 0.5f)
		{
			Climb();
		}
	}

	void KeyboardInput ()
	{
		if (playerIndex != PlayerIndex.One) { return; }
		Vector3 _inputX = Input.GetAxisRaw("Horizontal") * cam.transform.right;
		Vector3 _inputZ = Input.GetAxisRaw("Vertical") * cam.transform.forward;
		moveInput = _inputX + _inputZ;
		moveInput.y = 0;
		moveInput.Normalize();
		lookInput = SwissArmyKnife.GetMouseDirection(cam, transform.position);
		if (extendingArmsController != null)
		{
			if (lookInput.magnitude > 0.1f)
			{
				extendingArmsController.SetThrowDirection(lookInput);
			} else
			{
				extendingArmsController.DisableThrowDirectionIndicator();
			}
		}
		if (Input.GetMouseButton(1))
		{
			passController.Aim();
		}
		if (Input.GetMouseButtonUp(1))
		{
			passController.StopAim();
		}
		if (Input.GetMouseButton(0))
		{
			passController.TryReception();
			passController.Shoot();
		}
		if (Input.GetMouseButton(2))
		{
			passController.Receive(FindObjectOfType<BallBehaviour>());
		}
		if (Input.GetKeyDown(KeyCode.Space) && enableDunk)
		{
			dunkController.Dunk();
		}
		if (Input.GetKeyDown(KeyCode.E) && enableDash)
		{
			//extendingArmsController.ExtendArm();
			dashController.Dash();
		}
		if (Input.GetKeyDown(KeyCode.Space) && CanJump() && enableJump)
		{
			Jump();
		}
		if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.5f)
		{
			Climb();
		}

	}

	bool HasGamepad ()
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
	public void DisableInput ()
	{
		inputDisabled = true;
	}

	public void EnableInput()
	{
		inputDisabled = false;
	}

	public Vector3 GetLookInput()
	{
		return lookInput;
	}

	IEnumerator Vibrate_C ( float _duration, VibrationForce _force )
	{
		for (float i = 0; i < _duration; i += Time.deltaTime)
		{
			switch (_force)
			{
				case VibrationForce.Light:
					GamePad.SetVibration(playerIndex, 0.1f, 0.1f);
					break;
				case VibrationForce.Medium:
					GamePad.SetVibration(playerIndex, 0.2f, 0.2f);
					break;
				case VibrationForce.Heavy:
					GamePad.SetVibration(playerIndex, 0.5f, 0.5f);
					break;
			}
			yield return new WaitForEndOfFrame();
		}
		GamePad.SetVibration(playerIndex, 0f, 0f);
	}

	public override void Damage ( int _amount )
	{
		base.Damage(_amount);
		FXManager.InstantiateFX(FX_hit, Vector3.zero, true, Vector3.zero, Vector3.one * 2.25f, transform);
		if (currentHealth <= 0)
		{
			Kill();
		}
	}

	public override void Kill ()
	{
		animator.SetTrigger("Dead");
		SetUntargetable();
		Freeze();
		Hide();
		DisableInput();
	}

	void Revive()
	{
		animator.SetTrigger("Revived");
		UnFreeze();
		UnHide();
		SetTargetable();
		EnableInput();
	}
}
