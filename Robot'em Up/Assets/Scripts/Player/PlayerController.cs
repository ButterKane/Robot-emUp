﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;
using MyBox;

public class PlayerController : PawnController, IHitable
{
	[Space(2)]
	[Separator("Player settings")]
	public PlayerIndex playerIndex;
	public float triggerTreshold = 0.1f;
	GamePadState state;
	private Camera cam;
	private bool inputDisabled;

	[SerializeField] private bool _lockable;  public bool lockable { get { return _lockable; } set { _lockable = value; } }
	[SerializeField] private float _lockHitboxSize; public float lockHitboxSize { get { return _lockHitboxSize; } set { _lockHitboxSize = value; } }
	[System.NonSerialized] public bool enableDash;
    [System.NonSerialized] public bool enableJump;
    [System.NonSerialized] public bool enableDunk;
    [System.NonSerialized] public bool enableMagnet;

	[Separator("Revive settings")]
	public GameObject FX_hit;
	public GameObject FX_heal;
	public GameObject FX_death;
	public GameObject FX_revive;

	public float deathExplosionRadius = 5;
	public int deathExplosionDamage = 10;
	public float deathExplosionForce = 10;
	public float reviveExplosionRadius = 5;
	public int reviveExplosionDamage = 10;
	public float reviveExplosionForce = 10;
	public float reviveSpeedCoef = 0.3f;

	public int revivePartsCount = 3;
	[Range(0,1)] public float partExplosionAngleRandomness = 0.1f;
	public Vector2 minMaxProjectionForce = new Vector2(9, 11);
	public float reviveHoldDuration = 3;
	public float reviveFreezeDuration = 1;

