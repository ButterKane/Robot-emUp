using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PawnStates))]
public class PawnStatesEditor : Editor
{
	PawnStates pawnStatesParent;

	private void OnEnable ()
	{
		pawnStatesParent = (PawnStates)target;
	}

	public override void OnInspectorGUI ()
	{
		serializedObject.Update();
		for (int i = 0; i < pawnStatesParent.pawnStates.Count; i++)
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.BeginVertical(EditorStyles.helpBox);
				{
					//Pawn state editor
					SerializedProperty m_name = serializedObject.FindProperty("pawnStates.Array.data[" + i + "].name");
					EditorGUILayout.PropertyField(m_name);

					SerializedProperty m_invincibleDuringState = serializedObject.FindProperty("pawnStates.Array.data[" + i + "].invincibleDuringState");
					EditorGUILayout.PropertyField(m_invincibleDuringState);
					SerializedProperty m_damagesCancelState = serializedObject.FindProperty("pawnStates.Array.data[" + i + "].damagesCancelState");
					EditorGUILayout.PropertyField(m_damagesCancelState);
					SerializedProperty m_allowBallReception = serializedObject.FindProperty("pawnStates.Array.data[" + i + "].allowBallReception");
					EditorGUILayout.PropertyField(m_allowBallReception);
					SerializedProperty m_allowBallThrow = serializedObject.FindProperty("pawnStates.Array.data[" + i + "].allowBallThrow");
					EditorGUILayout.PropertyField(m_allowBallThrow);
					SerializedProperty m_preventMoving = serializedObject.FindProperty("pawnStates.Array.data[" + i + "].preventMoving");
					EditorGUILayout.PropertyField(m_preventMoving);

					EditorGUILayout.BeginVertical(EditorStyles.helpBox);
					EditorGUILayout.LabelField("Can override: ", EditorStyles.boldLabel);
					{
						for (int y = 0; y < pawnStatesParent.pawnStates.Count; y++)
						{
							bool toggleState = false;
							if (pawnStatesParent.pawnStates[i].stateOverriden.Contains(pawnStatesParent.pawnStates[y].name))
							{
								toggleState = true;
							}
							EditorGUI.BeginChangeCheck();
							GUILayout.Toggle(toggleState, pawnStatesParent.pawnStates[y].name);
							if (EditorGUI.EndChangeCheck())
							{
								if (toggleState)
								{
									pawnStatesParent.pawnStates[i].stateOverriden.Remove(pawnStatesParent.pawnStates[y].name);
								} else
								{
									pawnStatesParent.pawnStates[i].stateOverriden.Add(pawnStatesParent.pawnStates[y].name);
								}
							}
						}
					}
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndVertical();
			}
			if (GUILayout.Button(EditorGUIUtility.IconContent("winbtn_win_close"), GUILayout.Width(80), GUILayout.ExpandHeight(true)))
			{
				pawnStatesParent.pawnStates.RemoveAt(i);
			}
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("+", GUILayout.Width(30), GUILayout.Height(30)))
		{
			pawnStatesParent.pawnStates.Add(new PawnState());
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		EditorUtility.SetDirty(pawnStatesParent);
	}
}
