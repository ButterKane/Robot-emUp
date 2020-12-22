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
    //Key inputs;
    public static KeyCode leftKey = KeyCode.Q;
    public static KeyCode rightKey = KeyCode.D;
    public static KeyCode upKey = KeyCode.Z;
    public static KeyCode downKey = KeyCode.S;
    private static KeyCode dashKey = KeyCode.LeftShift;
    private static KeyCode dunkKey = KeyCode.Space;
    private static KeyCode grabKey = KeyCode.LeftControl;
    private static KeyCode highlightBall = KeyCode.H;


    [Space(2)]
    [Separator("Player settings")]
    public PlayerIndex playerIndex;
    public float triggerTreshold = 0.1f;
    public Color highlightedColor;
    public Color highlightedSecondColor;
    public bool forceInsideCameraView = false;
    [SerializeField] private bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
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
    [Range(0, 1)] public float partExplosionAngleRandomness = 0.1f;
    public Vector2 minMaxProjectionForce = new Vector2(9, 11);
    public float reviveHoldDuration = 3;
    public float reviveFreezeDuration = 1;

    private List<ReviveInformations> revivablePlayers = new List<ReviveInformations>(); //List of the players that can be revived

    //Grab values
    [HideInInspector] public List<GrabbableInformation> targetedGrabbable = new List<GrabbableInformation>();
    private GrabbableInformation prioritaryGrabInformation;

    //Input values
    private bool inputDisabled;
    private bool dashUsed;
    private bool rightTriggerWaitForRelease;
    private bool leftTriggerWaitForRelease;
    private bool leftShouldWaitForRelease;
    private bool rightButtonWaitForRelease;
    private float dashBuffer;
    private float timeSinceLastHeal;
    private float rbPressDuration;
    private bool reviving;
    public ControllerType controllerType;
    private bool pickingAbility;

    //Other
    private Coroutine freezeCoroutine;
    private Coroutine disableInputCoroutine;
    private bool canBeKilled = true;

    //References
    private DunkController dunkController;
    [HideInInspector] public DashController dashController;
    [HideInInspector] public ExtendingArmsController extendingArmsController;
    public static Transform middlePoint;
    private GamePadState state;
    private KeyBindingStruct bindings;
    private Camera cam;
    private PlayerUI ui;
    public Collider mainCollider;

    public enum ControllerType { Keyboard, Gamepad }
    public void Start()
    {
        base.Awake();
        if (!Application.isPlaying) { return; }

        if (playerIndex == PlayerIndex.One) { bindings = InputHandler.instance.bindingP1; }
        if (playerIndex == PlayerIndex.Two) { bindings = InputHandler.instance.bindingP2; }

        //References initialization
        mainCollider = GetComponent<Collider>();
        cam = Camera.main;
        dunkController = GetComponent<DunkController>();
        dashController = GetComponent<DashController>();
        extendingArmsController = GetComponent<ExtendingArmsController>();
        ui = GetComponent<PlayerUI>();
        animator.SetTrigger("BackOnGroundTrigger");

        //Set pass target
        passController.SetTargetedPawn(GetOtherPlayer());

        //Variable initialization
        GameManager.alivePlayers.Add(this);
        GenerateMiddlePoint();
    }
    private void Update()
    {
        if (LoadingScreen.loading) { return; }
        UpdateMiddlePoint();
        GetInput();
        UpdateOverHeal();
        if (playerIndex == PlayerIndex.One)
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                AbilityManager.UpgradeAbility(ConcernedAbility.PerfectReception, Upgrade.Upgrade1);
                //AbilityManager.UnlockAllAbilities();
            }
            if (Input.GetKeyDown(KeyCode.F2))
            {
                AbilityManager.ResetUpgrades();
            }
        }
    }
    protected override void LateUpdate()
    {
        base.LateUpdate();
        UpdateWhenOutOfCamera();
    }

    #region Public functions
    public static PlayerController GetNearestPlayer(Vector3 _point)
    {
        if (GameManager.alivePlayers.Count <= 0) { return null; }
        PlayerController i_nearestPlayer = GameManager.alivePlayers[0];
        float i_closestDistance = Vector3.Distance(i_nearestPlayer.transform.position, _point);
        foreach (PlayerController p in GameManager.alivePlayers)
        {
            float distance = Vector3.Distance(p.transform.position, _point);
            if (distance < i_closestDistance)
            {
                i_closestDistance = distance;
                i_nearestPlayer = p;
            }
        }
        return i_nearestPlayer;
    }

    public bool IsPickingAbility ()
    {
        return pickingAbility;
    }
    public static Transform GetMiddlePoint()
    {
        return middlePoint;
    } //Returns a transform of a point between both players
    public PlayerController GetOtherPlayer()
    {
        List<PlayerController> i_players = GameManager.players;
        foreach (PlayerController p in i_players)
        {
            if (p != this) { return p; }
        }
        return null;
    } //Returns the other player
    public void DisableInput()
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

    public bool IsInputDisabled()
    {
        return inputDisabled;
    }
    public void FreezeTemporarly(float _duration)
    {
        if (freezeCoroutine != null) { StopCoroutine(freezeCoroutine); }
        freezeCoroutine = StartCoroutine(FreezeTemporarly_C(_duration));
    }
    public void AddRevivablePlayer(ReviveInformations _player)
    {
        revivablePlayers.Add(_player);
    } //Indicate that the player can now revive someone
    public void Revive(PlayerController _player)
    {
        Analytics.CustomEvent("PlayerRevive", new Dictionary<string, object> { { "Zone", GameManager.GetCurrentZoneName() }, });
        FeedbackManager.SendFeedback(eventOnResurrecting, this);
        moveState = MoveState.Idle;
        _player.moveState = MoveState.Idle;
        _player.ui.DisplayHealth();
        _player.animator.SetTrigger("Revive");
        _player.SetTargetable();
        _player.UnHide();
        _player.currentHealth = GetMaxHealth();
        _player.transform.position = transform.position + Vector3.up * 7 + Vector3.left * 0.1f;
        _player.FreezeTemporarly(reviveFreezeDuration);
        _player.EnableInput();
        StartCoroutine(DisableInputsTemporarly_C(reviveFreezeDuration * 2));
        FreezeTemporarly(reviveFreezeDuration);
        SetTargetable();
        LinkController.i.EnableLink();
        List<ReviveInformations> i_newRevivablePlayers = new List<ReviveInformations>();
        StartCoroutine(ProjectEnemiesInRadiusAfterDelay_C(0.4f, reviveExplosionRadius, reviveExplosionDamage, DamageSource.ReviveExplosion));
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
        _player.canBeKilled = true;
    }
    public void KillWithoutCorePart()
    {
        canBeKilled = false;
        if (moveState == MoveState.Dead) { return; }
        SetUntargetable();
        ui.HideHealthBar(); 
        Analytics.CustomEvent("PlayerDeath", new Dictionary<string, object> { { "Zone", GameManager.GetCurrentZoneName() }, });
        dunkController.StopDunk();
        moveState = MoveState.Dead;
        animator.SetTrigger("Dead");
        passController.DropBall();
        Freeze();
        LinkController.i.DisableLink();
        DisableInput();
        StartCoroutine(HideAfterDelay_C(0.5f));
        StartCoroutine(ProjectEnemiesInRadiusAfterDelay_C(0.4f, reviveExplosionRadius, reviveExplosionDamage, DamageSource.DeathExplosion));
        GameManager.deadPlayers.Add(this);
        if (GameManager.deadPlayers.Count > 1)
        {
            Analytics.CustomEvent("PlayerSimultaneousDeath", new Dictionary<string, object> { { "Zone", GameManager.GetCurrentZoneName() }, });
        }
        GameManager.alivePlayers.Remove(this);
    }
    public void DisableInputTemporarly(float _delay)
    {
        if (disableInputCoroutine != null) { StopCoroutine(disableInputCoroutine); }
        disableInputCoroutine = StartCoroutine(DisableInputsTemporarly_C(_delay));
    }
    public void PushEveryPawn()
    {
        PawnController[] foundPawns = FindObjectsOfType<PawnController>();
        foreach (PawnController p in foundPawns)
        {
            //p.BumpMe(-p.transform.forward, BumpForce.Force2);
            p.Push(PushType.Heavy, -p.transform.forward, PushForce.Force2);
        }
    } //Debug function to push every pawn in scene
    public override void Kill()
    {
        if (canBeKilled)
        {
            KillWithoutCorePart();
            StartCoroutine(GenerateRevivePartsAfterDelay_C(0.4f));
        }
    }
    public override void Heal(int _amount)
    {
        timeSinceLastHeal = 0;
        if (currentHealth < GetMaxHealth())
        {
            base.Heal(_amount);
        }
        else
        {
            currentHealth += _amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, GetMaxHealth() + overHealValue);
        }
        if (ui != null)
        {
            ui.DisplayHealth(HealthAnimationType.Gain);
        }
    }
    public override void UpdateAnimatorBlendTree() //Called each frame by pawnController
    {
        base.UpdateAnimatorBlendTree();
        if (passController.IsAiming())
        {
            float angle = Vector3.SignedAngle(transform.forward, Vector3.forward, Vector3.up);
            Vector3 forwardVector = rb.velocity;
            forwardVector = Quaternion.Euler(0, angle, 0) * forwardVector;
            forwardVector = forwardVector / pawnMovementValues.moveSpeed;
            animator.SetFloat("ForwardBlend", forwardVector.z);
            animator.SetFloat("SideBlend", forwardVector.x);
        }
        else
        {
            animator.SetFloat("ForwardBlend", currentSpeed / pawnMovementValues.moveSpeed);
            animator.SetFloat("SideBlend", 0);
        }
    }
    public override void Damage(float _amount, bool _enableInvincibilityFrame = false)
    {
        if (!IsInvincible())
        {
            Analytics.CustomEvent("PlayerDamage", new Dictionary<string, object> { { "Zone", GameManager.GetCurrentZoneName() } });
            animator.SetTrigger("HitTrigger");
            if (ui != null)
            {
                ui.DisplayHealth(HealthAnimationType.Loss);
            }
            base.Damage(_amount * (PlayerPrefs.GetFloat("REU_GameSpeed", GameManager.i.damageTakenSettingsMod)/100), _enableInvincibilityFrame);   // manages the recovery time as well
        }
    }
    #endregion

    #region Private functions

    private void UpdateControllerType()
    {
        int connectedGamepadAmount = GamepadChecker.instance.GetConnectedGamepadAmount();
        if (connectedGamepadAmount <= 0)
        {
            //Display screen "connect gamepad"
            controllerType = ControllerType.Keyboard;
        } else if (connectedGamepadAmount == 1)
        {
            if (playerIndex == PlayerIndex.One)
            {
                controllerType = ControllerType.Gamepad;
            } else
            {
                controllerType = ControllerType.Keyboard;
            }
        } else
        {
            controllerType = ControllerType.Gamepad;
        }
    }
    private void GetInput()
    {
        UpdateControllerType();
        if (!Application.isPlaying || inputDisabled) { return; }
        state = GamePad.GetState(playerIndex);
        GetMoveAndLookInput(out moveInput, out lookInput);
        CheckAim();
        CheckPass();
        CheckDunk();
        CheckClimb();
        CheckDash();
        CheckHighlight();
        CheckRevive();
        CheckGrapple();
        CheckPickAbility();
    }

    private void CheckPickAbility()
    {
        switch (controllerType)
        {
            case ControllerType.Gamepad:
                if (state.Buttons.A == ButtonState.Pressed) pickingAbility = true; else pickingAbility = false;
                break;
            case ControllerType.Keyboard:
                if (Input.GetMouseButton(1)) pickingAbility = true; else pickingAbility = false;
                break;
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
            col.radius = 3f;
            col.isTrigger = true;
            GameObject cinematicCam = Instantiate(Resources.Load<GameObject>("Cinematic/CinematicCamera")).gameObject;
            cinematicCam.transform.SetParent(middlePoint);
            cinematicCam.transform.localPosition = Vector3.zero;
            cinematicCam.transform.localRotation = Quaternion.identity;
        }
    }
    private void UpdateMiddlePoint() //Only the player one can update the position of the middle point
    {
        if (middlePoint != null && playerIndex == PlayerIndex.One)
        {
            middlePoint.transform.position = GameManager.playerOne.transform.position + ((GameManager.playerTwo.transform.position - GameManager.playerOne.transform.position) / 2f);
        }
    }
    private void UpdateWhenOutOfCamera()
    {
        if (GameManager.timeInZone < 1.5f || !Application.isPlaying || !forceInsideCameraView) { return; }
        Vector3 i_viewPortPosition = GameManager.mainCamera.WorldToViewportPoint(transform.position);
        float extents = GameManager.cameraGlobalSettings.outOfCameraMaxDistancePercentage;
        if (i_viewPortPosition.x > 1 + extents || i_viewPortPosition.x < -extents || i_viewPortPosition.y > 1 + extents || i_viewPortPosition.y < -extents)
        {
            rb.velocity = Vector3.zero;
            Vector3 i_centerPos = GameManager.mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Vector3.Distance(transform.position, GameManager.mainCamera.transform.position)));
            i_centerPos.y = transform.position.y;
            Vector3 i_direction = i_centerPos - transform.position;
            i_direction = i_direction.normalized;
            float i_intensity = Vector3.Distance(i_centerPos, transform.position);
            rb.AddForce(i_direction * i_intensity, ForceMode.Impulse);
        }
    }
    private void CheckClimb()
    {
        if (moveInput.magnitude > 0.5f)
        {
            Climb();
        }
    }

    private void GetMoveAndLookInput(out Vector3 _moveInput, out Vector3 _lookInput)
    {
        _lookInput = lookInput;
        _moveInput = moveInput;
        if (cam != null)
        {
            Vector3 i_camForwardNormalized = cam.transform.forward;
            i_camForwardNormalized.y = 0;
            i_camForwardNormalized = i_camForwardNormalized.normalized;
            Vector3 i_camRightNormalized = cam.transform.right;
            i_camRightNormalized.y = 0;
            i_camRightNormalized = i_camRightNormalized.normalized;
            if ((currentPawnState != null && !currentPawnState.preventMoving) || currentPawnState == null)
            {
                switch (controllerType)
                {
                    case ControllerType.Gamepad:
                        _moveInput = (state.ThumbSticks.Left.X * i_camRightNormalized) + (state.ThumbSticks.Left.Y * i_camForwardNormalized);
                        _moveInput.y = 0;
                        _moveInput = _moveInput.normalized * ((_moveInput.magnitude - pawnMovementValues.deadzone) / (1 - pawnMovementValues.deadzone));
                        _lookInput = (state.ThumbSticks.Right.X * i_camRightNormalized) + (state.ThumbSticks.Right.Y * i_camForwardNormalized);
                        break;
                    case ControllerType.Keyboard:
                        int horizontalInput = 0;
                        if (Input.GetKey(leftKey)) horizontalInput--;
                        if (Input.GetKey(rightKey)) horizontalInput++;
                        int verticalInput = 0;
                        if (Input.GetKey(upKey)) verticalInput++;
                        if (Input.GetKey(downKey)) verticalInput--;
                        _moveInput = horizontalInput * i_camRightNormalized + verticalInput * i_camForwardNormalized;
                        _moveInput.y = 0;
                        _moveInput = _moveInput.normalized * ((_moveInput.magnitude - pawnMovementValues.deadzone) / (1 - pawnMovementValues.deadzone));
                        _lookInput = GetMouseLookedDirection();
                        break;
                }
            }
            else
            {
                _moveInput = Vector3.zero;
            }
        }
    }

    private Vector3 GetMouseLookedDirection()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Vector3.Distance(transform.position, cam.transform.position);
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(mousePos);
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Environment")))
        {
            mouseWorldPos = hit.point;
        }
        mouseWorldPos.y = transform.position.y;
        Vector3 mouseDirection = mouseWorldPos - transform.position;
        mouseDirection = Vector3.ClampMagnitude(mouseDirection / 13f, 1f); //mouse direction is divided by 1/sensivity
        return mouseDirection;
    }
    private void CheckRevive()
    {
        bool canRevive = false;
        switch (controllerType)
        {
            case ControllerType.Gamepad:
                if (state.Triggers.Right > triggerTreshold && state.Triggers.Left > triggerTreshold) canRevive = true;
                break;
            case ControllerType.Keyboard:
                if (Input.GetMouseButton(0)) canRevive = true;
                break;
        }
        if (revivablePlayers.Count > 0)
        {
            if (canRevive)
            {
                animator.SetBool("Reviving", true);
                reviving = true;
                AddSpeedModifier(new SpeedCoef(reviveSpeedCoef, Time.deltaTime, SpeedMultiplierReason.Reviving, false));
                foreach (ReviveInformations p in revivablePlayers)
                {
                    p.linkedPanel.FillAssemblingSlider();
                }
            }
            else
            {
                reviving = false;
                animator.SetBool("Reviving", false);
                UnFreeze();
                SetTargetable();
            }
        }
        else
        {
            if (reviving && !canRevive)
            {
                reviving = false;
                animator.SetBool("Reviving", false);
            }
        }
    }

    // New Input System methods
    private void CheckDunk()
    {
        bool canDunk = false;
        switch (controllerType)
        {
            case ControllerType.Keyboard:
                if (Input.GetKeyDown(dunkKey) && revivablePlayers.Count <= 0) canDunk = true;
                break;
            case ControllerType.Gamepad:
                if (state.Buttons.Y == ButtonState.Pressed && revivablePlayers.Count <= 0) canDunk = true;
                break;
        }
        if (canDunk) dunkController.Dunk();
    }
    private void CheckPass()
    {
        bool canPass = false;
        switch (controllerType)
        {
            case ControllerType.Keyboard:
                if (Input.GetMouseButton(0) && revivablePlayers.Count <= 0) canPass = true;
                break;
            case ControllerType.Gamepad:
                if (state.Triggers.Right > PlayerPrefs.GetFloat("REU_Trigger_Treshold", triggerTreshold) && revivablePlayers.Count <= 0) canPass = true;
                break;
        }
        if (canPass)
        {
            if (revivablePlayers.Count <= 0)
            {
                if (!rightTriggerWaitForRelease) { passController.TryReception(); passController.Shoot(); }
                rightTriggerWaitForRelease = true;
            }
        } else
        {
            rightTriggerWaitForRelease = false;
        }
    }
    private void CheckAim ()
    {
        bool canAim = false;
        switch (controllerType)
        {
            case ControllerType.Keyboard:
                if (Input.GetMouseButton(1)) canAim = true;
                break;
            case ControllerType.Gamepad:
                if (lookInput.magnitude > PlayerPrefs.GetFloat("REU_Trigger_Treshold", triggerTreshold * 100) / 100) canAim = true;
                break;
        }
        if (canAim)
        {
            passController.SetLookDirection(lookInput);
            if (!rightButtonWaitForRelease)
            {
                passController.Aim();
            }
        } else
        {
            passController.StopAim();
        }
    }
    private void CheckDash()
    {
        bool canDash = false;
        switch (controllerType)
        {
            case ControllerType.Gamepad:
                if (state.Triggers.Left > PlayerPrefs.GetFloat("REU_Trigger_Treshold", triggerTreshold)) canDash = true;
                break;
            case ControllerType.Keyboard:
                if (Input.GetKey(dashKey)) canDash = true;
                break;
        }
        if (canDash)
        {
            leftTriggerWaitForRelease = true;
            if (dashUsed == false && !reviving)
            {
                if (revivablePlayers.Count <= 0)
                {
                    Vector3 dashDirection = moveInput;
                    if (moveInput.magnitude <= 0)
                    {
                        dashDirection = transform.forward;
                    }
                    dashController.Dash(dashDirection);
                    dashUsed = true;
                }
                else if (!rightTriggerWaitForRelease)
                {
                    dashBuffer += Time.deltaTime;
                    if (dashBuffer >= delayBeforeDash)
                    {
                        Vector3 dashDirection = moveInput;
                        if (moveInput.magnitude <= 0)
                        {
                            dashDirection = transform.forward;
                        }
                        dashController.Dash(dashDirection);
                        dashUsed = true;
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
                dashUsed = false;
                leftTriggerWaitForRelease = false;
            }
        }
    }
    private void CheckGrapple()
    {
        bool canGrapple = false;
        switch (controllerType)
        {
            case ControllerType.Gamepad:
                if (state.Buttons.RightShoulder == ButtonState.Pressed) canGrapple = true;
                break;
            case ControllerType.Keyboard:
                if (Input.GetKey(grabKey)) canGrapple = true;
                break;
        }
        if (canGrapple)
        {
            rightButtonWaitForRelease = true;
            rbPressDuration += Time.deltaTime;
            if (extendingArmsController != null && targetedGrabbable.Count > 0)
            {
                float i_pressPercent = rbPressDuration / extendingArmsController.minGrabHoldDuration;
                i_pressPercent = Mathf.Clamp(i_pressPercent, 0f, 1f);
                extendingArmsController.UpdateDecalSize(i_pressPercent);

                prioritaryGrabInformation = GetPrioritaryGrabInformation();
                if (prioritaryGrabInformation != null)
                {
                    passController.StopAim();
                    extendingArmsController.TogglePreview(true);
                    ForceLookAt(prioritaryGrabInformation.targetedPosition.position); //Player will rotate toward look input
                }
            }
            else if (extendingArmsController != null)
            {
                extendingArmsController.TogglePreview(false);
            }
        }
        else if (state.Buttons.RightShoulder == ButtonState.Released && rightButtonWaitForRelease)
        {
            rightButtonWaitForRelease = false;
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
    }
    private void CheckHighlight()
    {
        bool canHighlight = false;
        switch (controllerType)
        {
            case ControllerType.Keyboard:
                if (Input.GetKey(highlightBall) && !leftShouldWaitForRelease) canHighlight = true;
                break;
            case ControllerType.Gamepad:
                if (state.Buttons.LeftShoulder == ButtonState.Pressed && !leftShouldWaitForRelease) canHighlight = true;
                break;
        }
        if (canHighlight)
        {
             Highlighter.HighlightBall();
            leftShouldWaitForRelease = true;
        } 
        else
        {
            leftShouldWaitForRelease = false;
        }
    }

    private void UpdateOverHeal()
    {
        if (currentHealth > GetMaxHealth())
        {
            timeSinceLastHeal += Time.deltaTime;
            if (timeSinceLastHeal >= delayBeforeOverhealDecay)
            {
                float i_lerpValue = (currentHealth - GetMaxHealth()) / overHealValue;
                currentHealth -= Time.deltaTime * (overHealDecaySpeedCurve.Evaluate(i_lerpValue) / overHealDecaySpeed);
            }
        }
    }
    private void GenerateReviveParts()
    {
        float i_currentAngle = 0;
        float i_defaultAngleDifference = 360f / revivePartsCount;
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
    private GrabbableInformation GetPrioritaryGrabInformation()
    {
        GrabbableInformation i_prioritaryInformation = targetedGrabbable[0];
        Vector3 i_prioritaryDirection = i_prioritaryInformation.targetedPosition.position - GetCenterPosition();
        i_prioritaryDirection.y = 0;
        float i_prioritaryAngle = Vector3.SignedAngle(lookInput, i_prioritaryDirection, Vector3.up);
        foreach (GrabbableInformation gi in targetedGrabbable)
        {
            Vector3 i_giDirection = gi.targetedPosition.position - GetCenterPosition();
            i_giDirection.y = 0;
            float i_newAngle = Vector3.SignedAngle(lookInput, i_giDirection, Vector3.up);
            if (Mathf.Abs(i_newAngle) < Mathf.Abs(i_prioritaryAngle))
            {
                i_prioritaryInformation = gi;
                i_prioritaryAngle = i_newAngle;
            }
        }
        return i_prioritaryInformation;
    }
    #endregion

    #region Coroutines
    IEnumerator HideAfterDelay_C(float _delay)
    {
        yield return new WaitForSeconds(_delay);
        Hide();
    }
    IEnumerator ProjectEnemiesInRadiusAfterDelay_C(float _delay, float _radius, int _damages, DamageSource _damageSource)
    {
        if (_delay > 0) { yield return new WaitForSeconds(_delay); }
        Collider[] foundColliders = Physics.OverlapSphere(transform.position, _radius);
        foreach (Collider hit in foundColliders)
        {
            IHitable potentialHitableObject = hit.transform.GetComponent<IHitable>();
            if (potentialHitableObject != null) { potentialHitableObject.OnHit(null, (hit.transform.position - transform.position).normalized, this, _damages, _damageSource); }
        }
    }
    IEnumerator GenerateRevivePartsAfterDelay_C(float _delay)
    {
        if (_delay > 0) { yield return new WaitForSeconds(_delay); }
        GenerateReviveParts();
    }
    IEnumerator DisableInputsTemporarly_C(float _duration)
    {
        for (float i = 0; i < _duration; i += Time.deltaTime)
        {
            if (!inputDisabled)
            {
                DisableInput();
            }
            yield return null;
        }
        EnableInput();
    }
    IEnumerator FreezeTemporarly_C(float _duration)
    {
        for (float i = 0; i < _duration; i += Time.deltaTime)
        {
            if (!frozen)
            {
                Freeze();
            }
            yield return null;
        }
        UnFreeze();
    }
    #endregion

    #region Interface implementation
    void IHitable.OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, float _damages, DamageSource _source, Vector3 _bumpModificators)
    {
        Analytics.CustomEvent("PlayerDamage", new Dictionary<string, object> { { "Source", _source } });
        Vector3 i_normalizedImpactVector = new Vector3(_impactVector.x, 0, _impactVector.z);
        
        switch (_source)
        {
            case DamageSource.RedBarrelExplosion:
                BumpMe(i_normalizedImpactVector, BumpForce.Force2);
                Damage(_damages);
                break;

            case DamageSource.EnemyContact:
                Damage(_damages);
                Push(PushType.Light, _impactVector, PushForce.Force1);
                break;

            case DamageSource.Laser:
                Damage(_damages, false);
                break;

            case DamageSource.SpawnImpact:
                Damage(_damages);
                Push(PushType.Light, _impactVector, PushForce.Force1);
                break;
        }
    }
    #endregion
}
