using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Wire))]
public class WireEditor : Editor
{
	Wire wire;
	LineRenderer lr;

	private void OnEnable ()
	{
		wire = (Wire)target;
		lr = wire.GetComponent<LineRenderer>();
	}

	private void OnSceneGUI ()
	{
		for (int i = 0; i < lr.positionCount; i++)
		{
			EditorGUI.BeginChangeCheck();
			Vector3 newTargetPosition = Handles.PositionHandle(lr.GetPosition(i), Quaternion.identity);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(lr, "Change Position");
				lr.SetPosition(i, newTargetPosition);
			}
		}
	}
}
