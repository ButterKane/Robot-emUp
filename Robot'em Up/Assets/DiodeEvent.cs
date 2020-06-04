using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiodeEvent : MonoBehaviour
{
	public void ActivateDiodeByID(int ID)
	{
		DiodeManager.ActivateDiodes(ID);
	}

	public void ActivateNeonByID(int ID)
	{
		DiodeManager.ActivateNeons(ID);
	}
}
