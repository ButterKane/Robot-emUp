using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

[CustomEditor(typeof(BossMode))]
public class BossModeEditor : Editor
{
	private BossMode bossMode;
	List<string> bossFunctions;

	private void OnEnable ()
	{
		bossMode = (BossMode)target;
		bossFunctions = new List<string>();
		bossFunctions =
			typeof(BossBehaviour)
			.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) // Instance methods, both public and private/protected
			.Where(x => x.DeclaringType == typeof(BossBehaviour)) // Only list methods defined in our own class
			.Where(x => x.GetParameters().Length == 0) // Make sure we only get methods with zero argumenrts
			.Select(x => x.Name)
			.ToArray().ToList();

	}

	public override void OnInspectorGUI ()
	{
		serializedObject.Update();
		SerializedProperty m_movementType = serializedObject.FindProperty("movementType");
		EditorGUILayout.PropertyField(m_movementType);

		SerializedProperty m_movementSpeedMultiplier = serializedObject.FindProperty("movementSpeedMultiplier");
		EditorGUILayout.PropertyField(m_movementSpeedMultiplier);

		SerializedProperty m_rotationType = serializedObject.FindProperty("rotationType");
		EditorGUILayout.PropertyField(m_rotationType);

		SerializedProperty m_rotationSpeedMultiplier = serializedObject.FindProperty("rotationSpeedMultiplier");
		EditorGUILayout.PropertyField(m_rotationSpeedMultiplier);

		//-------------- ON START EVENTS ------------------//
		GUILayout.BeginVertical(EditorStyles.helpBox);
		GUILayout.Label("On start events", EditorStyles.centeredGreyMiniLabel);
		GUI.backgroundColor = Color.grey;
		for (int i = 0; i < bossMode.actionsOnStart.Count; i++)
		{
			GUILayout.BeginHorizontal();
			int methodIndex = EditorGUILayout.Popup(GetMethodIndex(bossMode.actionsOnStart[i]), bossFunctions.ToArray());
			bossMode.actionsOnStart[i] = bossFunctions[methodIndex].ToString();
			if (GUILayout.Button(EditorGUIUtility.IconContent("winbtn_win_close"), GUILayout.Width(20), GUILayout.Height(20)))
			{
				RemoveActionOnStart(bossMode.actionsOnStart[i]);
			}
			GUILayout.EndHorizontal();
		}
		GUI.backgroundColor = Color.white;
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
		{
			AddActionOnStart();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();

		//-------------- ON MOVEMENT END EVENTS ------------------//
		bool canExecuteActionOnMovementEnd = true;
		switch (bossMode.movementType)
		{
			case BossMovementType.DontMove:
				canExecuteActionOnMovementEnd = false;
				break;
		}
		if (canExecuteActionOnMovementEnd)
		{
			GUILayout.BeginVertical(EditorStyles.helpBox);
			GUILayout.Label("On movement end events", EditorStyles.centeredGreyMiniLabel);
			GUI.backgroundColor = Color.grey;
			for (int i = 0; i < bossMode.actionsOnMovementEnd.Count; i++)
			{
				GUILayout.BeginHorizontal();
				int methodIndex = EditorGUILayout.Popup(GetMethodIndex(bossMode.actionsOnMovementEnd[i]), bossFunctions.ToArray());
				bossMode.actionsOnMovementEnd[i] = bossFunctions[methodIndex];
				if (GUILayout.Button(EditorGUIUtility.IconContent("winbtn_win_close"), GUILayout.Width(20), GUILayout.Height(20)))
				{
					RemoveActionOnMovementEnd(bossMode.actionsOnMovementEnd[i]);
				}
				GUILayout.EndHorizontal();
			}
			GUI.backgroundColor = Color.white;
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
			{
				AddActionOnMovementEnd();
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}

		//-------------- ON END EVENTS ------------------//
		GUILayout.BeginVertical(EditorStyles.helpBox);
		GUILayout.Label("On end events", EditorStyles.centeredGreyMiniLabel);
		GUI.backgroundColor = Color.grey;
		for (int i = 0; i < bossMode.actionsOnEnd.Count; i++)
		{
			GUILayout.BeginHorizontal();
			int methodIndex = EditorGUILayout.Popup(GetMethodIndex(bossMode.actionsOnEnd[i]), bossFunctions.ToArray());
			bossMode.actionsOnEnd[i] = bossFunctions[methodIndex];
			if (GUILayout.Button(EditorGUIUtility.IconContent("winbtn_win_close"), GUILayout.Width(20), GUILayout.Height(20)))
			{
				RemoveActionOnEnd(bossMode.actionsOnEnd[i]);
			}
			GUILayout.EndHorizontal();
		}
		GUI.backgroundColor = Color.white;
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
		{
			AddActionOnEnd();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();



		//-------------- END CONDITIONS ------------------//
		GUILayout.BeginVertical(EditorStyles.helpBox);
		{
			EditorGUIUtility.labelWidth = 75;
			GUILayout.Label("End conditions", EditorStyles.centeredGreyMiniLabel);
			for (int i = 0; i < bossMode.modeTransitions.Count; i++)
			{
				GUILayout.BeginHorizontal();
				{
					GUI.backgroundColor = Color.grey;
					Rect r = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
					{
						GUILayout.Space(10);
							GUILayout.BeginHorizontal();
							GUILayout.FlexibleSpace();
							GUILayout.Label("IF", EditorStyles.boldLabel);
							GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();
						for (int y = 0; y < bossMode.modeTransitions[i].transitionConditions.Count; y++)
						{
							GUILayout.BeginHorizontal();
							{
								SerializedProperty m_transitionConditionType = serializedObject.FindProperty("modeTransitions.Array.data[" + i + "].transitionConditions.Array.data[" + y + "].modeTransitionConditionType");
								EditorGUILayout.PropertyField(m_transitionConditionType, GUIContent.none);
								bool showAmount = false;
								switch (bossMode.modeTransitions[i].transitionConditions[y].modeTransitionConditionType)
								{
									case ModeTransitionConditionType.TimeSinceModeIsEnabledGreaterThan:
										showAmount = true;
										break;
									case ModeTransitionConditionType.HPInferiorInferiorOrEqualTo:
										showAmount = true;
										break;
									case ModeTransitionConditionType.PlayerDistanceGreaterThan:
										showAmount = true;
										break;
									case ModeTransitionConditionType.PlayerDistanceLessThan:
										showAmount = true;
										break;
								}
								if (showAmount)
								{
									SerializedProperty m_modeTransitionConditionValue = serializedObject.FindProperty("modeTransitions.Array.data[" + i + "].transitionConditions.Array.data[" + y + "].modeTransitionConditionValue");
									EditorGUILayout.PropertyField(m_modeTransitionConditionValue, GUIContent.none);
								}
								if (GUILayout.Button(EditorGUIUtility.IconContent("winbtn_win_close"), GUILayout.Width(20), GUILayout.Height(20)))
								{
									bossMode.modeTransitions[i].transitionConditions.Remove(bossMode.modeTransitions[i].transitionConditions[y]);
								}
							}
							GUILayout.EndHorizontal();
							if (y < bossMode.modeTransitions[i].transitionConditions.Count - 1)
							{
								GUILayout.BeginHorizontal();
									GUILayout.FlexibleSpace();
									GUILayout.Label("AND", EditorStyles.boldLabel);
									GUILayout.FlexibleSpace();
								GUILayout.EndHorizontal();
							}
						}
						GUILayout.BeginHorizontal();
						{
							GUILayout.FlexibleSpace();
							if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
							{
								bossMode.modeTransitions[i].transitionConditions.Add(new ModeTransitionCondition());
							}
							GUILayout.FlexibleSpace();
						}
						GUILayout.EndHorizontal();
						GUILayout.Space(15);
						GUILayout.BeginHorizontal();
							GUILayout.FlexibleSpace();
							GUILayout.Label("THEN CHANGE MODE TO:", EditorStyles.boldLabel);
							GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();
						for (int y = 0; y < bossMode.modeTransitions[i].modeToActivate.Count; y++)
						{
							if (y > 0 && y < bossMode.modeTransitions[i].modeToActivate.Count)
							{
								GUILayout.BeginHorizontal();
									GUILayout.FlexibleSpace();
									GUILayout.Label("OR:", EditorStyles.boldLabel);
									GUILayout.FlexibleSpace();
								GUILayout.EndHorizontal();
							}
							GUILayout.BeginHorizontal();
								SerializedProperty m_modeToActivate = serializedObject.FindProperty("modeTransitions.Array.data[" + i + "].modeToActivate.Array.data[" + y + "].mode");
								EditorGUILayout.PropertyField(m_modeToActivate, GUIContent.none, true, GUILayout.MinWidth(100));

								SerializedProperty m_modeChances = serializedObject.FindProperty("modeTransitions.Array.data[" + i + "].modeToActivate.Array.data[" + y + "].chances");
								EditorGUILayout.PropertyField(m_modeChances, new GUIContent("% Chances:"), true, GUILayout.MinWidth(50));

								if (GUILayout.Button(EditorGUIUtility.IconContent("winbtn_win_close"), GUILayout.Width(20), GUILayout.Height(20)))
								{
									bossMode.modeTransitions[i].modeToActivate.Remove(bossMode.modeTransitions[i].modeToActivate[y]);
								}
							GUILayout.EndHorizontal();
						}
						GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
						{
							bossMode.modeTransitions[i].modeToActivate.Add(new BossModeTransitionChances());
						}
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();
					}
					EditorGUILayout.EndVertical();
					GUI.backgroundColor = Color.white;
					if (GUILayout.Button(EditorGUIUtility.IconContent("winbtn_win_close"), GUILayout.Width(80), GUILayout.ExpandHeight(true)))
					{
						RemoveEndCondition(bossMode.modeTransitions[i]);
					}
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
			{
				AddEndCondition();
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();


		EditorUtility.SetDirty(bossMode);
		serializedObject.ApplyModifiedProperties();
	}

	void AddActionOnStart ()
	{
		bossMode.actionsOnStart.Add(bossFunctions[0]);
		serializedObject.ApplyModifiedProperties();
		EditorUtility.SetDirty(bossMode);

	}

	void RemoveActionOnStart ( string _method )
	{
		bossMode.actionsOnStart.Remove(_method);
	}

	void AddActionOnMovementEnd()
	{
		bossMode.actionsOnMovementEnd.Add(bossFunctions[0]);
	}
	void RemoveActionOnMovementEnd ( string _method )
	{
		bossMode.actionsOnMovementEnd.Remove(_method);
	}

	void AddActionOnEnd ()
	{
		bossMode.actionsOnEnd.Add(bossFunctions[0]);
	}
	void RemoveActionOnEnd ( string _method )
	{
		bossMode.actionsOnEnd.Remove(_method);
	}

	void AddEndCondition()
	{
		bossMode.modeTransitions.Add(new ModeTransition());
	}

	void RemoveEndCondition(ModeTransition _mt)
	{
		bossMode.modeTransitions.Remove(_mt);
	}

	int GetMethodIndex (string _method )
	{
		for (int i = 0; i < bossFunctions.Count; i++)
		{
			if (_method == bossFunctions[i])
			{
				return i;
			}
		}
		return 0;
	}
}