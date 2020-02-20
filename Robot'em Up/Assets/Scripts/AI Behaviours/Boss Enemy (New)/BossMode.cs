using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "BossMode", menuName = "GameDatas/Boss/Mode", order = 1)]
public class BossMode : ScriptableObject
{
	public BossMovementType movementType;
	public BossRotationType rotationType;
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
	TimeSinceModeIsEnabledGreaterThan, PlayerDistanceGreaterThan, PlayerDistanceLessThan, HPInferiorInferiorOrEqualTo, NoWallLeft, WeakPointsActivated, HitByDunk
}

[System.Serializable]
public class ModeTransition
{
	public List<ModeTransitionCondition> transitionConditions = new List<ModeTransitionCondition>();
	public BossMode modeToActivate;
}

[System.Serializable]
public class ModeTransitionCondition
{
	public ModeTransitionConditionType modeTransitionConditionType;
	public float modeTransitionConditionValue;
}
