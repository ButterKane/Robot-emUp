using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveData
{
	public AnimationCurve maxPowerLevel = new AnimationCurve();
	public float duration = 20f;
	public float pauseBeforeNextWave = 1f;

	public List<WaveEnemyProbability> currentEnemies = new List<WaveEnemyProbability>();

	public void SetEnemySpawnProbability(EnemyData _enemy, float _newProbability)
	{
		if (currentEnemies.Count <= 1) { return; }
		float difference = 0f;
		float otherTotal = 0;
		foreach (WaveEnemyProbability wep in currentEnemies)
		{
			if (_enemy.name == wep.enemyType.name)
			{
				difference = _newProbability - wep.probability;
				wep.probability = _newProbability;
			} else
			{
				otherTotal += wep.probability;
			}
		}

		//Reduce or improve all the other probabilities (So it keep a maximum of 1)
		foreach (WaveEnemyProbability wep in currentEnemies)
		{
			if (_enemy.name != wep.enemyType.name)
			{
				if (difference > 0)
				{
					if (otherTotal != 0)
					{
						float force = wep.probability / otherTotal;
						wep.probability -= force * difference;
					}
				} else
				{
					if ((float)(currentEnemies.Count - 1) - otherTotal != 0) {
						float force = (1f - wep.probability) / ((float)(currentEnemies.Count - 1) - otherTotal);
						wep.probability -= force * difference;
					}
				}
			}
			wep.probability = Mathf.Clamp(wep.probability, 0f, 1f);
		}
	}

	public void RemoveEnemySpawnProbability( WaveEnemyProbability _wep)
	{
		float difference = _wep.probability;
		currentEnemies.Remove(_wep);
		float totalProba = 0;
		foreach (WaveEnemyProbability wep in currentEnemies)
		{
			totalProba += wep.probability;
		}

		//Reduce or improve all the other probabilities (So it keep a maximum of 1)
		foreach (WaveEnemyProbability wep in currentEnemies)
		{
			if (totalProba != 0)
			{
				float force = wep.probability / totalProba;
				wep.probability += force * difference;
				wep.probability = Mathf.Clamp(wep.probability, 0f, 1f);
			} else
			{
				wep.probability = 1;
			}
		}
	}
}

[System.Serializable]
public class WaveEnemyProbability
{
	public EnemyData enemyType;
	public float probability;
}
