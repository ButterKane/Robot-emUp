using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDatas", menuName = "GlobalDatas/EnemyDatas", order = 1)]
public class EnemyDatas : ScriptableObject
{
	public List<EnemyData> enemyDatas;
}

[System.Serializable]
public class EnemyData
{
	public string name;
	public GameObject prefab;
	public Color color;
	public int powerLevel;
}
