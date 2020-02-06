using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "CameraGlobalDatas", menuName = "GlobalDatas/CameraGlobalDatas", order = 1)]
public class CameraGlobalSettings : ScriptableObject
{
	public float outOfCameraMaxDistancePercentage = 0.1f;
}
