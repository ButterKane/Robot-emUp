using UnityEngine;
using MyBox;

[CreateAssetMenu(fileName = "MomentumData", menuName = "GlobalDatas/Momentum", order = 1)]
public class MomentumData : ScriptableObject
{
	[Separator("General settings")]
	public float momentumLerpSpeed = 4;

	[Separator("Multipliers settings")]
	public Vector2 playerSpeedMultiplier = new Vector2(1, 1.15f);
	public Vector2 dashRecoverSpeedMultiplier = new Vector2(1, 1.25f);
	public Vector2 ballSpeedMultiplier = new Vector2(1, 1.25f);
	public Vector2 energyGainMultiplier = new Vector2(1, 1.2f);
	public Vector2 ballDamageMultiplier = new Vector2(1, 1);
	public Vector2 enemySpeedMultiplier = new Vector2(1, 0.9f);
	public Vector2 enemySpawnRateMultiplier = new Vector2(1, 1);
	public Vector2 vibrationMultiplier = new Vector2(1, 1.25f);
	public Vector2 screenShakeMultiplier = new Vector2(1, 1.25f);

	[Separator("PostProcess settings")]
	public Vector2 minMaxBloom = new Vector2(1, 4);
	public Vector2 minMaxTemperature = new Vector2(0, 38);
	public Vector2 minMaxChromaticAberration = new Vector2(0, 0.1f);
    public Vector2 minMaxGrain = new Vector2(0, 0.17f);

	[Separator("Gain settings")]
	[Range(0f, 1f)] public float momentumGainedOnPass = 0.03f;
	[Range(0f, 1f)] public float momentumGainedOnHit = 0.02f;
	[Range(0f, 1f)] public float momentumGainedOnDunk = 0.1f;
	[Range(0f, 1f)] public float momentumGainedOnPerfectReception = 0.03f;

	[Separator("Losses settings")]
	public float minPassDelayBeforeMomentumLoss = 1;
	public float momentumLossSpeedIfNoPass = 1;
	[Range(0f, 1f)] public float momentumLossOnFightEnd = 1;
	[Range(0f, 1f)] public float momentumLossOnDamage = 0.1f;
	[Range(0f, 1f)] public float momentumLossWhenBallHitTheGround = 0.5f;
}