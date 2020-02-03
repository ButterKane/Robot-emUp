using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CameraDefaultDatas", menuName = "GlobalDatas/CameraDefaultDatas", order = 1)]
public class CameraDefaultDatas : ScriptableObject
{
	public float fov = 50;
	public float deadZoneWidth = 0.1f;
	public float deadZoneHeight = 0.1f;
	public float deadZoneDepth = 10f;
	public float softZoneWidth = 0.25f;
	public float softZoneHeight = 0.25f;
	public float minDistance = 30;
	public float maxDistance = 30;
	public float distance = 30;
}
