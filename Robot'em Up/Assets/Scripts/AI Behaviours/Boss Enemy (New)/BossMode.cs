using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "BossMode", menuName = "GameDatas/Boss/Mode", order = 1)]
public class BossMode : ScriptableObject
{
	public BossMovementType movementType;
	public float movementSpeedMultiplier = 1;
	public BossRotationType rotationType;
	public float rotationSpeedMultiplier = 1;
	public List<ModeTransition> modeTransitions = new List<ModeTransition>();
	public List<string> actionsOnStart = new List<string>();
	public List<string> actionsOnMovementEnd = new List<string>();
	public List<string> actionsOnEnd = new List<string>();
}

public enum BossMovementType
{
	FollowNearestPlayer, GoToCenter, DontMove
}

public enum BossRotationType
{
	LookNearestPlayer, LookCenter, None
}

public enum ModeTransitionConditionType
{
	TimeSinceModeIsEnabledGreaterThan, PlayerDistanceGreaterThan, PlayerDistanceLessThan, HPInferiorInferiorOrEqualTo, NoWallLeft, WeakPointsActivated, HitByDunk, SecondPhaseEnabled, FirstPhaseEnabled
}

[System.Serializable]
public class ModeTransition
{
	public List<ModeTransitionCondition> transitionConditions = new List<ModeTransitionCondition>();
	public List<BossModeTransitionChances> modeToActivate = new List<BossModeTransitionChances>();
}

[System.Serializable]
public class ModeTransitionCondition
{
	public ModeTransitionConditionType modeTransitionConditionType;
	public float modeTransitionConditionValue;
}

[System.Serializable]
public class BossModeTransitionChances
{
	public BossMode mode;
	public int chances = 100;
}
