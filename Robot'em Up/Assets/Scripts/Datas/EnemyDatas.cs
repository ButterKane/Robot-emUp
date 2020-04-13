using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDatas", menuName = "GlobalDatas/EnemyDatas", order = 1)]
public class EnemyDatas : ScriptableObject
{
	public List<EnemyData> enemyDatas;

	public int GetEnemyIndex(EnemyData _enemy)
	{
		for (int i = 0; i < enemyDatas.Count; i++)
		{
			if (_enemy.name == enemyDatas[i].name)
			{
				return i;
			}
		}
		return 0;
	}

	public static EnemyDatas GetEnemyDatas()
	{
		return Resources.Load<EnemyDatas>("EnemyDatas");
	}
	public EnemyData GetEnemyByID(string id)
	{
		foreach (EnemyData d in enemyDatas)
		{
			if (id == d.name)
			{
				return d;
			}
		}
		return null;
	}
}

[System.Serializable]
public class EnemyData
{ 
	public string name;
	public GameObject prefab;
	public Color color;
}
