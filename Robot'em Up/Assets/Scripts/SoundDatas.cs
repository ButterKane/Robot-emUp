using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundDatas", menuName = "GlobalDatas/SoundDatas", order = 1)]
public class SoundDatas : ScriptableObject
{
	public List<SoundData> soundList = new List<SoundData>(); 
}
