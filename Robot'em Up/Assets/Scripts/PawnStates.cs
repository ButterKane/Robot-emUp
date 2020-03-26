using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "StateInteractions", menuName = "GlobalDatas/StateInteractions")]
public class PawnStates : ScriptableObject
{
	public List<PawnState> pawnStates;

	public PawnState GetPawnStateByName(string _name)
	{
		foreach (PawnState _ps in pawnStates)
		{
			if (_ps.name == _name)
			{
				return _ps;
			}
		}
		return null;
	}

	public bool IsStateOverriden(PawnState currentState, PawnState newState)
	{
		foreach (string s in newState.stateOverriden)
		{
			if (s == currentState.name)
			{
				return true;
			}
		}
		return false;
	}
}

[System.Serializable]
public class PawnState
{
	public string name = "New state";

	public List<string> stateOverriden = new List<string>();
	public bool invincibleDuringState;
	public bool damagesCancelState;
	public bool allowBallReception;
	public bool allowBallThrow;
	public bool preventMoving;
}
