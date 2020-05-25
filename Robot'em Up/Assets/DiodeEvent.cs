using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiodeEvent : MonoBehaviour
{
	public void ActivateDiodeByID(int ID)
	{
		DiodeManager.ActivateDiodes(ID);
	}
}
