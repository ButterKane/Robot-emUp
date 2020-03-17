using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PushForce { Force1, Force2, Force3 }
public enum BumpForce { Force1, Force2, Force3 }
public enum WallSplatForce { Heavy, Light }

[CreateAssetMenu(fileName = "PushDatas", menuName = "GlobalDatas/PushDatas")]
public class PushDatas : ScriptableObject
{
	public AnimationCurve lightPushSpeedCurve;
	public float lightPushAircontrolSpeed;

	public AnimationCurve heavyPushSpeedCurve;
	public float heavyPushAirControlSpeed;

	[Header("Bump & push global settings")]
	public float bumpRestDuration;
	public float bumpPlayerRestDuration;
	public Vector2 bumpRandomRangeModifier;
	public Vector2 bumpRandomDurationModifier;
	public Vector2 bumpRandomRestModifier;

	[Header("Light push settings (DEFAULT)")]
	public float lightPushForce1Distance;
	public float lightPushForce1Height;
	public float lightPushForce1Duration;

	public float lightPushForce2Distance;
	public float lightPushForce2Height;
	public float lightPushForce2Duration;

	public float lightPushForce3Distance;
	public float lightPushForce3Height;
	public float lightPushForce3Duration;

	[Header("Light push settings (PLAYER)")]
	public float lightPushPlayerForce1Distance;
	public float lightPushPlayerForce1Height;
	public float lightPushPlayerForce1Duration;

	public float lightPushPlayerForce2Distance;
	public float lightPushPlayerForce2Height;
	public float lightPushPlayerForce2Duration;

	public float lightPushPlayerForce3Distance;
	public float lightPushPlayerForce3Height;
	public float lightPushPlayerForce3Duration;

	[Header("Heavy push settings (DEFAULT)")]
	public float heavyPushForce1Distance;
	public float heavyPushForce1Height;
	public float heavyPushForce1Duration;

	public float heavyPushForce2Distance;
	public float heavyPushForce2Height;
	public float heavyPushForce2Duration;

	public float heavyPushForce3Distance;
	public float heavyPushForce3Height;
	public float heavyPushForce3Duration;


	[Header("Heavy push settings (PLAYER)")]
	public float heavyPushPlayerForce1Distance;
	public float heavyPushPlayerForce1Height;
	public float heavyPushPlayerForce1Duration;

	public float heavyPushPlayerForce2Distance;
	public float heavyPushPlayerForce2Height;
	public float heavyPushPlayerForce2Duration;

	public float heavyPushPlayerForce3Distance;
	public float heavyPushPlayerForce3Height;
	public float heavyPushPlayerForce3Duration;


	[Header("Bump settings (DEFAULT)")]
	public float BumpForce1Distance;
	public float BumpForce1Duration;

	public float BumpForce2Distance;
	public float BumpForce2Duration;

	public float BumpForce3Distance;
	public float BumpForce3Duration;


	[Header("Bump settings (PLAYER)")]
	public float BumpPlayerForce1Distance;
	public float BumpPlayerForce1Duration;

	public float BumpPlayerForce2Distance;
	public float BumpPlayerForce2Duration;

	public float BumpPlayerForce3Distance;
	public float BumpPlayerForce3Duration;


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

	[Header("WallSplat Player")]
	public float wallSplatPlayerDamages;
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
