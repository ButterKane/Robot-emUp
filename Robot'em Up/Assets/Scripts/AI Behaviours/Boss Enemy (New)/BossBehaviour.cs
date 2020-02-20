using MyBox;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BossBehaviour : MonoBehaviour
{
	public enum BossPhase { PhaseOne, PhaseTwo }

	[Header("Global Settings")]
	public float topPartRotationSpeed;
	public float firstPhaseHP;
	public float secondPhaseHP;
	public float healthBarHeight;
	public float minAttackRange = 15f;
	public Transform zoneCenter;
	public BossMode startingMode;

	[Header("BulletHell settings")]
	public float bulletHellRotationSpeed;
	public List<Transform> bulletSpawner;
	public float minDelayBetweenBullets;
	public float maxDelayBetweenBullets;

	[Header("Range mode settings")]
	public float shoulderRotationSpeed;

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

	//Other values
	private float timeSinceLastModeChange;
	private bool weakPointsActivated;
	private List<EnemyBehaviour> minions;

	private NavMeshAgent navMesh;
	private List<Quaternion> shoulderInitialRotation;
	private bool shoulderRotationEnabled;
	private bool bulletStormEnabled;
	private List<float> bulletStormCooldowns;

	private void Start ()
	{
		GetReferences();
		GenerateHealthBars();
		InitializeValues();
		ChangeMode(startingMode);
	}
	private void Update ()
	{
		UpdateMovement();
		CheckForModeTransition();
		UpdateHealthBars();
		UpdateCooldowns();
		UpdateShoulderRotation();
		UpdateBulletStormMode();
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

	public void EnableTurrets()
	{
		shoulderRotationEnabled = true;
		foreach (Spawner s in minionSpawners)
		{
			minions.Add(s.SpawnEnemy(enemyToSpawn, false));
		}
	}

	public void DisableTurrets ()
	{
		shoulderRotationEnabled = false;
		for (int i = 0; i < minionSpawners.Count; i++)
		{
			if (minions[i] != null)
			{
				minionSpawners[i].RetractEnemy(minions[i]);
			}
		}
		minions.Clear();
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
		Debug.Log("Punch");
	}

	public void SpawnMinions()
	{

	}
	
	public void StartBulletStorm ()
	{
		bulletStormEnabled = true;
		Debug.Log("Bullet storm");
	}

	public void StopBulletStorm ()
	{
		bulletStormEnabled = false;
	}

	public void DetachMinions()
	{

	}

	public void HammerPunch()
	{

	}

	public void LaserAttack()
	{

	}

	public void GroundAttack()
	{

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
		bulletStormCooldowns = new List<float>();
		foreach (Transform t in bulletSpawner)
		{
			bulletStormCooldowns.Add(0);
		}
	}

	private void UpdateBulletStormMode ()
	{
		if (bulletStormEnabled)
		{
			topPart.localRotation = Quaternion.Euler(new Vector3(topPart.localRotation.eulerAngles.x, topPart.localRotation.eulerAngles.y + Time.deltaTime * bulletHellRotationSpeed, topPart.localRotation.eulerAngles.z));
			for (int i = 0; i < bulletSpawner.Count; i++)
			{
				if (bulletStormCooldowns[i] <= 0)
				{
					//Spawn bullet
					GameObject bullet = Instantiate(Resources.Load<GameObject>("EnemyResource/BossResource/BulletStormPrefab"));
					bullet.transform.position = bulletSpawner[i].transform.position;
					bullet.transform.rotation = Quaternion.LookRotation(bulletSpawner[i].forward + AddNoiseOnAngle(0, 30));
					bulletStormCooldowns[i] = Random.Range(minDelayBetweenBullets, maxDelayBetweenBullets);
				}

			}
		}
	}

	Vector3 AddNoiseOnAngle ( float min, float max )
	{
		// Find random angle between min & max inclusive
		float xNoise = Random.Range(min, max);
		float yNoise = 0f;
		float zNoise = 0f;

		// Convert Angle to Vector3
		Vector3 noise = new Vector3(
		  Mathf.Sin(2 * Mathf.PI * xNoise / 360),
		  Mathf.Sin(2 * Mathf.PI * yNoise / 360),
		  Mathf.Sin(2 * Mathf.PI * zNoise / 360)
		);
		return noise;
	}

	private void GetReferences()
	{
		navMesh = GetComponent<NavMeshAgent>();
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
		Vector3 destination = Vector3.zero;
		switch (currentMode.movementType)
		{
			case BossMovementType.DontMove:
				break;
			case BossMovementType.FollowNearestPlayer:
				PlayerController target = PlayerController.GetNearestPlayer(transform.position);
				destination = target.transform.position;
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
				foreach (string s in currentMode.actionsOnMovementEnd)
				{
					InvokeMethod(s);
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
		List<float> newBulletStormCD = new List<float>();
		foreach (float f in bulletStormCooldowns)
		{
			newBulletStormCD.Add(f - Time.deltaTime);
		}
		bulletStormCooldowns = newBulletStormCD;
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
		if (shoulderRotationEnabled)
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

	private bool IsTransitionConditionValid(ModeTransitionCondition _mtc)
	{
		float value = _mtc.modeTransitionConditionValue;
		bool isValid = false;
		switch (_mtc.modeTransitionConditionType)
		{
			case ModeTransitionConditionType.HitByDunk: //Not implemented yet
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
}
