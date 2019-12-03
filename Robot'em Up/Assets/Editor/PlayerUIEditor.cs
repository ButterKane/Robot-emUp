using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerUI))]
public class PlayerUIEditor : Editor
{
	PlayerUI playerUI;

	private void OnEnable ()
	{
		playerUI = (PlayerUI)target;
	}

	public override void OnInspectorGUI ()
	{

	}
}
