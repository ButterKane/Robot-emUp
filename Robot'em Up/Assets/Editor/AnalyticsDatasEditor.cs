using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnalyticsDatas))]
public class AnalyticsDatasEditor : Editor
{
	AnalyticsDatas datas;

	/*
	 * 	public string dataName;
	public bool sortPerZone;
	public bool sortPerArena;
	public bool perPlayer;
	[ReadOnly] public int playerOneValue;
	[ReadOnly] public int playerTwoValue;
	[ReadOnly] public int totalValue;
	*/

	private void OnEnable ()
	{
		datas = (AnalyticsDatas)target; 
	}

	public override void OnInspectorGUI ()
	{
		EditorGUI.BeginChangeCheck();
		SerializedProperty m_activateDataRetrieving = serializedObject.FindProperty("activateDataRetrieving");
		EditorGUILayout.PropertyField(m_activateDataRetrieving);
		if (EditorGUI.EndChangeCheck())
		{
			//DisableHWStatistics
		}
	}
}
