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
	public ArenaDoor exitDoor;
	public bool skipArena = false;

	private bool waveStarted = false;
	private int currentWaveIndex = -1;
	private float currentMaxPowerLevel;
	private float currentPowerLevel;

	public List<EnemyBehaviour> currentEnemies;
	private WaveEnemy nextEnemyToSpawn;
	private float delayBeforeNextWave;
	private bool enemiesKilled = false;
	private bool arenaFinished = false;
	private bool eventPlaying;

	private void Awake ()
	{
		arenaFinished = false;
		currentWaveIndex = -1;
		zone = GetComponentInChildren<CameraZone>();
		if (startOnTriggerEnter && zone != null)
		{
			zone.onZoneActivation.AddListener(StartArena);
		}
	}
	private void Update ()
	{
		if (arenaFinished) { return; }
		if (waveStarted) { 
			if (nextEnemyToSpawn == null)
			{
				nextEnemyToSpawn = GetRandomEnemy();
			} else
			{
				if (nextEnemyToSpawn.enemyType.prefab.GetComponent<EnemyBehaviour>().powerLevel + currentPowerLevel <= currentMaxPowerLevel)
				{
					InstantiateEnemy(nextEnemyToSpawn);
				}
			}
		} else
		{
			if (currentWaveIndex >= 0 && !eventPlaying)
			{
				if (currentPowerLevel <= 0)
				{
					EndWave();
					delayBeforeNextWave -= Time.deltaTime;
					if (delayBeforeNextWave <= 0)
					{
						StartNextWave();
					}
				}
			}
		}
	}

	#region Public functions
	public void StartArena()
	{
		if (skipArena) { EndArena(); return; }
		if (currentWaveIndex >= 0) { return; } else
		{
			StartNextWave();
		}
		onStartEvents.Invoke();
		if (exitDoor != null)
		{
			exitDoor.OnArenaEnter();
		}
	}
	public void RegisterEnemy(EnemyBehaviour _enemy)
	{
		_enemy.onDeath.AddListener(() => { OnEnemyDeath(_enemy); });
		currentEnemies.Add(_enemy);
		UpdateCurrentPowerLevel();
	}
	public void EndWave()
	{
		if (!enemiesKilled)
		{
			FeedbackManager.SendFeedback("event.ArenaWaveFinished", this);
			if (exitDoor != null) { exitDoor.OnWaveFinished(); }
			enemiesKilled = true;
			currentEnemies.Clear();
		}
	}
	public void EndArena()
	{
		if (arenaFinished) { return; }
		arenaFinished = true;
		if (exitDoor != null) { exitDoor.OnWaveFinished(); }
		FeedbackManager.SendFeedback("event.ArenaFinished", this);
		if (exitDoor != null)
		{
			exitDoor.OnArenaFinished();
		}
	}
	public void StartNextWave()
	{
		if (waveStarted) { return; }
		if (currentWaveIndex >= 0)
		{
			waveList[currentWaveIndex].onEndEvents.Invoke();
		}
		currentWaveIndex++;
		if (currentWaveIndex >= waveList.Count) { EndArena(); return; }
		if (exitDoor != null) { exitDoor.OnWaveStart(); }
		enemiesKilled = false;
		if (waveList[currentWaveIndex].onStartSpawnEvent.isEnabled)
		{
			eventPlaying = true;
			waveList[currentWaveIndex].onStartSpawnEvent.linkedWaveController = this;
			waveList[currentWaveIndex].onStartSpawnEvent.onEnd.AddListener(() => { StartCoroutine(StartWave_C(currentWaveIndex)); eventPlaying = false; });
			waveList[currentWaveIndex].onStartSpawnEvent.StartEvent();
			return;
		} else
		{
			if (waveList[currentWaveIndex].currentEnemies.Count <= 0) { EndArena(); return; }
		}
		if (currentWaveIndex <= waveList.Count)
		{
			StartCoroutine(StartWave_C(currentWaveIndex));
		}
	}
	public void StopWaveSpawning()
	{
		delayBeforeNextWave = waveList[currentWaveIndex].pauseBeforeNextWave;
		waveStarted = false;
	}
	public WaveEnemy GetRandomEnemy ()
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
		int i_chosenSpawnerIndex = Random.Range(0, _enemy.spawnIndexes.Count);
		i_chosenSpawnerIndex = _enemy.spawnIndexes[i_chosenSpawnerIndex];
		GameObject i_spawner = spawnList[i_chosenSpawnerIndex].transform.gameObject;

		Spawner i_spawnerScript = i_spawner.GetComponent<Spawner>();
		if (i_spawnerScript != null && !i_spawnerScript.IsFree()) { return; }

		//A custom spawner is assigned, it can be used to spawn enemy
		if (i_spawnerScript != null)
		{
			if (!i_spawnerScript.IsFree()) { return; }
			EnemyBehaviour spawnedEnemy = i_spawnerScript.SpawnEnemy(EnemyDatas.GetEnemyDatas().GetEnemyByID(_enemy.enemyType.name), true);
			RegisterEnemy(spawnedEnemy);
		}

		//No custom spawner found, it'll spawn the enemy instantly on the assigned position
		else
		{
			GameObject i_newEnemy = Instantiate(_enemy.enemyType.prefab).gameObject;
			EnemyBehaviour i_enemyBehaviour = i_newEnemy.GetComponent<EnemyBehaviour>();
			if (i_enemyBehaviour == null) { Destroy(i_newEnemy); Debug.LogWarning("Wave can't instantiate enemy: invalid prefab"); return; }
			if (i_enemyBehaviour.GetNavMesh() != null) { i_enemyBehaviour.GetNavMesh().enabled = false; }
			i_newEnemy.transform.position = spawnList[i_chosenSpawnerIndex].transform.position;
			i_enemyBehaviour.GetNavMesh().enabled = true;
			RegisterEnemy(i_enemyBehaviour);
		}
		UpdateCurrentPowerLevel();
		nextEnemyToSpawn = null;
	}
	public float UpdateCurrentPowerLevel ()
	{
		currentPowerLevel = 0;
		foreach (EnemyBehaviour enemy in currentEnemies)
		{
			currentPowerLevel += enemy.powerLevel;
		}
		return currentPowerLevel;
	}
	#endregion

	#region Private functions
	private void OnEnemyDeath ( EnemyBehaviour _enemy )
	{
		currentEnemies.Remove(_enemy);
		UpdateCurrentPowerLevel();
	}
	#endregion

	#region Coroutines
	IEnumerator StartWave_C (int _waveIndex)
	{
		waveStarted = true;

		if (_waveIndex < waveList.Count)
		{
			float waveDuration = waveList[_waveIndex].duration;
			if (waveDuration <= -1)
			{
				currentMaxPowerLevel = waveList[_waveIndex].maxPowerLevel.Evaluate(0);
			}
			else
			{
				for (float i = 0; i < waveDuration; i += Time.deltaTime)
				{
					yield return null;
					currentMaxPowerLevel = waveList[_waveIndex].maxPowerLevel.Evaluate(i / waveList[_waveIndex].duration);
				}
				StopWaveSpawning();
			}
		}
	}
	#endregion
}
