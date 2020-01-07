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
	public GameObject trail;
	public GameObject wallHit;
	public GameObject receiveCore;
	public GameObject throwCore;

	public GameObject lightExplosion;
	public GameObject heavyExplosion;

	public GameObject dunkJump;
	public GameObject dunkExplosion;
	public GameObject dunkIdle;
	public GameObject dunkDash;
	public GameObject dunkReceiving;

	public GameObject perfectReception;
}
