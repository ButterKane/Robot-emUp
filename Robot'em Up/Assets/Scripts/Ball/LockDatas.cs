using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

[CreateAssetMenu(fileName = "LockData", menuName = "GlobalDatas/Lock", order = 1)]
public class LockDatas : ScriptableObject
{
	[Separator("General settings")]
	public bool enableLock = true;
}
