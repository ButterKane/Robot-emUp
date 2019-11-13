using UnityEngine;

[CreateAssetMenu(fileName = "EnergyData", menuName = "GlobalDatas/Energy", order = 1)]
public class EnergyData : ScriptableObject
{
	[Header("Global settings")]
	public float energyLerpSpeed;
}