	private DunkController dunkController;
	private DashController dashController;
	private ExtendingArmsController extendingArmsController;
	private List<ReviveInformations> revivablePlayers = new List<ReviveInformations>(); //List of the players that can be revived

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
		if (state.Triggers.Right > triggerTreshold && revivablePlayers.Count <= 0)
		{
			passController.TryReception();
			passController.Shoot();
		}
		if (state.Buttons.Y == ButtonState.Pressed && enableDunk && revivablePlayers.Count <= 0)
		{
			dunkController.Dunk();
		}
		if (state.Triggers.Left > triggerTreshold && revivablePlayers.Count <= 0)
		{
			//extendingArmsController.ExtendArm();
			if (enableDash)
			{
				dashController.Dash();
			}
		}
		if (state.Buttons.A == ButtonState.Pressed && CanJump() && enableJump)
		{
			Jump();
		}
		if (Mathf.Abs(state.ThumbSticks.Left.X) > 0.5f || Mathf.Abs(state.ThumbSticks.Left.Y) > 0.5f)
		{
			Climb();
		}
		if (revivablePlayers.Count > 0)
		{
			if (state.Triggers.Right > triggerTreshold && state.Triggers.Left > triggerTreshold)
			{
				AddSpeedCoef(new SpeedCoef(reviveSpeedCoef, Time.deltaTime, SpeedMultiplierReason.Reviving, false));
				foreach (ReviveInformations p in revivablePlayers)
				{
					p.linkedPanel.FillAssemblingSlider();
				}
			} else
			{
				UnFreeze();
				SetTargetable();
			}
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

	public override void Heal ( int _amount )
	{
		base.Heal(_amount);
		FXManager.InstantiateFX(FX_heal, Vector3.zero, true, Vector3.zero, Vector3.one * 3.25f, transform);
		PlayerUI potentialPlayerUI = GetComponent<PlayerUI>();
		if (potentialPlayerUI != null)
		{
			potentialPlayerUI.DisplayHealth(HealthAnimationType.Gain);
		}
	}
	public override void Damage ( int _amount )
	{
        if (!IsInvincible)
        {
			PlayerUI potentialPlayerUI = GetComponent<PlayerUI>();
			if (potentialPlayerUI != null)
			{
				potentialPlayerUI.DisplayHealth(HealthAnimationType.Loss);
			}
            base.Damage(_amount);
			float damageForce = _amount / maxHealth;
			if (damageForce < 0.2)
			{
				Vibrate(0.2f, VibrationForce.Light);
			}
			else if (damageForce < 0.5)
			{
				Vibrate(0.3f, VibrationForce.Medium);
			} else
			{
				Vibrate(0.3f, VibrationForce.Heavy);
			}
            FXManager.InstantiateFX(FX_hit, Vector3.zero, true, Vector3.zero, Vector3.one * 2.25f, transform);
        }
	}

	public override void Kill ()
	{
		if (moveState == MoveState.Dead) { return; }
		SoundManager.PlaySound("DeathFromOneCharacter", transform.position, transform);
		moveState = MoveState.Dead;
		animator.SetTrigger("Dead");
		FXManager.InstantiateFX(FX_death, GetCenterPosition(), false, Vector3.zero, Vector3.one);
		DropBall();
		SetUntargetable();
		Freeze();
		DisableInput();
		StartCoroutine(HideAfterDelay(0.5f));
		StartCoroutine(ProjectEnemiesInRadiusAfterDelay(0.4f, deathExplosionRadius, deathExplosionForce, deathExplosionDamage, DamageSource.DeathExplosion));
		StartCoroutine(GenerateRevivePartsAfterDelay(0.4f));
		GameManager.deadPlayers.Add(this);
	}

	public void Revive(PlayerController _player)
	{
		moveState = MoveState.Idle;
		_player.animator.SetTrigger("Revive");
		_player.SetTargetable();
		_player.UnHide();
		_player.currentHealth = GetMaxHealth();
		_player.transform.position = transform.position + Vector3.up * 7 + Vector3.left * 0.1f;
		_player.FreezeTemporarly(reviveFreezeDuration);
		FreezeTemporarly(reviveFreezeDuration);
		SetTargetable();
		List<ReviveInformations> newRevivablePlayers = new List<ReviveInformations>();
		FXManager.InstantiateFX(FX_revive, GetCenterPosition(), false, Vector3.zero, Vector3.one * 5);
		SoundManager.PlaySound("AllyResurrection", _player.transform.position, transform);
		StartCoroutine(ProjectEnemiesInRadiusAfterDelay(0.4f, reviveExplosionRadius, reviveExplosionForce, reviveExplosionDamage, DamageSource.ReviveExplosion));
		foreach (ReviveInformations inf in revivablePlayers)
		{
			if (inf.linkedPlayer != _player)
			{
				newRevivablePlayers.Add(inf);
			}
		}
		revivablePlayers = newRevivablePlayers;
		GameManager.deadPlayers.Remove(_player);
	}

	void GenerateReviveParts()
	{
		float currentAngle = 0;
		float defaultAngleDifference = 360 / revivePartsCount;
		for (int i = 0; i < revivePartsCount; i++)
		{
			GameObject revivePart = Instantiate(Resources.Load<GameObject>("PlayerResource/PlayerCore"), null);
			revivePart.name = "Part " + i + " of " + gameObject.name;
			revivePart.transform.position = transform.position;
			Vector3 wantedDirectionAngle = SwissArmyKnife.RotatePointAroundPivot(Vector3.forward, Vector3.up, new Vector3(0, currentAngle, 0));
			float throwForce = Random.Range(minMaxProjectionForce.x, minMaxProjectionForce.y);
			wantedDirectionAngle.y = throwForce * 0.035f;
			revivePart.GetComponent<CorePart>().Init(this, wantedDirectionAngle.normalized * throwForce, revivePartsCount, 0);
			currentAngle += defaultAngleDifference + Random.Range(-defaultAngleDifference * partExplosionAngleRandomness, defaultAngleDifference * partExplosionAngleRandomness);
		}
	}

	public void FreezeTemporarly(float _duration)
	{
		StartCoroutine(FreezeTemporarly_C(_duration));
	}

	public void AddRevivablePlayer(ReviveInformations _player)
	{
		revivablePlayers.Add(_player);
	}

	IEnumerator HideAfterDelay(float _delay)
	{
		yield return new WaitForSeconds(_delay);
		Hide();
	}

	IEnumerator ProjectEnemiesInRadiusAfterDelay(float _delay, float _radius, float _force, int _damages, DamageSource _damageSource)
	{
		yield return new WaitForSeconds(_delay);
		foreach (Collider hit in Physics.OverlapSphere(transform.position, _radius))
		{
			IHitable potentialHitableObject = hit.transform.GetComponent<IHitable>();
			if (potentialHitableObject != null) { potentialHitableObject.OnHit(null, (hit.transform.position - transform.position).normalized * _force, this, _damages, _damageSource); }
		}
	}

	IEnumerator GenerateRevivePartsAfterDelay(float _delay)
	{
		yield return new WaitForSeconds(_delay);
		GenerateReviveParts();
	}

	IEnumerator FreezeTemporarly_C(float _duration)
	{
		Freeze();
		DisableInput();
		yield return new WaitForSeconds(_duration);
		UnFreeze();
		EnableInput();
	}

	void IHitable.OnHit ( BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source, Vector3 _bumpModificators = default(Vector3))
	{
		SoundManager.PlaySound("PlayerHitNoBump", transform.position);
		switch (_source)
		{
			case DamageSource.RedBarrelExplosion:
				Damage(_damages);
				break;
		}
	}
}
