using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpawnStartPosition))]
public class SpawnStartPositionEditor : Editor
{
	SpawnStartPosition parent;

	private void Awake ()
	{
		parent = (SpawnStartPosition)target;
	}
	private void OnSceneGUI ()
	{
		parent.linkedSpawner.RecalculateEndspawnLocation();
	}
}

