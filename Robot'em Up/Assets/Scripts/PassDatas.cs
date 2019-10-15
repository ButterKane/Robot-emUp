using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PassData", menuName = "GameDatas/Pass", order = 1)]
public class PassDatas : ScriptableObject
{
	public float maxDistance;
	public float moveSpeed;
	public int maxBounces;
	public float speedMultiplierOnBounce;
}
