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
	GamePadState state;
	private Camera cam;
	private bool inputDisabled;
	public Color highlightedColor;
	public Color highlightedSecondColor;

	[SerializeField] private bool lockable;  public bool lockable_access { get { return lockable; } set { lockable = value; } }
	[SerializeField] private float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }
	public bool enableDash;
    public bool enableJump;
    public bool enableDunk;
    public bool enableMagnet;

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

	private DunkController dunkController;
	private DashController dashController;
	private ExtendingArmsController extendingArmsController;
	private List<ReviveInformations> revivablePlayers = new List<ReviveInformations>(); //List of the players that can be revived
	private bool dashPressed = false;
	private bool rightTriggerWaitForRelease;
	private bool leftShouldWaitForRelease;
	public static Transform middlePoint;
	private Vector3 previousPosition;

	public void Start ()
	{
		base.Awake();
		GameManager.alivePlayers.Add(this);
		cam = Camera.main;
		dunkController = GetComponent<DunkController>();
		dashController = GetComponent<DashController>();
		extendingArmsController = GetComponent<ExtendingArmsController>();
		if (Application.isPlaying)
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
	}

	protected override void FixedUpdate ()
	{
		previousPosition = transform.position;
		base.FixedUpdate();
	}
	private void Update ()
	{
		if (middlePoint != null && playerIndex == PlayerIndex.One)
		{
			middlePoint.transform.position = GameManager.playerOne.transform.position + ((GameManager.playerTwo.transform.position - GameManager.playerOne.transform.position) / 2f);
		}
		if (Application.isPlaying && !inputDisabled)
		{
			GetInput();
		}
    }
	private void LateUpdate ()
	{
		if (Application.isPlaying)
		{
			CheckIfOutOfCamera();
		}
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

	void CheckIfOutOfCamera ()
	{
		if (GameManager.timeInZone < 1f) { return; }
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
		moveInput = (state.ThumbSticks.Left.X * camRightNormalized) + (state.ThumbSticks.Left.Y * camForwardNormalized);
		moveInput.y = 0;
		moveInput = moveInput.normalized * ((moveInput.magnitude - deadzone) / (1 - deadzone));
		lookInput = (state.ThumbSticks.Right.X * camRightNormalized) + (state.ThumbSticks.Right.Y * camForwardNormalized);
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
			if (!rightTriggerWaitForRelease) { passController.TryReception(); passController.Shoot(); }
			rightTriggerWaitForRelease = true;
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
		if (state.Buttons.Y == ButtonState.Pressed && enableDunk && revivablePlayers.Count <= 0)
		{
			dunkController.Dunk();
		}
		if (state.Triggers.Left > triggerTreshold)
		{
			//extendingArmsController.ExtendArm();
			if (enableDash && revivablePlayers.Count <= 0 && dashPressed == false)
			{
				Vector3 dashDirection = moveInput;
				if (moveInput.magnitude <= 0)
				{
					dashDirection = transform.forward;
				}
				dashController.Dash(dashDirection);
				//Push(PushForce.Light, transform.forward, 10f, 1f, 1f);
			}
			dashPressed = true;
		} else
		{
			dashPressed = false;
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
		Vector3 i_inputX = Input.GetAxisRaw("Horizontal") * cam.transform.right;
		Vector3 i_inputZ = Input.GetAxisRaw("Vertical") * cam.transform.forward;
		moveInput = i_inputX + i_inputZ;
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
			//dunkController.Dunk();
		}
		if (Input.GetKeyDown(KeyCode.E) && enableDash)
		{
			//extendingArmsController.ExtendArm();
			Vector3 dashDirection = moveInput;
			if (moveInput.magnitude <= 0)
			{
				dashDirection = transform.forward;
			}
			dashController.Dash(dashDirection);
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

	public override void Heal ( float _amount )
	{
		base.Heal(_amount);
		PlayerUI i_potentialPlayerUI = GetComponent<PlayerUI>();
		if (i_potentialPlayerUI != null)
		{
			i_potentialPlayerUI.DisplayHealth(HealthAnimationType.Gain);
		}
	}

	public override void UpdateAnimatorBlendTree ()
	{
		base.UpdateAnimatorBlendTree();
		animator.SetFloat("IdleRunningBlend", currentSpeed / moveSpeed);
	}
	public override void Damage ( float _amount )
	{
        if (!isInvincible_access)
        {
			animator.SetTrigger("HitTrigger");
			PlayerUI i_potentialPlayerUI = GetComponent<PlayerUI>();
			if (i_potentialPlayerUI != null)
			{
				i_potentialPlayerUI.DisplayHealth(HealthAnimationType.Loss);
			}
            base.Damage(_amount);   // manages the recovery time as well
        }
	}

    public override void DamageFromLaser(float _amount)
    {
        if (!isInvincible_access)
        {
            animator.SetTrigger("HitTrigger");
            PlayerUI i_potentialPlayerUI = GetComponent<PlayerUI>();
            if (i_potentialPlayerUI != null)
            {
                i_potentialPlayerUI.DisplayHealth(HealthAnimationType.Loss);
            }
            base.DamageFromLaser(_amount);
        }
    }

    public override void Kill ()
	{
		if (moveState == MoveState.Dead) { return; }
		Analytics.CustomEvent("PlayerDeath", new Dictionary<string, object> { { "Zone", GameManager.GetCurrentZoneName() }, });
		dunkController.StopDunk();
		moveState = MoveState.Dead;
		animator.SetTrigger("Dead");
		DropBall();
		SetUntargetable();
		Freeze();
		DisableInput();
		StartCoroutine(HideAfterDelay(0.5f));
		StartCoroutine(ProjectEnemiesInRadiusAfterDelay(0.4f, deathExplosionRadius, deathExplosionForce, deathExplosionDamage, DamageSource.DeathExplosion));
		StartCoroutine(GenerateRevivePartsAfterDelay(0.4f));
		GameManager.deadPlayers.Add(this);
		if (GameManager.deadPlayers.Count > 1)
		{
			Analytics.CustomEvent("PlayerSimultaneousDeath", new Dictionary<string, object> { { "Zone", GameManager.GetCurrentZoneName() }, });
		}
		GameManager.alivePlayers.Remove(this);
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
		StartCoroutine(ProjectEnemiesInRadiusAfterDelay(0.4f, reviveExplosionRadius, reviveExplosionForce, reviveExplosionDamage, DamageSource.ReviveExplosion));
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
                BumpMe(10, 1, 0.4f, i_normalizedImpactVector, _bumpModificators.x, _bumpModificators.y, _bumpModificators.z);
				Damage(_damages);
				break;

            case DamageSource.EnemyContact:
                Damage(_damages);
                //Push(_impactVector, 30, 2f);
                break;

            case DamageSource.Laser:
                DamageFromLaser(_damages);
                break;

			case DamageSource.SpawnImpact:
				Damage(_damages);
				//Push(-_impactVector, _damages * 10f, 1f);
				break;
		}
	}

	public static PlayerController GetNearestPlayer(Vector3 _point)
	{
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

	public override void BumpMe ( float _bumpDistance, float _bumpDuration, float _restDuration, Vector3 _bumpDirection, float _randomDistanceMod, float _randomDurationMod, float _randomRestDurationMod )
	{
		base.BumpMe(_bumpDistance, _bumpDuration, _restDuration, _bumpDirection, _randomDistanceMod, _randomDurationMod, _randomRestDurationMod);
	}
}
