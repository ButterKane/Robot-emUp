using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;
public class MassagePrefab : MonoBehaviour
{
	public float massageForce;
	private void Update ()
	{
		GamePad.SetVibration(PlayerIndex.Two, massageForce, massageForce);
		GamePad.SetVibration(PlayerIndex.One, massageForce, massageForce);
	}
}
