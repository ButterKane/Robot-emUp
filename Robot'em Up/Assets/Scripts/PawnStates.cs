using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StateInteractions", menuName = "GlobalDatas/StateInteractions")]
public class PawnStates : MonoBehaviour
{
	public List<PawnState> pawnStates;
}

public class PawnState
{
	public string name;

	public List<PawnState> stateOverriden;
	public List<PawnState> overridenStates;
	public PawnStateInterruptionFunction interruptionFunction;

	public bool invincibleDuringState;
	public bool damagesCancelState;
	public bool allowBallReception;
	public bool allowBallThrow;
}

public class PawnStateInterruptionFunction
{
	public object scriptType;
	public string functionName;
}
