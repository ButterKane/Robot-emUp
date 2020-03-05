using MyBox;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BossBehaviour : MonoBehaviour, IHitable
{
	public enum BossPhase { PhaseOne, PhaseTwo }

	[Header("Global Settings")]
	public float topPartRotationSpeed;
	public float firstPhaseHP;
	public float secondPhaseHP;
	public float healthBarHeight;
	public float minAttackRange = 15f;
	public float maxAttackAngle = 5f;
	public List<BossLeg> legs;
	public Transform zoneCenter;
	public BossMode startingMode;
	public BossTileGenerator tileGenerator;

	[Header("BulletHell settings")]
	public float bulletStormRotationSpeed;
	public float minDelayBetweenBullets;
	public float maxDelayBetweenBullets;
	public float bulletStormCanonMaxAngle;
	public float bulletStormCanonRotationSpeed;

	[Header("Electrical plates settings")]
	public float groundAttackPreparationDuration;

	[Header("WeakPoint settings")]
	public float legMaxHP = 50;

	[Header("Punch settings")]
	public float punchDistance = 2f;
	public float punchCooldown;
	public float punchChargeDuration = 2f;
	public AnimationCurve punchChargeSpeedCurve;
	public float punchDamages = 5f;
	public float punchPushForce = 10f;
	public float punchPushHeight = 3f;
	public float punchSize = 2f;

	[Header("Hammer settings")]
	public float hammerCooldown = 5f;
	public float hammerChargeDuration = 2f;
	public AnimationCurve hammerChargeSpeedCurve;

	[Header("Tea bag settings")]
	public float damageRadius;
	public float triggerRadius;
	public float bumpRadius;
	public float teabagSize;
	public float maxTimeBeforeTeaBag = 5f;
	public float teaBagPreviewDuration = 2f;
	public float teaBagDamages;
	public float teaBagPushForce;

	[Header("Range mode settings")]
	public float shoulderRotationSpeed;
	public AnimationCurve turretDetachCurve;

	[Header("References")]
	public Transform topPart;
	public Transform shoulderLeft;
	public Transform shoulderRight;
	public List<Spawner> minionSpawners;
	public EnemyData enemyToSpawn;

	[Header("Informations")]
	[ReadOnly] public BossPhase currentPhase;
	[ReadOnly] public BossMode currentMode;

	//HealthBars Refs
	private Transform healthBar;
	private Image healthBar1FillInstant;
	private Image healthBar1FillLerped;
	private Image healthBar2FillInstant;
	private Image healthBar2FillLerped;

	//Health values
	private float firstPhaseCurrentHealth;
	private float secondPhaseCurrentHealth;

	//Cooldown values
	private float punchCurrentCD;
	private float hammerCurrentCD;

	//Other values
	private float timeSinceLastModeChange;
	private bool weakPointsActivated;
	private List<EnemyBehaviour> minions;

	private NavMeshAgent navMesh;
	private List<Quaternion> shoulderInitialRotation;
	private bool shoulderRotationEnabled;
	private bool bulletStormEnabled;
	private Transform currentTarget;
	private bool frozen;
	private float timeBeforeTeaBag;
	private int weakPointsActivatedCount;
	private bool hitByDunk;
	private GameObject bossExplosionFX;
	private bool destroyed;
	private bool groundAttackActivated;
	[HideInInspector] public Animator animator;
	[HideInInspector] public List<BossTile> tiles;

	[SerializeField] private bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
	[SerializeField] private float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }

	private void Start ()
	{
		GetReferences();
		GenerateHealthBars();
		InitializeValues();
		ChangeMode(startingMode);
		tiles = tileGenerator.GenerateTiles();
	}
	private void Update ()
	{
		UpdateMovement();
		CheckForModeTransition();
		UpdateHealthBars();
		UpdateCooldowns();
		UpdateShoulderRotation();
		UpdateStormBulletMode();
		UpdateTeaBag();
	}


	//Public functions
	public void Damage(float _amount)
	{
		switch (currentPhase)
		{
			case BossPhase.PhaseOne:
				firstPhaseCurrentHealth -= _amount;
				firstPhaseCurrentHealth = Mathf.Clamp(firstPhaseCurrentHealth, 0, firstPhaseHP);
				break;
			case BossPhase.PhaseTwo:
				secondPhaseCurrentHealth -= _amount;
				secondPhaseCurrentHealth = Mathf.Clamp(secondPhaseCurrentHealth, 0, secondPhaseHP);
				break;
		}
	}

	public void ToggleInvincibility(bool _state)
	{

	}

	public void ActivateWeakPoint()
	{
		weakPointsActivatedCount++;
		if (weakPointsActivatedCount >= 4)
		{
			weakPointsActivated = true;
		}
	}

	public void Reconstruct ()
	{
		StartCoroutine(Reconstruct_C());
	}

	public void EnableTurretsInstantly ()
	{
		EnableTurrets(-1);
	}
	public void EnableTurrets(float _spawnSpeedOverride = -1)
	{
		shoulderRotationEnabled = true;
		foreach (Spawner s in minionSpawners)
		{
			minions.Add(s.SpawnEnemy(enemyToSpawn, false, _spawnSpeedOverride));
		}
	}

	public void DisableTurrets ()
	{
		shoulderRotationEnabled = false;
		for (int i = 0; i < minions.Count; i++)
		{
			Debug.Log(minions);
			Debug.Log(minions[i]);
			Debug.Log(minions[i].transform.parent);
			if (minions != null && minions[i] != null && minions[i].transform.parent != null && minionSpawners[i] != null)
			{
				minionSpawners[i].RetractEnemy(minions[i]);
			}
		}
		minions.Clear();
	}

	public Vector3 GetGroundPosition()
	{
		RaycastHit hit;
		if (Physics.Raycast(transform.position, Vector3.down, out hit, 20f, LayerMask.GetMask("Environment")))
		{
			return hit.point;
		}
		return transform.position;
	}
	public void ChangeMode(BossMode _newMode)
	{
		Debug.Log("Changing to mode: " + _newMode);
		timeSinceLastModeChange = 0;
		if (currentMode != null)
		{
			foreach (string s in currentMode.actionsOnEnd)
			{
				InvokeMethod(s);
			}
		}
		currentMode = _newMode;
		foreach (string s in currentMode.actionsOnStart)
		{
			InvokeMethod(s);
		}
	}

	public void EnablePhaseTwo()
	{
		ChangePhase(BossPhase.PhaseTwo);
	}
	public void ChangePhase(BossPhase _newPhase)
	{
		if (currentPhase == _newPhase) { return; }
		currentPhase = _newPhase;
	}

	public void Punch()
	{
		if (punchCurrentCD <= 0)
		{
			punchCurrentCD = punchCooldown;
			GameObject punchObj = Instantiate(Resources.Load<GameObject>("EnemyResource/BossResource/BossPunch"));
			Vector3 newPunchPosition = transform.position + (topPart.forward * punchDistance);
			RaycastHit hit;
			if (Physics.Raycast(punchObj.transform.position, Vector3.down, out hit, 20f, LayerMask.GetMask("Environment")))
			{
				newPunchPosition.y = hit.point.y;
			}
			punchObj.transform.position = newPunchPosition;
			Vector3 punchForwardFlat = currentTarget.transform.position - transform.position;
			punchForwardFlat.y = 0;
			punchObj.transform.rotation = Quaternion.LookRotation(punchForwardFlat);
			punchObj.transform.localScale = Vector3.one * punchSize;
			BossPunch punchScript = punchObj.GetComponent<BossPunch>();
			punchScript.punchChargeDuration = punchChargeDuration;
			punchScript.punchChargeSpeedCurve = punchChargeSpeedCurve;
			punchScript.punchDamages = punchDamages;
			punchScript.punchPushForce = punchPushForce;
			punchScript.punchPushHeight = punchPushHeight;
			punchScript.StartPunch();
			Freeze(punchChargeDuration);
		}
	}
	
	public void StartBulletStorm ()
	{
		bulletStormEnabled = true;
		foreach (BossLeg leg in legs)
		{
			leg.canonMaxAngle = bulletStormCanonMaxAngle;
			leg.canonRotationSpeed = bulletStormCanonRotationSpeed;
			leg.maxDelayBetweenBullets = maxDelayBetweenBullets;
			leg.minDelayBetweenBullets = minDelayBetweenBullets;
			leg.EnableCanon();

			leg.maxHP = legMaxHP;
			leg.SetDestructible();
		}
	}

	public void StopBulletStorm ()
	{
		bulletStormEnabled = false;
		foreach (BossLeg leg in legs)
		{
			leg.DisableCanon();
		}
	}

	public void DetachTurrets()
	{
		StartCoroutine(DetachTurrets_C());
	}

	public void HammerPunch()
	{
		if (hammerCurrentCD <= 0)
		{
			Vector3 playerPosition = currentTarget.transform.position;
			Collider[] enviroNearPlayer = Physics.OverlapSphere(playerPosition, 5f, LayerMask.GetMask("Environment"));
			Transform nearestTile = null;
			float nearestDistance = 5f;
			for (int i = 0; i < enviroNearPlayer.Length; i++)
			{
				if (enviroNearPlayer[i].gameObject.tag == "Boss_Tile")
				{
					if (Vector3.Distance(enviroNearPlayer[i].transform.position, playerPosition) <= nearestDistance)
					{
						nearestTile = enviroNearPlayer[i].transform;
						nearestDistance = Vector3.Distance(enviroNearPlayer[i].transform.position, playerPosition);
					}
				}
			}
			if (nearestTile == null) { return; }
			hammerCurrentCD = hammerCooldown;
			GameObject hammerObj = Instantiate(Resources.Load<GameObject>("EnemyResource/BossResource/BossHammer"));
			Vector3 newHammerPosition = nearestTile.transform.position + Vector3.up * 1f;
			RaycastHit hit;
			if (Physics.Raycast(hammerObj.transform.position, Vector3.down, out hit, 100f, LayerMask.GetMask("Environment")))
			{
				newHammerPosition.y = hit.point.y;
				Debug.Log(newHammerPosition.y);
			}
			else
			{
				Debug.Log("No hit");
			}
			hammerObj.transform.position = newHammerPosition;
			Vector3 hammerForwardFlat = currentTarget.transform.position - transform.position;
			hammerForwardFlat.y = 0;
			hammerObj.transform.rotation = Quaternion.identity;
			hammerObj.transform.localScale = Vector3.one * 10f;
			BossPunch hammerScript = hammerObj.GetComponent<BossPunch>();
			hammerScript.punchChargeDuration = hammerChargeDuration;
			hammerScript.punchChargeSpeedCurve = hammerChargeSpeedCurve;
			hammerScript.StartPunch();
			Freeze(hammerChargeDuration);
		}
	}

	public void LaserAttack()
	{

	}

	public void GroundAttack()
	{
		if (!groundAttackActivated)
		{
			groundAttackActivated = true;
			StartCoroutine(GroundAttack_C());
		}
	}

	public void Kill()
	{
		animator.SetTrigger("Death");
		FeedbackManager.SendFeedback("event.BossDeath", this);
	}
	public void FallOnGround ()
	{
		destroyed = true;
		foreach (BossLeg leg in legs)
		{
			leg.RemoveExplosionFX();
		}
		bossExplosionFX = FeedbackManager.SendFeedback("event.BossFallOnGround", this).GetVFX();
		EnergyManager.IncreaseEnergy(1f);
		StartCoroutine(Stagger_C());
	}
	public void Teabag()
	{
		if (destroyed) { return; }
		GameObject teabagObj = Instantiate(Resources.Load<GameObject>("EnemyResource/BossResource/BossTeabag"));
		teabagObj.transform.position = transform.position;
		RaycastHit hit;
		if (Physics.Raycast(transform.position, Vector3.down, out hit, 10, LayerMask.GetMask("Environment")))
		{
			teabagObj.transform.position = hit.point + Vector3.up * 0.1f;
		}
		BossTeaBag teabagScript = teabagObj.GetComponent<BossTeaBag>();
		teabagScript.Init(this, teaBagPreviewDuration, teaBagPushForce, teaBagDamages, bumpRadius, damageRadius);
	}


	//Private functions

	private void InitializeValues()
	{
		firstPhaseCurrentHealth = firstPhaseHP;
		secondPhaseCurrentHealth = secondPhaseHP;
		minions = new List<EnemyBehaviour>();
		shoulderInitialRotation = new List<Quaternion>();
		shoulderInitialRotation.Add(shoulderLeft.transform.localRotation);
		shoulderInitialRotation.Add(shoulderRight.transform.localRotation);
		tiles = new List<BossTile>();
	}

	public void Freeze(float _duration)
	{
		StartCoroutine(Freeze_C(_duration));
	}

	private void GetReferences()
	{
		navMesh = GetComponent<NavMeshAgent>();
		animator = GetComponent<Animator>();
	}
	private void GenerateHealthBars ()
	{
		healthBar = Instantiate(Resources.Load<GameObject>("EnemyResource/BossResource/BossHealthBar")).transform;
		healthBar.transform.SetParent(GameManager.mainCanvas.transform);
		healthBar1FillInstant = healthBar.transform.Find("PhaseOne").transform.Find("HealthBarFillInstant").GetComponent<Image>();
		healthBar1FillLerped = healthBar.transform.Find("PhaseOne").transform.Find("HealthBarFillLerped").GetComponent<Image>();
		healthBar2FillInstant = healthBar.transform.Find("PhaseTwo").transform.Find("HealthBarFillInstant").GetComponent<Image>();
		healthBar2FillLerped = healthBar.transform.Find("PhaseTwo").transform.Find("HealthBarFillLerped").GetComponent<Image>();

		healthBar1FillInstant.fillAmount = 1;
		healthBar1FillLerped.fillAmount = 1;
		healthBar2FillInstant.fillAmount = 1;
		healthBar2FillLerped.fillAmount = 1;

	}
	private void UpdateMovement()
	{
		if (frozen)
		{
			navMesh.enabled = false;
			return;
		} else
		{
			navMesh.enabled = true;
		}
		Vector3 destination = Vector3.zero;
		switch (currentMode.movementType)
		{
			case BossMovementType.DontMove:
				break;
			case BossMovementType.FollowNearestPlayer:
				currentTarget = PlayerController.GetNearestPlayer(transform.position).transform;
				destination = currentTarget.transform.position;
				break;
			case BossMovementType.GoToCenter:
				destination = zoneCenter.position;
				break;
		}
		if (destination != Vector3.zero)
		{
			navMesh.SetDestination(destination);
			Vector3 destinationFlatted = destination;
			destinationFlatted.y = 0;
			Vector3 positionFlatted = transform.position;
			positionFlatted.y = 0;
			if (Vector3.Distance(destinationFlatted, positionFlatted) < minAttackRange)
			{
				if (currentTarget != null)
				{
					Vector3 targetPositionFlat = currentTarget.position;
					targetPositionFlat.y = topPart.position.y;
					if (Vector3.Angle(topPart.forward, targetPositionFlat - topPart.transform.position) < maxAttackAngle)
					{
						foreach (string s in currentMode.actionsOnMovementEnd)
						{
							InvokeMethod(s);
						}
					}
				} else
				{
					foreach (string s in currentMode.actionsOnMovementEnd)
					{
						InvokeMethod(s);
					}
				}
			}
		}
		bool enableRotation = true;
		Vector3 lookPosition = transform.position + transform.forward;
		switch (currentMode.rotationType)
		{
			case BossRotationType.LookCenter:
				lookPosition = zoneCenter.position;
				break;
			case BossRotationType.LookNearestPlayer:
				lookPosition = PlayerController.GetNearestPlayer(transform.position).transform.position;
				break;
			case BossRotationType.None:
				enableRotation = false;
				break;
		}
		if (enableRotation)
		{
			lookPosition.y = topPart.transform.position.y;
			Quaternion wantedRotation = Quaternion.LookRotation(lookPosition - topPart.transform.position);
			topPart.transform.rotation = Quaternion.Lerp(topPart.transform.rotation, wantedRotation, Time.deltaTime * topPartRotationSpeed);
		}
	}
	private void UpdateHealthBars()
	{
		healthBar.transform.position = GameManager.mainCamera.WorldToScreenPoint(transform.position) + new Vector3(0, healthBarHeight,0);
		healthBar1FillInstant.fillAmount = firstPhaseCurrentHealth / firstPhaseHP;
		healthBar1FillLerped.fillAmount = Mathf.Lerp(healthBar1FillLerped.fillAmount, firstPhaseCurrentHealth / firstPhaseHP, Time.deltaTime);
		healthBar2FillInstant.fillAmount = secondPhaseCurrentHealth / secondPhaseHP;
		healthBar2FillLerped.fillAmount = Mathf.Lerp(healthBar2FillInstant.fillAmount, secondPhaseCurrentHealth / secondPhaseHP, Time.deltaTime) ;
	}

	private void UpdateCooldowns()
	{
		timeSinceLastModeChange += Time.deltaTime;
		punchCurrentCD -= Time.deltaTime;
		hammerCurrentCD -= Time.deltaTime;
	}

	private void UpdateStormBulletMode ()
	{
		if (bulletStormEnabled)
		{
			transform.Rotate(Vector3.up, Time.deltaTime * bulletStormRotationSpeed);
		}
	}
	private void CheckForModeTransition()
	{
		if (currentMode == null) { return; }
		foreach (ModeTransition mt in currentMode.modeTransitions)
		{
			bool canActivateMode = true;
			foreach (ModeTransitionCondition mtc in mt.transitionConditions) {
				if (!IsTransitionConditionValid(mtc))
				{
					canActivateMode = false;
				}
			}
			if (canActivateMode)
			{
				ChangeMode(mt.modeToActivate);
			}
		}
	}

	private void SpawnElectricalPlates()
	{
		for (int i = 0; i < tiles.Count; i++)
		{
			if (i % 2 != 0)
			{
				tiles[i].SpawnElectricalPlate();
			}
		}
	}

	private float GetCurrentHP()
	{
		float currentHP = 0;
		switch (currentPhase)
		{
			case BossPhase.PhaseOne:
				currentHP = firstPhaseCurrentHealth;
				break;
			case BossPhase.PhaseTwo:
				currentHP = secondPhaseCurrentHealth;
				break;
		}
		return currentHP;
	}

	private void UpdateShoulderRotation()
	{
		if (shoulderRotationEnabled && minions != null && minions.Count > 1)
		{
			Vector3 leftShoulderLookDirection = shoulderLeft.transform.position - minions[0].focusedPlayer.transform.position;
			shoulderLeft.transform.rotation = Quaternion.Lerp(shoulderLeft.transform.rotation, Quaternion.LookRotation(leftShoulderLookDirection), shoulderRotationSpeed);
			Vector3 rightShoulderLookDirection = shoulderRight.transform.position - minions[1].focusedPlayer.transform.position;
			shoulderRight.transform.rotation = Quaternion.Lerp(shoulderRight.transform.rotation, Quaternion.LookRotation(rightShoulderLookDirection), shoulderRotationSpeed);
		} else
		{
			shoulderLeft.transform.localRotation = Quaternion.Lerp(shoulderLeft.transform.localRotation, shoulderInitialRotation[0], shoulderRotationSpeed);
			shoulderRight.transform.localRotation = Quaternion.Lerp(shoulderRight.transform.localRotation, shoulderInitialRotation[1], shoulderRotationSpeed);
		}
	}

	private void UpdateTeaBag()
	{
		bool playerTooClose = false;
		foreach (PlayerController p in GameManager.alivePlayers)
		{
			float playerDistance = Vector3.Distance(GetGroundPosition(), p.transform.position);
			if (playerDistance < triggerRadius)
			{
				playerTooClose = true;
			}
		}
		if (playerTooClose)
		{
			timeBeforeTeaBag += Time.deltaTime;
			if (timeBeforeTeaBag >= maxTimeBeforeTeaBag)
			{
				Teabag();
				timeBeforeTeaBag = 0f;
			}
		} else
		{
			timeBeforeTeaBag -= Time.deltaTime;
		}
		timeBeforeTeaBag = Mathf.Clamp(timeBeforeTeaBag, 0, maxTimeBeforeTeaBag);
	}

	private bool IsTransitionConditionValid(ModeTransitionCondition _mtc)
	{
		float value = _mtc.modeTransitionConditionValue;
		bool isValid = false;
		switch (_mtc.modeTransitionConditionType)
		{
			case ModeTransitionConditionType.SecondPhaseEnabled:
				if (currentPhase == BossPhase.PhaseTwo) { isValid = true; }
				break;
			case ModeTransitionConditionType.FirstPhaseEnabled:
				if (currentPhase == BossPhase.PhaseOne) { isValid = true; }
				break;
			case ModeTransitionConditionType.HitByDunk: //Not implemented yet
				if (hitByDunk) { isValid = true; }
				break;
			case ModeTransitionConditionType.HPInferiorInferiorOrEqualTo:
				if (GetCurrentHP() <= value) { isValid = true; }
				break;
			case ModeTransitionConditionType.NoWallLeft: //Not implemented yet
				break;
			case ModeTransitionConditionType.PlayerDistanceGreaterThan:
				if (Vector3.Distance(PlayerController.GetNearestPlayer(transform.position).transform.position, transform.position) > value) { isValid = true; }
				break;
			case ModeTransitionConditionType.PlayerDistanceLessThan:
				if (Vector3.Distance(PlayerController.GetNearestPlayer(transform.position).transform.position, transform.position) < value) { isValid = true; }
				break;
			case ModeTransitionConditionType.TimeSinceModeIsEnabledGreaterThan:
				if (timeSinceLastModeChange > value) { isValid = true; }
				break;
			case ModeTransitionConditionType.WeakPointsActivated:
				if (weakPointsActivated) { isValid = true; }
				break;
		}
		return isValid;
	}
	private MethodInfo InvokeMethod(string _method)
	{
		MethodInfo mi = typeof(BossBehaviour).GetMethod(_method);
		if (mi != null) { mi.Invoke(this, new object[0]); return mi;  } else { return null; }
	}
	//Coroutines

	IEnumerator Freeze_C(float _duration)
	{
		for (float i = 0; i < _duration; i+= Time.deltaTime)
		{
			frozen = true;
			yield return null;
		}
		frozen = false;
	}

	IEnumerator Stagger_C()
	{
		yield return new WaitForSeconds(2f);
		transform.forward = -Vector3.forward;
		animator.SetTrigger("Stagger");
	}

	IEnumerator Reconstruct_C ()
	{
		hitByDunk = false;
		animator.SetTrigger("StaggerHit");
		EnableTurrets(0.15f);
		shoulderRotationEnabled = false;
		yield return new WaitForSeconds(0.25f);
		DetachTurrets();
		animator.SetTrigger("Reconstruct");
		ParticleSystem.EmissionModule em = bossExplosionFX.GetComponent<ParticleSystem>().emission;
		em.enabled = false;
		foreach (BossLeg leg in legs)
		{
			leg.Reconstruct();
			leg.DisableCanon();
		}
		yield return new WaitForSeconds(1f);
		Destroy(bossExplosionFX.gameObject);
		destroyed = false;
		weakPointsActivatedCount = 0;
		weakPointsActivated = false;
	}

	IEnumerator DetachTurrets_C()
	{
		foreach (EnemyBehaviour e in minions)
		{
			e.enabled = false;
			e.transform.rotation = Quaternion.LookRotation(Vector3.forward);
			e.transform.SetParent(null);
			Vector3 endPosition = transform.position;
			RaycastHit hit;
			Vector3 shoulderCenterPosition = transform.position;
			shoulderCenterPosition.y = e.transform.position.y;
			shoulderCenterPosition.z = e.transform.position.z;
			if (Physics.Raycast(e.transform.position + ((e.transform.position - shoulderCenterPosition).normalized * 10) - (Vector3.forward * 10), Vector3.down, out hit, 50, LayerMask.GetMask("Environment")))
			{
				endPosition = hit.point;
				StartCoroutine(MoveTurretToPosition_C(e, endPosition, 1f));
				yield return null;
			}

		}
	}

	IEnumerator MoveTurretToPosition_C(EnemyBehaviour _turret, Vector3 _endPosition, float _duration)
	{
		Vector3 startPosition = _turret.transform.position;
		for (float i = 0; i < _duration; i+= Time.deltaTime)
		{
			Vector3 newPosition = Vector3.Lerp(startPosition, _endPosition, i / _duration);
			newPosition.y = Mathf.Lerp(startPosition.y, _endPosition.y, turretDetachCurve.Evaluate(i / _duration));
			_turret.transform.position = Vector3.Lerp(startPosition, _endPosition, i / _duration);
			yield return null;
		}
		yield return null;
		_turret.transform.position = _endPosition;
		_turret.enabled = true;
		DisableTurrets();
	}

	IEnumerator GroundAttack_C()
	{
		for (float i = 0; i < groundAttackPreparationDuration; i += Time.deltaTime)
		{
			yield return null;
		}
		SpawnElectricalPlates();
	}

	public void OnHit ( BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, float _damages, DamageSource _source, Vector3 _bumpModificators = default )
	{
		if (_source == DamageSource.Dunk)
		{
			hitByDunk = true;
			Debug.Log("BossHitByDunk");
		}
	}
}
