using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

[CustomEditor(typeof(FeedbackDatas))]
public class FeedbackEditor : Editor
{
	FeedbackDatas feedbackDatas;


	public override void OnInspectorGUI ()
	{
		feedbackDatas = (FeedbackDatas)target;

		this.serializedObject.Update();

		GUIStyle headerStyle = new GUIStyle(EditorStyles.helpBox);
		headerStyle.alignment = TextAnchor.MiddleCenter;
		headerStyle.fontSize = 20;
		headerStyle.fontStyle = FontStyle.Bold;

		GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButton);
		buttonStyle.alignment = TextAnchor.MiddleCenter;
		buttonStyle.fontSize = 20;
		buttonStyle.fontStyle = FontStyle.Bold;

		GUILayout.BeginVertical(EditorStyles.helpBox);
		{
			GUI.color = Color.gray;
			GUILayout.Box("Global settings", headerStyle);
			GUILayout.Space(10);
			GUI.color = Color.white;
			GUILayout.Space(10);

			if (GUILayout.Button("Check for implemented events\n (May cause severe lag, save before)", buttonStyle, GUILayout.Height(100)))
			{
				foreach (FeedbackData feedbackData in feedbackDatas.feedbackList)
				{
					if (IsEventCalled(feedbackData.eventName))
					{
						feedbackData.eventCalled = true;
					} else
					{
						feedbackData.eventCalled = false;
					}
				}
			}
			//None yet
		}
		GUILayout.EndVertical();

