using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public class SpawnInformation
{
	public string customName;
	public Transform transform;
	public Color color;
}
public class WaveController : MonoBehaviour 
{
	public CameraZone zone;
	public bool startOnTriggerEnter;
	public UnityEvent onStartEvents;
	public List<WaveData> waveList = new List<WaveData>(); 
	public List<SpawnInformation> spawnList = new List<SpawnInformation>();


	private bool waveStarted = false;
	private int currentWaveIndex = -1;
	private float currentMaxPowerLevel;
	private float currentPowerLevel;

	public List<EnemyBehaviour> currentEnemies;
	private WaveEnemy nextEnemyToSpawn;
	private float delayBeforeNextWave;

	private void Awake ()
	{
		currentWaveIndex = -1;
		zone = GetComponentInChildren<CameraZone>();
		if (startOnTriggerEnter && zone != null)
		{
			zone.onZoneActivation.AddListener(StartNextWave);
		}
	}

	private void Update ()
	{
		if (waveStarted) { 
			if (nextEnemyToSpawn == null)
			{
				nextEnemyToSpawn = GetRandomEnemy();
			} else
			{
				if (nextEnemyToSpawn.enemyType.prefab.GetComponent<EnemyBehaviour>().powerLevel + currentPowerLevel <= currentMaxPowerLevel)
				{
					InstantiateEnemy(nextEnemyToSpawn);
					nextEnemyToSpawn = null;
				}
			}
		} else
		{
			if (currentWaveIndex >= 0)
			{
				if (currentPowerLevel <= 0)
				{
					delayBeforeNextWave -= Time.deltaTime;
					if (delayBeforeNextWave <= 0)
					{
						StartNextWave();
					}
				}
			}
		}
	}

	public void StartNextWave()
	{
		if (waveStarted) { return; }
		if (currentWaveIndex+1 >= waveList.Count) { return; }
		currentWaveIndex++;
		if (currentWaveIndex == 0)
		{
			onStartEvents.Invoke();
		}
		waveStarted = true;
		StartCoroutine(StartWave_C(currentWaveIndex));
	}

	public void StopWave()
	{
		delayBeforeNextWave = waveList[currentWaveIndex].pauseBeforeNextWave;
		waveStarted = false;
		waveList[currentWaveIndex].onEndEvents.Invoke();
	}

	IEnumerator StartWave_C (int _waveIndex)
	{
		for (float i = 0; i < waveList[_waveIndex].duration; i+=Time.deltaTime)
		{
			yield return null;
			currentMaxPowerLevel = waveList[_waveIndex].maxPowerLevel.Evaluate(i / waveList[_waveIndex].duration);
		}
		StopWave();
	}

	public WaveEnemy GetRandomEnemy()
	{
		if (waveList[currentWaveIndex].currentEnemies.Count <= 0) { Debug.LogWarning("Wave can't instantiate enemies: no list defined"); return null; }
		float internal_pickChances = Random.value;
		int internal_chosenIndex = 0;
		float internal_cumulativeChances = waveList[currentWaveIndex].currentEnemies[internal_chosenIndex].probability;

		while (internal_pickChances > internal_cumulativeChances && internal_chosenIndex < waveList[currentWaveIndex].currentEnemies.Count)
		{
			internal_chosenIndex++;
			internal_cumulativeChances += waveList[currentWaveIndex].currentEnemies[internal_chosenIndex].probability;
		}
		return waveList[currentWaveIndex].currentEnemies[internal_chosenIndex];
	}
	public void InstantiateEnemy ( WaveEnemy _enemy )
	{
		if (_enemy.spawnIndexes.Count <= 0) { Debug.LogWarning("Can't spawn enemy: no spawn assigned"); return; }
		GameObject internal_newEnemy = Instantiate(_enemy.enemyType.prefab).gameObject;
		EnemyBehaviour internal_enemyBehaviour = internal_newEnemy.GetComponent<EnemyBehaviour>();
		if (internal_enemyBehaviour == null) { Destroy(internal_newEnemy); Debug.LogWarning("Wave can't instantiate enemy: invalid prefab"); return; }
		internal_enemyBehaviour.onDeath.AddListener(() => { OnEnemyDeath(internal_enemyBehaviour); });

		int internal_chosenSpawnerIndex = Random.Range(0, _enemy.spawnIndexes.Count);
		internal_chosenSpawnerIndex = _enemy.spawnIndexes[internal_chosenSpawnerIndex];
		internal_enemyBehaviour.GetNavMeshAgent().enabled = false;
		internal_newEnemy.transform.position = spawnList[internal_chosenSpawnerIndex].transform.position;
		internal_enemyBehaviour.GetNavMeshAgent().enabled = true;
		currentEnemies.Add(internal_enemyBehaviour);
		UpdateCurrentPowerLevel();
	}

	public float UpdateCurrentPowerLevel()
	{
		currentPowerLevel = 0;
		foreach (EnemyBehaviour enemy in currentEnemies)
		{
			currentPowerLevel += enemy.powerLevel;
		}
		return currentPowerLevel;
	}

	void OnEnemyDeath(EnemyBehaviour _enemy)
	{
		currentEnemies.Remove(_enemy);
		UpdateCurrentPowerLevel();
	}
}
