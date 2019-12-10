using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BallData", menuName = "GameDatas/Ball", order = 1)]
public class BallDatas : ScriptableObject
{
	[Header("Global settings")]
	public float maxPreviewDistance;
	public float moveSpeed;
	public int maxBounces;
	public float speedMultiplierOnBounce;
	public float speedMultiplierOnPerfectReception;
	public float maxSpeedMultiplierOnPerfectReception;
	public float damageModifierOnPerfectReception;
	public float maxDamageModifierOnPerfectReception;
	public int damages;
	public Gradient colorOverDamage;
	public Texture2D hitDecal;

	[Header("FX")]
	public GameObject Trail;
	public GameObject WallHit;
	public GameObject ReceiveCore;
	public GameObject ThrowCore;

	public GameObject LightExplosion;
	public GameObject HeavyExplosion;

	public GameObject DunkJump;
	public GameObject DunkExplosion;
	public GameObject DunkIdle;
	public GameObject DunkDash;
	public GameObject DunkReceiving;

	public GameObject PerfectReception;
}
