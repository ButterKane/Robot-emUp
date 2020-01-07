using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class WaveData
{
	public AnimationCurve maxPowerLevel = new AnimationCurve();
	public float duration = 20f;
	public float pauseBeforeNextWave = 1f;
	public UnityEvent onEndEvents;

	public List<WaveEnemy> currentEnemies = new List<WaveEnemy>();

	public void SetEnemySpawnProbability(EnemyData _enemy, float _newProbability)
	{
		if (currentEnemies.Count <= 1) { return; }
		float internal_difference = 0f;
		float internal_otherTotal = 0;
		foreach (WaveEnemy wep in currentEnemies)
		{
			if (_enemy.name == wep.enemyType.name)
			{
				internal_difference = _newProbability - wep.probability;
				wep.probability = _newProbability;
			} else
			{
				internal_otherTotal += wep.probability;
			}
		}

		//Reduce or improve all the other probabilities (So it keep a maximum of 1)
		foreach (WaveEnemy wep in currentEnemies)
		{
			if (_enemy.name != wep.enemyType.name)
			{
				if (internal_difference > 0)
				{
					if (internal_otherTotal != 0)
					{
						float force = wep.probability / internal_otherTotal;
						wep.probability -= force * internal_difference;
					}
				} else
				{
					if ((float)(currentEnemies.Count - 1) - internal_otherTotal != 0) {
						float force = (1f - wep.probability) / ((float)(currentEnemies.Count - 1) - internal_otherTotal);
						wep.probability -= force * internal_difference;
					}
				}
			}
			wep.probability = Mathf.Clamp(wep.probability, 0f, 1f);
		}
	}

	public void RemoveEnemySpawnProbability( WaveEnemy _wep)
	{
		float internal_difference = _wep.probability;
		currentEnemies.Remove(_wep);
		float internal_totalProba = 0;
		foreach (WaveEnemy wep in currentEnemies)
		{
			internal_totalProba += wep.probability;
		}

		//Reduce or improve all the other probabilities (So it keep a maximum of 1)
		foreach (WaveEnemy wep in currentEnemies)
		{
			if (internal_totalProba != 0)
			{
				float force = wep.probability / internal_totalProba;
				wep.probability += force * internal_difference;
				wep.probability = Mathf.Clamp(wep.probability, 0f, 1f);
			} else
			{
				wep.probability = 1;
			}
		}
	}
}

[System.Serializable]
public class WaveEnemy
{
	public EnemyData enemyType;
	public float probability;
	public List<int> spawnIndexes;
}
