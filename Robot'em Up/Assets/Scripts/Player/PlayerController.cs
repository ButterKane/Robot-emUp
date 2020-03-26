using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;
using MyBox;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

[ExecuteAlways]
public class PlayerController : PawnController, IHitable
{
	[Space(2)]
	[Separator("Player settings")]
	public PlayerIndex playerIndex;
	public float triggerTreshold = 0.1f;
	public Color highlightedColor;
	public Color highlightedSecondColor;
	[SerializeField] private bool lockable;  public bool lockable_access { get { return lockable; } set { lockable = value; } }
	[SerializeField] private float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }
	[SerializeField] private Vector3 lockSize3DModifier = Vector3.one; public Vector3 lockSize3DModifier_access { get { return lockSize3DModifier; } set { lockSize3DModifier = value; } }

    public bool enableMagnet;

	[Separator("Over heal settings")]
	public float overHealValue = 20f;
	public float delayBeforeOverhealDecay = 5f;
	public float overHealDecaySpeed = 5f;
	public AnimationCurve overHealDecaySpeedCurve;

	[Separator("Dash while revive available settings")]
	public float delayBeforeDash = 0.2f;

	[Separator("Revive settings")]
	public string eventOnResurrecting = "event.PlayerResurrecting";
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

	private List<ReviveInformations> revivablePlayers = new List<ReviveInformations>(); //List of the players that can be revived

	//Grab values
	[HideInInspector] public List<GrabbableInformation> targetedGrabbable = new List<GrabbableInformation>();
	private GrabbableInformation prioritaryGrabInformation;

	//Input values
	private bool inputDisabled;
	private bool dashPressed;
	private bool rightTriggerWaitForRelease;
	private bool leftTriggerWaitForRelease;
	private bool leftShouldWaitForRelease;
	private bool rbPressed;
	private float dashBuffer;
	private float timeSinceLastHeal;
	private float rbPressDuration;
	private bool reviving;

	//References
	private DunkController dunkController;
	private DashController dashController;
	[HideInInspector] public ExtendingArmsController extendingArmsController;
	public static Transform middlePoint;
	private GamePadState state;
	private Camera cam;
	private PlayerUI ui;
	public Collider mainCollider;

	public void Start ()
	{
		base.Awake();
		if (!Application.isPlaying) { return; }

		//References initialization
		mainCollider = GetComponent<Collider>();
		cam = Camera.main;
		dunkController = GetComponent<DunkController>();
		dashController = GetComponent<DashController>();
		extendingArmsController = GetComponent<ExtendingArmsController>();
		ui = GetComponent<PlayerUI>();

		//Variable initialization
		ExtendingArmsController.grabableObjects.Add(mainCollider);
		GameManager.alivePlayers.Add(this);
		GenerateMiddlePoint();
	}

	private void Update ()
	{
		UpdateMiddlePoint();
		GetInput();
		UpdateOverHeal();
    }
	protected override void LateUpdate ()
	{
		base.LateUpdate();
		CheckIfOutOfCamera();
	}

	private void GetInput ()
	{
		if (!Application.isPlaying || inputDisabled) { return; }
		if (HasGamepad())
		{
			GamepadInput();
		}
	}

	private void GenerateMiddlePoint()
	{
		if (middlePoint == null)
		{
			middlePoint = new GameObject().transform;
			middlePoint.name = "MiddlePoint";
			middlePoint.tag = "MiddlePoint";
			middlePoint.gameObject.AddComponent<Rigidbody>().isKinematic = true;
			DontDestroyOnLoad(middlePoint.gameObject);
			GameManager.DDOL.Add(middlePoint.gameObject);
			SphereCollider col = middlePoint.gameObject.AddComponent<SphereCollider>();
			col.isTrigger = true;
		}
	}
	private void UpdateMiddlePoint()
	{
		if (middlePoint != null && playerIndex == PlayerIndex.One)
		{
			middlePoint.transform.position = GameManager.playerOne.transform.position + ((GameManager.playerTwo.transform.position - GameManager.playerOne.transform.position) / 2f);
		}
	}
	private void CheckIfOutOfCamera ()
	{
		if (GameManager.timeInZone < 1f || !Application.isPlaying) { return; }
		Vector3 viewPortPosition = GameManager.mainCamera.WorldToViewportPoint(transform.position);
		float extents = GameManager.cameraGlobalSettings.outOfCameraMaxDistancePercentage;
		if (viewPortPosition.x > 1 + extents || viewPortPosition.x < -extents || viewPortPosition.y > 1 + extents || viewPortPosition.y < -extents) {
			rb.velocity = Vector3.zero;
			Vector3 centerPos = GameManager.mainCamera.ViewportToWorldPoint(new Vector3(0.5f,0.5f,Vector3.Distance(transform.position, GameManager.mainCamera.transform.position)));
			centerPos.y = transform.position.y;
			Vector3 direction = centerPos - transform.position;
			direction = direction.normalized;
			float intensity = Vector3.Distance(centerPos, transform.position);
			rb.AddForce(direction * intensity, ForceMode.Impulse);
		}
	}
	void GamepadInput ()
	{
		state = GamePad.GetState(playerIndex);
		Vector3 camForwardNormalized = cam.transform.forward;
		camForwardNormalized.y = 0;
		camForwardNormalized = camForwardNormalized.normalized;
		Vector3 camRightNormalized = cam.transform.right;
		camRightNormalized.y = 0;
		camRightNormalized = camRightNormalized.normalized;
		if ((currentPawnState != null && !currentPawnState.preventMoving) || currentPawnState == null)
		{
			moveInput = (state.ThumbSticks.Left.X * camRightNormalized) + (state.ThumbSticks.Left.Y * camForwardNormalized);
			moveInput.y = 0;
			moveInput = moveInput.normalized * ((moveInput.magnitude - deadzone) / (1 - deadzone));
			lookInput = (state.ThumbSticks.Right.X * camRightNormalized) + (state.ThumbSticks.Right.Y * camForwardNormalized);
		} else
		{
			moveInput = Vector3.zero;
		}
		if (lookInput.magnitude > 0.1f)
		{
			if (!rbPressed)
			{
				passController.Aim();
			}
			extendingArmsController.SetDirection(lookInput);
		} else
		{
			passController.StopAim();
		}
		if (state.Buttons.B == ButtonState.Pressed)
		{
			AddSpeedModifier(new SpeedCoef(reviveSpeedCoef, Time.deltaTime, SpeedMultiplierReason.Reviving, false));
			//StartCoroutine(PushEverything_C());
		}

		if (state.Triggers.Right > triggerTreshold && revivablePlayers.Count <= 0)
		{
			if (revivablePlayers.Count <= 0)
			{
				if (!rightTriggerWaitForRelease) { passController.TryReception(); passController.Shoot(); }
				rightTriggerWaitForRelease = true;
			} else
			{
				dashBuffer = 0;
				rightTriggerWaitForRelease = true;
			}
		}
		if (state.Triggers.Right < triggerTreshold)
		{
			rightTriggerWaitForRelease = false;
		}
		if (state.Buttons.LeftShoulder == ButtonState.Pressed && !leftShouldWaitForRelease)
		{
			Highlighter.HighlightBall();
			//Highlighter.HighlightObject(transform.Find("Model"), highlightedColor, highlightedSecondColor);
			leftShouldWaitForRelease = true;
		}
		if (state.Buttons.LeftShoulder == ButtonState.Released)
		{
			leftShouldWaitForRelease = false;
		}
		if (state.Buttons.Y == ButtonState.Pressed && revivablePlayers.Count <= 0)
		{
			dunkController.Dunk();
		}
		if (state.Buttons.RightShoulder == ButtonState.Pressed)
		{
			rbPressed = true;
			rbPressDuration += Time.deltaTime;
			if (extendingArmsController != null && targetedGrabbable.Count > 0)
			{
				float pressPercent = rbPressDuration / extendingArmsController.minGrabHoldDuration;
				pressPercent = Mathf.Clamp(pressPercent, 0f, 1f);
				extendingArmsController.UpdateDecalSize(pressPercent);
				GrabbableInformation prioritaryInf = targetedGrabbable[0];
				Vector3 prioritaryDirection = prioritaryInf.targetedPosition.position - GetCenterPosition();
				prioritaryDirection.y = 0;
				float prioritaryAngle = Vector3.SignedAngle(lookInput, prioritaryDirection, Vector3.up);
				foreach (GrabbableInformation gi in targetedGrabbable)
				{
					Vector3 giDirection = gi.targetedPosition.position - GetCenterPosition();
					giDirection.y = 0;
					float newAngle = Vector3.SignedAngle(lookInput, giDirection, Vector3.up);
					if (Mathf.Abs(newAngle) < Mathf.Abs(prioritaryAngle))
					{
						prioritaryInf = gi;
						prioritaryAngle = newAngle;
					}
				}
				prioritaryGrabInformation = prioritaryInf;
				if (prioritaryGrabInformation != null)
				{
					passController.StopAim();
					extendingArmsController.TogglePreview(true);
					ForceLookAt(prioritaryGrabInformation.targetedPosition.position); //Player will rotate toward look input
				}
			}
		} else if (state.Buttons.RightShoulder == ButtonState.Released && rbPressed)
		{
			rbPressed = false;
			if (extendingArmsController != null)
			{
				extendingArmsController.TogglePreview(false);
				if (rbPressDuration >= extendingArmsController.minGrabHoldDuration && targetedGrabbable.Count > 0)
				{
					extendingArmsController.ExtendArm();
				}
			}
			rbPressDuration = 0;
		}
		if (state.Triggers.Left > triggerTreshold)
		{
			leftTriggerWaitForRelease = true;
			if (dashPressed == false && !reviving)
			{
				if (revivablePlayers.Count <= 0)
				{
					Vector3 dashDirection = moveInput;
					if (moveInput.magnitude <= 0)
					{
						dashDirection = transform.forward;
					}
					dashController.Dash(dashDirection);
					dashPressed = true;
				}
				else if (!rightTriggerWaitForRelease)
				{
//					Debug.Log("DashBuffer++");
					dashBuffer += Time.deltaTime;
					if (dashBuffer >= delayBeforeDash)
					{
						Vector3 dashDirection = moveInput;
						if (moveInput.magnitude <= 0)
						{
							dashDirection = transform.forward;
						}
						dashController.Dash(dashDirection);
						dashPressed = true;
					}
				}
			}
		}
		else
		{
			if (leftTriggerWaitForRelease)
			{
				if (revivablePlayers.Count > 0 && dashBuffer < delayBeforeDash && !reviving)
				{
					Vector3 dashDirection = moveInput;
					if (moveInput.magnitude <= 0)
					{
						dashDirection = transform.forward;
					}
					dashController.Dash(dashDirection);
				}
				dashBuffer = 0;
				dashPressed = false;
				leftTriggerWaitForRelease = false;
			}
		}
		if (Mathf.Abs(state.ThumbSticks.Left.X) > 0.5f || Mathf.Abs(state.ThumbSticks.Left.Y) > 0.5f)
		{
			Climb();
		}
		if (revivablePlayers.Count > 0)
		{
			if (state.Triggers.Right > triggerTreshold && state.Triggers.Left > triggerTreshold)
			{
				reviving = true;
				AddSpeedModifier(new SpeedCoef(reviveSpeedCoef, Time.deltaTime, SpeedMultiplierReason.Reviving, false));
				foreach (ReviveInformations p in revivablePlayers)
				{
					p.linkedPanel.FillAssemblingSlider();
				}
			} else if (state.Triggers.Right <= 0 && state.Triggers.Left <= 0)
			{
				reviving = false;
			}
			else
			{
				UnFreeze();
				SetTargetable();
			}
		} else
		{
			if (reviving && state.Triggers.Right <= 0 && state.Triggers.Left <= 0)
			{
				reviving = false;
			}
		}
	}

	private void UpdateOverHeal ()
	{
		if (currentHealth > GetMaxHealth())
		{
			timeSinceLastHeal += Time.deltaTime;
		}
		if (currentHealth > GetMaxHealth() && timeSinceLastHeal >= delayBeforeOverhealDecay)
		{
			float lerpValue = (currentHealth - GetMaxHealth()) / overHealValue;
			currentHealth -= Time.deltaTime * (overHealDecaySpeedCurve.Evaluate(lerpValue) / overHealDecaySpeed);
		}
	}

	IEnumerator PushEverything_C ()
	{
		foreach (PawnController p in FindObjectsOfType<PawnController>())
		{
			p.Push(PushType.Heavy, -p.transform.forward, PushForce.Force2);
			yield return new WaitForEndOfFrame();
		}
	}

	private bool HasGamepad ()
	{
		string[] i_names = Input.GetJoystickNames();
		for (int i = 0; i < i_names.Length; i++)
		{
			if (i_names[i].Length > 0)
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

	public override void Heal ( int _amount )
	{
		timeSinceLastHeal = 0;
		if (currentHealth < GetMaxHealth())
		{
			base.Heal(_amount);
		} else
		{
			currentHealth += _amount;
			currentHealth = Mathf.Clamp(currentHealth, 0, GetMaxHealth() + overHealValue);
		}
		if (ui != null)
		{
			ui.DisplayHealth(HealthAnimationType.Gain);
		}
	}

	public override void UpdateAnimatorBlendTree ()
	{
		base.UpdateAnimatorBlendTree();
		animator.SetFloat("IdleRunningBlend", currentSpeed / moveSpeed);
	}
	public override void Damage ( float _amount, bool _enableInvincibilityFrame = false )
	{
        if (!IsInvincible())
        {
			animator.SetTrigger("HitTrigger");
			PlayerUI i_potentialPlayerUI = GetComponent<PlayerUI>();
			if (i_potentialPlayerUI != null)
			{
				i_potentialPlayerUI.DisplayHealth(HealthAnimationType.Loss);
			}
            base.Damage(_amount, _enableInvincibilityFrame);   // manages the recovery time as well
        }
	}

    public void KillWithoutCorePart()
	{
		if (moveState == MoveState.Dead) { return; }
		Analytics.CustomEvent("PlayerDeath", new Dictionary<string, object> { { "Zone", GameManager.GetCurrentZoneName() }, });
		dunkController.StopDunk();
		moveState = MoveState.Dead;
		animator.SetTrigger("Dead");
		passController.DropBall();
		SetUntargetable();
		Freeze();
		DisableInput();
		StartCoroutine(HideAfterDelay(0.5f));
		StartCoroutine(ProjectEnemiesInRadiusAfterDelay(0.4f, reviveExplosionRadius, reviveExplosionDamage, DamageSource.DeathExplosion));
		GameManager.deadPlayers.Add(this);
		if (GameManager.deadPlayers.Count > 1)
		{
			Analytics.CustomEvent("PlayerSimultaneousDeath", new Dictionary<string, object> { { "Zone", GameManager.GetCurrentZoneName() }, });
		}
		GameManager.alivePlayers.Remove(this);
	}
	public override void Kill ()
	{
		KillWithoutCorePart();
		StartCoroutine(GenerateRevivePartsAfterDelay(0.4f));
	}

	public void Revive(PlayerController _player)
	{
		Analytics.CustomEvent("PlayerRevive", new Dictionary<string, object> { { "Zone", GameManager.GetCurrentZoneName() }, });
		FeedbackManager.SendFeedback(eventOnResurrecting, this);
		moveState = MoveState.Idle;
		_player.moveState = MoveState.Idle;
		_player.animator.SetTrigger("Revive");
		_player.SetTargetable();
		_player.UnHide();
		_player.currentHealth = GetMaxHealth();
		_player.transform.position = transform.position + Vector3.up * 7 + Vector3.left * 0.1f;
		_player.FreezeTemporarly(reviveFreezeDuration);
		_player.EnableInput();
		StartCoroutine(DisableInputsTemporarly(reviveFreezeDuration * 2));
		FreezeTemporarly(reviveFreezeDuration);
		SetTargetable();
		List<ReviveInformations> i_newRevivablePlayers = new List<ReviveInformations>();
		StartCoroutine(ProjectEnemiesInRadiusAfterDelay(0.4f,reviveExplosionRadius, reviveExplosionDamage, DamageSource.ReviveExplosion));
		foreach (ReviveInformations inf in revivablePlayers)
		{
			if (inf.linkedPlayer != _player)
			{
				i_newRevivablePlayers.Add(inf);
			}
		}
		revivablePlayers = i_newRevivablePlayers;
		GameManager.deadPlayers.Remove(_player);
		GameManager.alivePlayers.Add(_player);
	}

	void GenerateReviveParts()
	{
		float i_currentAngle = 0;
		float i_defaultAngleDifference = 360 / revivePartsCount;
		for (int i = 0; i < revivePartsCount; i++)
		{
			GameObject i_revivePart = Instantiate(Resources.Load<GameObject>("PlayerResource/PlayerCore"), null);
			i_revivePart.name = "Part " + i + " of " + gameObject.name;
			i_revivePart.transform.position = transform.position;
			Vector3 i_wantedDirectionAngle = SwissArmyKnife.RotatePointAroundPivot(Vector3.forward, Vector3.up, new Vector3(0, i_currentAngle, 0));
			float i_throwForce = Random.Range(minMaxProjectionForce.x, minMaxProjectionForce.y);
			i_wantedDirectionAngle.y = i_throwForce * 0.035f;
			i_revivePart.GetComponent<CorePart>().Init(this, i_wantedDirectionAngle.normalized * i_throwForce, revivePartsCount, 0);
			i_currentAngle += i_defaultAngleDifference + Random.Range(-i_defaultAngleDifference * partExplosionAngleRandomness, i_defaultAngleDifference * partExplosionAngleRandomness);
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

	IEnumerator ProjectEnemiesInRadiusAfterDelay(float _delay, float _radius, int _damages, DamageSource _damageSource)
	{
		yield return new WaitForSeconds(_delay);
		foreach (Collider hit in Physics.OverlapSphere(transform.position, _radius))
		{
			IHitable potentialHitableObject = hit.transform.GetComponent<IHitable>();
			if (potentialHitableObject != null) { potentialHitableObject.OnHit(null, (hit.transform.position - transform.position).normalized, this, _damages, _damageSource); }
		}
	}

	IEnumerator GenerateRevivePartsAfterDelay(float _delay)
	{
		yield return new WaitForSeconds(_delay);
		GenerateReviveParts();
	}

	IEnumerator DisableInputsTemporarly(float _duration)
	{
		for (float i = 0; i < _duration; i+= Time.deltaTime)
		{
			if (!inputDisabled)
			{
				DisableInput();
			}
			yield return null;
		}
		EnableInput();
		yield return null;
	}
	IEnumerator FreezeTemporarly_C(float _duration)
	{
		Freeze();
		yield return new WaitForSeconds(_duration);
		UnFreeze();
	}

	void IHitable.OnHit ( BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, float _damages, DamageSource _source, Vector3 _bumpModificators)
	{
		Analytics.CustomEvent("PlayerDamage", new Dictionary<string, object> { { "Zone", GameManager.GetCurrentZoneName() }, { "Source", _source } } ) ;
		Vector3 i_normalizedImpactVector = new Vector3(_impactVector.x, 0, _impactVector.z);
        if (_source == DamageSource.Ball) { return; }
		switch (_source)
		{
			case DamageSource.RedBarrelExplosion:
                FeedbackManager.SendFeedback("event.PlayerBeingBumpedAway", this);
                BumpMe(i_normalizedImpactVector, BumpForce.Force2);
				Damage(_damages);
				break;

            case DamageSource.EnemyContact:
                FeedbackManager.SendFeedback(eventOnBeingHit, this);
                Damage(_damages);
                Push(PushType.Light, _impactVector, PushForce.Force1);
                break;

            case DamageSource.Laser:
                FeedbackManager.SendFeedback(eventOnBeingHit, this);
                Damage(_damages, false);
                break;

			case DamageSource.SpawnImpact:
                FeedbackManager.SendFeedback(eventOnBeingHit, this);
                Damage(_damages);
				Push(PushType.Light, _impactVector, PushForce.Force1);
				break;
		}
	}

	public static PlayerController GetNearestPlayer(Vector3 _point)
	{
		if (GameManager.alivePlayers.Count <= 0) { return null; }
		PlayerController nearestPlayer = GameManager.alivePlayers[0];
		float closestDistance = Vector3.Distance(nearestPlayer.transform.position, _point);
		foreach (PlayerController p in GameManager.alivePlayers)
		{
			float distance = Vector3.Distance(p.transform.position, _point);
			if (distance < closestDistance)
			{
				closestDistance = distance;
				nearestPlayer = p;
			}
		}
		return nearestPlayer;
	}

	public PlayerController GetOtherPlayer()
	{
		List<PlayerController> players = GameManager.alivePlayers;
		foreach (PlayerController p in players)
		{
			if (p != this) { return p; }
		}
		return null;
	}
}
