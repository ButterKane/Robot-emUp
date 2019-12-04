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
				if (delayBeforeNextWave <= 0 && currentPowerLevel <= 0)
				{
					StartNextWave();
				} else
				{
					delayBeforeNextWave -= Time.deltaTime;
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
		Debug.Log("Starting wave " + _waveIndex);
		for (float i = 0; i < waveList[_waveIndex].duration; i+=Time.deltaTime)
		{
			yield return null;
			currentMaxPowerLevel = waveList[_waveIndex].maxPowerLevel.Evaluate(i / waveList[_waveIndex].duration);
		}
		Debug.Log("Wave " + _waveIndex + " finished");
		StopWave();
	}

	public WaveEnemy GetRandomEnemy()
	{
		if (waveList[currentWaveIndex].currentEnemies.Count <= 0) { Debug.LogWarning("Wave can't instantiate enemies: no list defined"); return null; }
		float pickChances = Random.value;
		int chosenIndex = 0;
		float cumulativeChances = waveList[currentWaveIndex].currentEnemies[chosenIndex].probability;
		while (pickChances > cumulativeChances && chosenIndex < waveList[currentWaveIndex].currentEnemies.Count - 1)
		{
			chosenIndex++;
			cumulativeChances += waveList[currentWaveIndex].currentEnemies[chosenIndex].probability;
		}
		return waveList[currentWaveIndex].currentEnemies[chosenIndex];
	}
	public void InstantiateEnemy ( WaveEnemy _enemy )
	{
		GameObject newEnemy = Instantiate(_enemy.enemyType.prefab).gameObject;
		EnemyBehaviour enemyBehaviour = newEnemy.GetComponent<EnemyBehaviour>();
		if (enemyBehaviour == null) { Destroy(newEnemy); Debug.LogWarning("Wave can't instantiate enemy: invalid prefab"); return; }
		enemyBehaviour.onDeath.AddListener(() => { OnEnemyDeath(enemyBehaviour); });

		int chosenSpawnerIndex = Random.Range(0, _enemy.spawnIndexes.Count - 1);
		chosenSpawnerIndex = _enemy.spawnIndexes[chosenSpawnerIndex];
		for (int i = 0; i < _enemy.spawnIndexes.Count; i++)
		{
			Debug.Log("Enemy spawn indexes: " + _enemy.spawnIndexes[i]);
		}
		Debug.Log("Chosen spawn: " + chosenSpawnerIndex);
		enemyBehaviour.GetNavMeshAgent().enabled = false;
		newEnemy.transform.position = spawnList[chosenSpawnerIndex].transform.position;
		enemyBehaviour.GetNavMeshAgent().enabled = true;
		currentEnemies.Add(enemyBehaviour);
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
		Debug.Log("Instantied enemy " + _enemy.name + " has been killed");
	}
}
