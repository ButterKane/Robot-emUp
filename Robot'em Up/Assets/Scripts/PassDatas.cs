using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PassData", menuName = "GameDatas/Pass", order = 1)]
public class PassDatas : ScriptableObject
{
	[Header("Global settings")]
	public float maxDistance;
	public float maxPreviewDistance;
	public float moveSpeed;
	public int maxBounces;
	public float speedMultiplierOnBounce;

	[Header("FX")]
	public GameObject Trail;
	public GameObject DunkExplosion;
	public GameObject WallHit;
	public GameObject ReceiveCore;

	public GameObject LightExplosion;
	public GameObject HeavyExplosion;
}
