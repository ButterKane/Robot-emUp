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
		this.serializedObject.Update();
		for (int i = 0; i < datas.analyticsDatas.Count; i++)
		{
			AnalyticsData data = datas.analyticsDatas[i];
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			{

				EditorGUILayout.BeginHorizontal();
				SerializedProperty m_force = serializedObject.FindProperty("analyticsDatas.Array.data[" + i + "].dataName");
				GUILayout.Label("Name: ", GUILayout.Width(100));
				EditorGUILayout.PropertyField(m_force, GUIContent.none);

				if (GUILayout.Button(EditorGUIUtility.IconContent("winbtn_win_close"), GUILayout.Width(20), GUILayout.Height(20)))
				{
					RemoveData(data);
					break;
				}

				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				SerializedProperty m_sortPerZone = serializedObject.FindProperty("analyticsDatas.Array.data[" + i + "].sortPerZone");
				GUILayout.Label("Sort per zone?: ", GUILayout.Width(100));
				EditorGUILayout.PropertyField(m_sortPerZone, GUIContent.none);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				SerializedProperty m_perPlayer = serializedObject.FindProperty("analyticsDatas.Array.data[" + i + "].perPlayer");
				GUILayout.Label("Sort per player?: ", GUILayout.Width(100));
				EditorGUILayout.PropertyField(m_perPlayer, GUIContent.none);
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(10);

				if (datas.analyticsDatas[i].perPlayer)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("PlayerOneValue: " + data.playerOneValue, EditorStyles.boldLabel);
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("PlayerTwoValue: " + data.playerTwoValue, EditorStyles.boldLabel);
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("Total value: " + data.totalValue, EditorStyles.boldLabel);
					EditorGUILayout.EndHorizontal();
				} else
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("Value: " + data.totalValue, EditorStyles.boldLabel);
					EditorGUILayout.EndHorizontal();
				}

			}
			EditorGUILayout.EndVertical();
			serializedObject.ApplyModifiedProperties();
		}

		if (GUILayout.Button("Add new data"))
		{
			AddData();
		}

		if (GUILayout.Button("Reset datas"))
		{
			//AnalyticsManager.CleanDatas();
		}
	}

	public void RemoveData(AnalyticsData _data)
	{
		datas.analyticsDatas.Remove(_data);
	}

	public void AddData()
	{
		datas.analyticsDatas.Add(new AnalyticsData());
	}
}
