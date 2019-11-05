using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BallData", menuName = "GameDatas/Ball", order = 1)]
public class BallDatas : ScriptableObject
{
	[Header("Global settings")]
	public float maxDistance;
	public float maxPreviewDistance;
	public float moveSpeed;
	public int maxBounces;
	public float speedMultiplierOnBounce;
	public float damageModifierOnReception;
	public float maxDamageModifier;
	public float momentumGainOnReception;
	public int damages;
	public Gradient colorOverDamage;

	[Header("FX")]
	public GameObject Trail;
	public GameObject WallHit;
	public GameObject ReceiveCore;

	public GameObject LightExplosion;
	public GameObject HeavyExplosion;

	public GameObject DunkJump;
	public GameObject DunkExplosion;
	public GameObject DunkIdle;
	public GameObject DunkDash;
	public GameObject DunkReceiving;

	public GameObject PerfectReception;
}