		GUILayout.BeginVertical(EditorStyles.helpBox);
		{
			GUI.color = Color.gray;
			GUILayout.Box("Events", headerStyle);
			GUILayout.Space(10);
			GUI.color = Color.white;
			GUILayout.Space(10);

			for (int i = 0; i < feedbackDatas.feedbackList.Count; i++)
			{
				GUI.color = new Color(0.8f, 0.8f, 0.8f);
				GUILayout.BeginVertical(EditorStyles.helpBox);
				{
					FeedbackData feedbackData = feedbackDatas.feedbackList[i];
					GUILayout.Label(feedbackData.eventName, EditorStyles.centeredGreyMiniLabel);

					GUILayout.BeginHorizontal();
					feedbackData.eventName = EditorGUILayout.TextField(feedbackData.eventName);
					if (feedbackData.eventCalled)
					{
						EditorGUILayout.LabelField(EditorGUIUtility.IconContent("d_winbtn_mac_max"), GUILayout.Height(20), GUILayout.Width(20));
					} else
					{
						EditorGUILayout.LabelField(EditorGUIUtility.IconContent("d_winbtn_mac_close"), GUILayout.Height(20), GUILayout.Width(20));
					}
					GUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
					Rect scale = GUILayoutUtility.GetLastRect();

					EditorGUILayout.BeginHorizontal();
					{
						GUILayout.FlexibleSpace();
						GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(100), GUILayout.Width(EditorGUIUtility.currentViewWidth / 2 - 25));
						{
							if (!feedbackData.vibrationDataInited)
							{
								if (GUILayout.Button("Add vibrations", buttonStyle, GUILayout.Height(100)))
								{
									AddVibration(feedbackData);
								}
							} else
							{
								EditorGUILayout.BeginHorizontal();
								GUILayout.FlexibleSpace();
								GUILayout.Label("Vibration", EditorStyles.largeLabel);
								GUILayout.FlexibleSpace();
								EditorGUILayout.EndHorizontal();

								Rect rect = GUILayoutUtility.GetLastRect();
								Rect crossRect = new Rect(rect.x + (EditorGUIUtility.currentViewWidth / 2) - 55, rect.y, 20, 20);
								if (GUI.Button(crossRect, EditorGUIUtility.IconContent("winbtn_win_close")))
								{
									RemoveVibration(feedbackData);
								}

								EditorGUILayout.BeginHorizontal();
								SerializedProperty m_force = serializedObject.FindProperty("feedbackList.Array.data[" + i + "].vibrationData.force");
								GUILayout.Label("Force: ", GUILayout.Width(100));
								EditorGUILayout.PropertyField(m_force, GUIContent.none );
								EditorGUILayout.EndHorizontal();

								EditorGUILayout.BeginHorizontal();
								SerializedProperty m_duration = serializedObject.FindProperty("feedbackList.Array.data[" + i + "].vibrationData.duration");
								GUILayout.Label("Duration: ", GUILayout.Width(100));
								EditorGUILayout.PropertyField(m_duration, GUIContent.none);
								EditorGUILayout.EndHorizontal();

								EditorGUILayout.BeginHorizontal();
								SerializedProperty m_target = serializedObject.FindProperty("feedbackList.Array.data[" + i + "].vibrationData.target");
								GUILayout.Label("Target: ", GUILayout.Width(100));
								EditorGUILayout.PropertyField(m_target, GUIContent.none);
								EditorGUILayout.EndHorizontal();
							}
						}
						GUILayout.EndVertical();

						GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(100), GUILayout.Width(EditorGUIUtility.currentViewWidth / 2 - 25));
						{
							if (!feedbackData.shakeDataInited)
							{
								if (GUILayout.Button("Add screenShake", buttonStyle, GUILayout.Height(100)))
								{
									AddScreenshake(feedbackData);
								}
							} else
							{
								EditorGUILayout.BeginHorizontal();
								GUILayout.FlexibleSpace();
								GUILayout.Label("Screenshake", EditorStyles.largeLabel);
								GUILayout.FlexibleSpace();
								EditorGUILayout.EndHorizontal();

								Rect rect = GUILayoutUtility.GetLastRect();
								Rect crossRect = new Rect(rect.x + (EditorGUIUtility.currentViewWidth / 2) - 55, rect.y, 20, 20);
								if (GUI.Button(crossRect, EditorGUIUtility.IconContent("winbtn_win_close")))
								{
									RemoveScreenshake(feedbackData);
								}

								EditorGUILayout.BeginHorizontal();
								SerializedProperty m_duration = serializedObject.FindProperty("feedbackList.Array.data[" + i + "].shakeData.duration");
								GUILayout.Label("Duration: ", GUILayout.Width(100));
								EditorGUILayout.PropertyField(m_duration, GUIContent.none);
								EditorGUILayout.EndHorizontal();

								EditorGUILayout.BeginHorizontal();
								SerializedProperty m_intensity = serializedObject.FindProperty("feedbackList.Array.data[" + i + "].shakeData.intensity");
								GUILayout.Label("Force: ", GUILayout.Width(100));
								EditorGUILayout.PropertyField(m_intensity, GUIContent.none);
								EditorGUILayout.EndHorizontal();

								EditorGUILayout.BeginHorizontal();
								SerializedProperty m_frequency = serializedObject.FindProperty("feedbackList.Array.data[" + i + "].shakeData.frequency");
								GUILayout.Label("Frequency: ", GUILayout.Width(100));
								EditorGUILayout.PropertyField(m_frequency, GUIContent.none);
								EditorGUILayout.EndHorizontal();
							}
						}
						GUILayout.EndVertical();
						GUILayout.FlexibleSpace();
					}
					EditorGUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Delete", GUILayout.Width(100), GUILayout.Height(20)))
					{
						feedbackDatas.feedbackList.Remove(feedbackDatas.feedbackList[i]);
						return;
					}
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
					GUILayout.Space(10);

				}
				GUILayout.EndVertical();
				GUILayout.Space(10);
			}


			GUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Add event", GUILayout.Width(100), GUILayout.Height(30)))
				{
					AddEvent();
				}
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(10);
		}
		GUILayout.EndVertical();

		this.serializedObject.ApplyModifiedProperties();
		if (GUI.changed)
		{
			EditorUtility.SetDirty(target);
		}
	}

	public void AddEvent()
	{
		FeedbackData newFeedbackData = new FeedbackData();
		newFeedbackData.shakeData = null;
		newFeedbackData.vibrationData = null;
		newFeedbackData.eventName = "event.null (" + (feedbackDatas.feedbackList.Count + 1) + ")";
		feedbackDatas.feedbackList.Add(newFeedbackData);
	}

	void AddVibration(FeedbackData _data)
	{
		_data.vibrationDataInited = true;
	}

	void RemoveVibration ( FeedbackData _data )
	{
		_data.vibrationDataInited = false;
	}

	void AddScreenshake ( FeedbackData _data )
	{
		_data.shakeDataInited = true;
	}

	void RemoveScreenshake ( FeedbackData _data )
	{
		_data.shakeDataInited = false;
	}

	bool IsEventCalled(string _eventName)
	{
		bool stringFound = false;
		string[] assetPaths = AssetDatabase.GetAllAssetPaths();
		foreach (string assetPath in assetPaths)
		{
			if (assetPath.EndsWith(".cs"))
			{
				if (File.ReadAllText(assetPath).Contains(_eventName))
				{
					stringFound = true;
				}
			}
		}
		return stringFound;
	}
}
