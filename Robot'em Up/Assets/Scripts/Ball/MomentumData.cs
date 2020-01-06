﻿using UnityEngine;

[CreateAssetMenu(fileName = "MomentumData", menuName = "GlobalDatas/Momentum", order = 1)]
public class MomentumData : ScriptableObject
{
	[Header("Global settings")]
	public float momentumLerpSpeed;

	[Header("Multipliers settings")]
	public Vector2 playerSpeedMultiplier;
	public Vector2 dashRecoverSpeedMultiplier;
	public Vector2 ballSpeedMultiplier;
	public Vector2 energyGainMultiplier;
	public Vector2 ballDamageMultiplier;
	public Vector2 enemySpeedMultiplier;
	public Vector2 enemySpawnRateMultiplier;
	public Vector2 vibrationMultiplier;
	public Vector2 screenShakeMultiplier;

	[Header("PostProcess settings")]
	public Vector2 minMaxBloom;
	public Vector2 minMaxTemperature;
	public Vector2 minMaxChromaticAberration;
    public Vector2 minMaxGrain;

	[Header("Gain settings")]
	[Range(0f, 1f)] public float momentumGainedOnPass;
	[Range(0f, 1f)] public float momentumGainedOnHit;
	[Range(0f, 1f)] public float momentumGainedOnDunk;
	[Range(0f, 1f)] public float momentumGainedOnPerfectReception;

	[Header("Losses settings")]
	public float minPassDelayBeforeMomentumLoss;
	public float momentumLossSpeedIfNoPass;
	[Range(0f, 1f)] public float momentumLossOnFightEnd;
	[Range(0f, 1f)] public float momentumLossOnDamage;
	[Range(0f, 1f)] public float momentumLossWhenBallHitTheGround;
}