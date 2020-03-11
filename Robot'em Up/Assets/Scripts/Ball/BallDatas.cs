using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BallData", menuName = "GameDatas/Ball", order = 1)]
public class BallDatas : ScriptableObject
{
	[Separator("Global settings")]
	public float maxPreviewDistance;
	public float moveSpeed;
	public int maxBounces;
	public float speedMultiplierOnBounce;
	public float speedMultiplierOnPerfectReception;
	public float maxSpeedMultiplierOnPerfectReception;
	public float damageModifierOnPerfectReception;
	public float maxDamageModifierOnPerfectReception;
	public int damages;
	public float maxFXSizeMultiplierOnPerfectReception;
	public float maxTimeOutOfScreen = 3f;
	public float comingBackToScreenSpeed = 5f;
	public float timescaleOnHit = 0.1f;
	public float timescaleDurationOnHit = 0.1f;
	public Gradient colorOverDamage;
	public float minimalChargeForPush;
	public float minimalChargeForBump;
}
