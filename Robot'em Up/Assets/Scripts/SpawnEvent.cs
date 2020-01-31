using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpawnEventController : MonoBehaviour
{
	public SpawnEvent spawnEvent;
	private void Awake ()
	{
		spawnEvent.linkedSpawnEventController = this;
		spawnEvent.isEnabled = true;  
	}
}

[System.Serializable]
public class SpawnEvent //A spawn event is either called from a waveController, or from a spawnEventController, which is an object in scene
{
	public List<SpawnEventInformation> spawnEventList;
	public bool mustKillEnemiesBeforeNextWave;
	public float delayBeforeNextWave = 1f;
	public SpawnEventController linkedSpawnEventController; //Spawn event is either linked to a spawnEventController, or to a waveController
	public WaveController linkedWaveController;
	public bool isEnabled;
	[System.NonSerialized] public UnityEvent onEnd = new UnityEvent();

	private bool eventFinished;
	private List<EnemyBehaviour> enemyList;

	public void StartEvent()
	{
		if (eventFinished != false) { return; }
		eventFinished = false;
		enemyList = new List<EnemyBehaviour>();
		for (int i = 0; i < spawnEventList.Count; i++)
		{
			if (linkedSpawnEventController != null)
			{
				linkedSpawnEventController.StartCoroutine(StartEvent_C(spawnEventList[i]));
			}
			if (linkedWaveController != null)
			{
				linkedWaveController.StartCoroutine(StartEvent_C(spawnEventList[i]));
			}
		}
	}

	IEnumerator StartEvent_C( SpawnEventInformation _inf)
	{
		yield return new WaitForSeconds(_inf.delayBeforeSpawn);
		EnemyBehaviour newEnemy = _inf.spawner.SpawnEnemy(_inf.enemyType, false);
		newEnemy.onDeath.AddListener(() => { OnEnemyDeath(newEnemy); });
		enemyList.Add(newEnemy);

		if (enemyList.Count >= spawnEventList.Count && !mustKillEnemiesBeforeNextWave && linkedWaveController != null)
		{
			yield return new WaitForSeconds(delayBeforeNextWave);
			OnEventEnd();
		}
	}

	void OnEventEnd()
	{
		eventFinished = true;
		onEnd.Invoke();
	}

	void OnEnemyDeath(EnemyBehaviour _enemy)
	{
		enemyList.Remove(_enemy);
		if (enemyList.Count <= 0)
		{
			if (linkedWaveController != null)
			{
				linkedWaveController.StartCoroutine(WaitAndEnd_C());
			}
			if (linkedSpawnEventController != null)
			{
				linkedSpawnEventController.StartCoroutine(WaitAndEnd_C());
			}
		}
	}

	IEnumerator WaitAndEnd_C()
	{
		yield return new WaitForSeconds(delayBeforeNextWave);
		OnEventEnd();
	}
}

[System.Serializable]
public class SpawnEventInformation
{
	public EnemyData enemyType;
	public Spawner spawner;
	public float delayBeforeSpawn;
}
