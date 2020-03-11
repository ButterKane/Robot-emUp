using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PushDatas", menuName = "GlobalDatas/PushDatas")]
public class PushDatas : ScriptableObject
{
	public AnimationCurve lightPushSpeedCurve;
	public float lightPushAircontrolSpeed;

	public AnimationCurve heavyPushSpeedCurve;
	public float heavyPushAirControlSpeed;

	[Header("Bump settings")]
	public float bumpRestDuration;
	public float bumpPlayerRestDuration;
	public Vector2 bumpRandomRangeModifier;
	public Vector2 bumpRandomDurationModifier;
	public Vector2 bumpRandomRestModifier;

	[Header("WallSplat Default")]
	public float wallSplatHeavyForwardPush = 3f;
	public float wallSplatLightRecoverTime = 0.5f;
	public float wallSplatHeavyRecoverTime = 1f;
	public float wallSplatHeavyFallSpeed = 10f;
	public AnimationCurve wallSplatHeavyHeightCurve;
	public AnimationCurve wallSplatHeavySpeedCurve;
	public Vector2 randomWallSplatHeavyRecoverTimeAddition;
	public Vector2 randomWallSplatLightRecoverTimeAddition;
	public float wallSplatDamages;
	public float wallSplatPlayerDamages;

	[Header("WallSplat Player")]
	public float wallSplatPlayerHeavyForwardPush = 3f;
	public float wallSplatPlayerLightRecoverTime = 0.5f;
	public float wallSplatPlayerHeavyRecoverTime = 1f;
	public float wallSplatPlayerHeavyFallSpeed = 10f;
	public AnimationCurve wallSplatPlayerHeavyHeightCurve;
	public AnimationCurve wallSplatPlayerHeavySpeedCurve;

	public static PushDatas GetDatas()
	{
		return Resources.Load<PushDatas>("PushDatas");
	}
}
