using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : MonoBehaviour
{
	private LineRenderer lr;
	private Color defaultColor = Color.white;
	private Color activatedColor = Color.cyan;
	private void Awake ()
	{
		lr = GetComponent<LineRenderer>();
	}
	public void ActivateWire()
	{
		Debug.Log("")
	}
}
