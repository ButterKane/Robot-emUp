using UnityEngine;
using MyBox;

[CreateAssetMenu(fileName = "MomentumData", menuName = "GlobalDatas/Momentum", order = 1)]
public class MomentumData : ScriptableObject
{
	[Separator("Global settings")]
	public float momentumLerpSpeed;

	[Separator("Multiplier settings")]
	[MinMaxRange(0f, 5f)] public RangedFloat playerSpeedMultiplier;
	[MinMaxRange(0f, 5f)] public RangedFloat dashRecoverSpeedMultiplier;
	[MinMaxRange(0f, 5f)] public RangedFloat ballSpeedMultiplier;
	[MinMaxRange(0f, 5f)] public RangedFloat energyGainMultiplier;
	[MinMaxRange(0f, 5f)] public RangedFloat ballDamageMultiplier;
	[MinMaxRange(0f, 5f)] public RangedFloat enemySpeedMultiplier;
	[MinMaxRange(0f, 5f)] public RangedFloat enemySpawnRateMultiplier;

	[Separator("Momentum gains")]
	[Range(0f, 1f)] public float momentumGainedOnPass;
	[Range(0f, 1f)] public float momentumGainedOnHit;
	[Range(0f, 1f)] public float momentumGainedOnDunk;
	[Range(0f, 1f)] public float momentumGainedOnPerfectReception;

	[Separator("Momentum losses")]
	public float minPassDelayBeforeMomentumLoss;
	public float momentumLossSpeedIfNoPass;
	[Range(0f, 1f)] public float momentumLossOnFightEnd;
	[Range(0f, 1f)] public float momentumLossOnDamage;
	[Range(0f, 1f)] public float momentumLossWhenBallHitTheGround;
}