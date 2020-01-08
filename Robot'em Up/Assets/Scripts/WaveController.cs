﻿using System.Collections;
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
		float i_pickChances = Random.value;
		int i_chosenIndex = 0;
		float i_cumulativeChances = waveList[currentWaveIndex].currentEnemies[i_chosenIndex].probability;

		while (i_pickChances > i_cumulativeChances && i_chosenIndex < waveList[currentWaveIndex].currentEnemies.Count)
		{
			i_chosenIndex++;
			i_cumulativeChances += waveList[currentWaveIndex].currentEnemies[i_chosenIndex].probability;
		}
		return waveList[currentWaveIndex].currentEnemies[i_chosenIndex];
	}
	public void InstantiateEnemy ( WaveEnemy _enemy )
	{
		if (_enemy.spawnIndexes.Count <= 0) { Debug.LogWarning("Can't spawn enemy: no spawn assigned"); return; }
		GameObject i_newEnemy = Instantiate(_enemy.enemyType.prefab).gameObject;
		EnemyBehaviour i_enemyBehaviour = i_newEnemy.GetComponent<EnemyBehaviour>();
		if (i_enemyBehaviour == null) { Destroy(i_newEnemy); Debug.LogWarning("Wave can't instantiate enemy: invalid prefab"); return; }
		i_enemyBehaviour.onDeath.AddListener(() => { OnEnemyDeath(i_enemyBehaviour); });

		int i_chosenSpawnerIndex = Random.Range(0, _enemy.spawnIndexes.Count);
		i_chosenSpawnerIndex = _enemy.spawnIndexes[i_chosenSpawnerIndex];
		i_enemyBehaviour.GetNavMesh().enabled = false;
		i_newEnemy.transform.position = spawnList[i_chosenSpawnerIndex].transform.position;
		i_enemyBehaviour.GetNavMesh().enabled = true;
		currentEnemies.Add(i_enemyBehaviour);
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
