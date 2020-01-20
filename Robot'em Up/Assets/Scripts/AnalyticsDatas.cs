using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

[CreateAssetMenu(fileName = "AnalyticsDatas", menuName = "GlobalDatas/AnalyticsDatas", order = 1)]
public class AnalyticsDatas : ScriptableObject
{
	public List<AnalyticsData> analyticsDatas = new List<AnalyticsData>();
}

[System.Serializable]
public class AnalyticsData
{
	public string dataName;
	public bool sortPerZone;
	public bool sortPerArena;
	public bool perPlayer;
	public int playerOneValue;
	public int playerTwoValue;
	public int totalValue;
}
