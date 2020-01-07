using UnityEngine;
using MyBox;

[CreateAssetMenu(fileName = "EnergyData", menuName = "GlobalDatas/Energy", order = 1)]
public class EnergyData : ScriptableObject
{
	[Separator("Global settings")]
	public float energyLerpSpeed;
}