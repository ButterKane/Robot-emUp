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
	public List<bool> showPosition;
	string[] categoryOptions;
	int selectedCategoryIndex;
	List<string> soundList;

	private void OnEnable ()
	{
		feedbackDatas = (FeedbackDatas)target;
		showPosition = new List<bool>();
		for (int i = 0; i < feedbackDatas.feedbackList.Count; i++)
		{
			showPosition.Add(false);
		}
		selectedCategoryIndex = feedbackDatas.feedbackCategories.Count;
		RecalculateCategoryOptions();
		RecalculateSoundList();
	}

	void RecalculateSoundList()
	{
		soundList = new List<string>();
		foreach (SoundData sound in Resources.Load<SoundDatas>("SoundDatas").soundList)
		{
			soundList.Add(sound.soundName);
		}
	}
	
	int GetCategoryIndex(FeedbackEventCategory _category)
	{
		for (int i =0; i < feedbackDatas.feedbackCategories.Count; i++)
		{
			if (_category.displayName == feedbackDatas.feedbackCategories[i].displayName)
			{
				return i;
			}
		}
		return -1;
	}
	void RecalculateCategoryOptions()
	{
		List<string> i_categoryOptionsList = new List<string>();
		for (int i = 0; i < feedbackDatas.feedbackCategories.Count; i++)
		{
			i_categoryOptionsList.Add(feedbackDatas.feedbackCategories[i].displayName);
		}
		categoryOptions = i_categoryOptionsList.ToArray();
		return;
	}
	public override void OnInspectorGUI ()
	{
		feedbackDatas = (FeedbackDatas)target;

		this.serializedObject.Update();

		GUIStyle i_headerStyle = new GUIStyle(EditorStyles.helpBox);
		i_headerStyle.alignment = TextAnchor.MiddleCenter;
		i_headerStyle.fontSize = 20;
		i_headerStyle.fontStyle = FontStyle.Bold;

		GUIStyle i_buttonStyle = new GUIStyle(EditorStyles.miniButton);
		i_buttonStyle.alignment = TextAnchor.MiddleCenter;
		i_buttonStyle.fontSize = 20;
		i_buttonStyle.fontStyle = FontStyle.Bold;

		GUILayout.BeginVertical(EditorStyles.helpBox);
		{
			GUI.color = Color.gray;
			GUILayout.Box("Global settings", i_headerStyle);
			GUILayout.Space(10);
			GUI.color = Color.white;
			GUILayout.Space(10);

			if (GUILayout.Button("Check for implemented events\n (May cause severe lag, save before)", i_buttonStyle, GUILayout.Height(100)))
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
			GUILayout.Box("Events", i_headerStyle);
			GUILayout.Space(10);
			GUI.color = Color.white;
			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Display events of category: ", EditorStyles.largeLabel);
			List<string> i_categoryOptionsWithAll = categoryOptions.ToList();
			i_categoryOptionsWithAll.Add("All");

			selectedCategoryIndex = EditorGUILayout.Popup(selectedCategoryIndex, i_categoryOptionsWithAll.ToArray());

			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			for (int i = 0; i < feedbackDatas.feedbackList.Count; i++)
			{
				if (selectedCategoryIndex < feedbackDatas.feedbackCategories.Count && feedbackDatas.feedbackCategories[selectedCategoryIndex] != feedbackDatas.feedbackList[i].category) { continue; }
				GUI.color = new Color(0.8f, 0.8f, 0.8f);
				FeedbackData i_feedbackData = feedbackDatas.feedbackList[i];
				GUILayout.BeginVertical(EditorStyles.helpBox);
					{
					GUILayout.BeginHorizontal();
					showPosition[i] = EditorGUILayout.Foldout(showPosition[i], feedbackDatas.feedbackList[i].eventName);
					GUI.color = i_feedbackData.category.displayColor;
					int index = EditorGUILayout.Popup(GetCategoryIndex(feedbackDatas.feedbackList[i].category), categoryOptions);
					if (index != -1)
					{
						feedbackDatas.feedbackList[i].category = feedbackDatas.feedbackCategories[index];
					}
					GUI.color = new Color(0.8f, 0.8f, 0.8f);
					if (i_feedbackData.eventCalled)
					{
						EditorGUILayout.LabelField(EditorGUIUtility.IconContent("d_winbtn_mac_max"), GUILayout.Height(20), GUILayout.Width(20));
					}
					else
					{
						EditorGUILayout.LabelField(EditorGUIUtility.IconContent("d_winbtn_mac_close"), GUILayout.Height(20), GUILayout.Width(20));
					}

					GUILayout.EndHorizontal();
					if (showPosition[i])
					{

						GUILayout.BeginHorizontal();
						i_feedbackData.eventName = EditorGUILayout.TextField(i_feedbackData.eventName);
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
								if (!i_feedbackData.vibrationDataInited)
								{
									if (GUILayout.Button("Add vibrations", i_buttonStyle, GUILayout.Height(100)))
									{
										AddVibration(i_feedbackData);
									}
								}
								else
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
										RemoveVibration(i_feedbackData);
									}

									EditorGUILayout.BeginHorizontal();
									SerializedProperty m_force = serializedObject.FindProperty("feedbackList.Array.data[" + i + "].vibrationData.force");
									GUILayout.Label("Force: ", GUILayout.Width(100));
									EditorGUILayout.PropertyField(m_force, GUIContent.none);
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
								if (!i_feedbackData.shakeDataInited)
								{
									if (GUILayout.Button("Add screenShake", i_buttonStyle, GUILayout.Height(100)))
									{
										AddScreenshake(i_feedbackData);
									}
								}
								else
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
										RemoveScreenshake(i_feedbackData);
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

						GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(100), GUILayout.Width(EditorGUIUtility.currentViewWidth - 50));
						{
							if (!i_feedbackData.soundDataInited)
							{
								if (GUILayout.Button("Add sound", i_buttonStyle, GUILayout.Height(100)))
								{
									AddSound(i_feedbackData);
								}
							}
							else
							{
								EditorGUILayout.BeginHorizontal();
								GUILayout.FlexibleSpace();
								GUILayout.Label("Sound", EditorStyles.largeLabel);
								GUILayout.FlexibleSpace();
								EditorGUILayout.EndHorizontal();

								Rect rect = GUILayoutUtility.GetLastRect();
								Rect crossRect = new Rect(rect.x + EditorGUIUtility.currentViewWidth - 85, rect.y, 20, 20);
								if (GUI.Button(crossRect, EditorGUIUtility.IconContent("winbtn_win_close")))
								{
									RemoveSound(i_feedbackData);
								}
								EditorGUILayout.BeginHorizontal();
								{
									GUILayout.Label("Sound Name: ", GUILayout.Width(100));
									i_feedbackData.soundData.soundName = EditorExtend.TextFieldAutoComplete(i_feedbackData.soundData.soundName, soundList.ToArray(), maxShownCount: 10, levenshteinDistance: 0.5f);
								}
								EditorGUILayout.EndHorizontal();
								EditorGUILayout.BeginHorizontal();
								{
									GUILayout.Label("Attach to target: ", GUILayout.Width(100));
									i_feedbackData.soundData.attachToTarget = EditorGUILayout.Toggle(i_feedbackData.soundData.attachToTarget);
								}
								EditorGUILayout.EndHorizontal();
							}
						}
						GUILayout.EndVertical();

						/*	public GameObject vfxPrefab;
	public Vector3 offset;
	public Vector3 scaleMultiplier;
	public VFXDirection direction;
	public float spawnDelay;
	public bool attachToTarget;
	*/

						GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(100), GUILayout.Width(EditorGUIUtility.currentViewWidth - 50));
						{
							if (!i_feedbackData.vfxDataInited)
							{
								if (GUILayout.Button("Add VFX", i_buttonStyle, GUILayout.Height(100)))
								{
									AddVFX(i_feedbackData);
								}
							}
							else
							{
								EditorGUILayout.BeginHorizontal();
								GUILayout.FlexibleSpace();
								GUILayout.Label("VFX", EditorStyles.largeLabel);
								GUILayout.FlexibleSpace();
								EditorGUILayout.EndHorizontal();

								Rect rect = GUILayoutUtility.GetLastRect();
								Rect crossRect = new Rect(rect.x + EditorGUIUtility.currentViewWidth - 85, rect.y, 20, 20);
								if (GUI.Button(crossRect, EditorGUIUtility.IconContent("winbtn_win_close")))
								{
									RemoveVFX(i_feedbackData);
								}
								EditorGUILayout.BeginHorizontal();
								SerializedProperty m_prefab = serializedObject.FindProperty("feedbackList.Array.data[" + i + "].vfxData.vfxPrefab");
								GUILayout.Label("Prefab: ", GUILayout.Width(100));
								EditorGUILayout.PropertyField(m_prefab, GUIContent.none);
								EditorGUILayout.EndHorizontal();

								EditorGUILayout.BeginHorizontal();
								SerializedProperty m_offset = serializedObject.FindProperty("feedbackList.Array.data[" + i + "].vfxData.offset");
								GUILayout.Label("Offset: ", GUILayout.Width(100));
								EditorGUILayout.PropertyField(m_offset, GUIContent.none);

								SerializedProperty m_scaleMultiplier = serializedObject.FindProperty("feedbackList.Array.data[" + i + "].vfxData.scaleMultiplier");
								GUILayout.Label("Scale: ", GUILayout.Width(100));
								EditorGUILayout.PropertyField(m_scaleMultiplier, GUIContent.none);
								EditorGUILayout.EndHorizontal();

								EditorGUILayout.BeginHorizontal();
								SerializedProperty m_direction = serializedObject.FindProperty("feedbackList.Array.data[" + i + "].vfxData.direction");
								GUILayout.Label("Direction: ", GUILayout.Width(150));
								EditorGUILayout.PropertyField(m_direction, GUIContent.none);

								SerializedProperty m_attachToTarget = serializedObject.FindProperty("feedbackList.Array.data[" + i + "].vfxData.attachToTarget");
								GUILayout.Label("Attach to target: ", GUILayout.Width(100));
								EditorGUILayout.PropertyField(m_attachToTarget, GUIContent.none);
								EditorGUILayout.EndHorizontal();
							}
						}
						GUILayout.EndVertical();

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

						GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						if (GUILayout.Button("Preview", GUILayout.Width(100), GUILayout.Height(20)))
						{
							PreviewFeedback(feedbackDatas.feedbackList[i]);
							return;
						}
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();

					}
					GUILayout.EndVertical();
				}
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

		GUILayout.BeginVertical(EditorStyles.helpBox);
		{
			GUI.color = Color.gray;
			GUILayout.Box("Categories", i_headerStyle);
			GUILayout.Space(10);
			GUI.color = Color.white;
			GUILayout.Space(10);

			for (int i = 0; i < feedbackDatas.feedbackCategories.Count; i++)
			{
				GUILayout.BeginHorizontal();
				{
					EditorGUILayout.BeginHorizontal();
					FeedbackEventCategory category = feedbackDatas.feedbackCategories[i];
					GUILayout.FlexibleSpace();
					category.displayName = EditorGUILayout.TextField(category.displayName);
					if (GUI.changed)
					{
						RecalculateCategoryOptions();
						break;
					}
					category.displayColor = EditorGUILayout.ColorField(category.displayColor);
					this.serializedObject.ApplyModifiedProperties();
					if (GUILayout.Button(EditorGUIUtility.IconContent("winbtn_win_close")))
					{
						feedbackDatas.feedbackCategories.RemoveAt(i);
						RecalculateCategoryOptions();
						break;
					}
					EditorGUILayout.EndHorizontal();
				}
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.BeginHorizontal();
		{
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Add category", GUILayout.Width(100), GUILayout.Height(30)))
			{
				AddCategory();
			}
			GUILayout.FlexibleSpace();

		}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();

		this.serializedObject.ApplyModifiedProperties();
		if (GUI.changed)
		{
			EditorUtility.SetDirty(target);
		}
	}

	public void PreviewFeedback(FeedbackData _data)
	{
		if (FeedbackPreviewer.instance == null)
		{
			FeedbackPreviewer previewWindow = ScriptableObject.CreateInstance<FeedbackPreviewer>();
			previewWindow.Show();
		}
		FeedbackPreviewer.instance.PreviewFeedback(_data);
	}

	public void AddEvent()
	{
		FeedbackData i_newFeedbackData = new FeedbackData();
		i_newFeedbackData.shakeData = null;
		i_newFeedbackData.vibrationData = null;
		i_newFeedbackData.eventName = "event.null (" + (feedbackDatas.feedbackList.Count + 1) + ")";
		i_newFeedbackData.soundData = null;
		i_newFeedbackData.vfxData = null;
		int categoryIndex = Mathf.Clamp(selectedCategoryIndex, 0, feedbackDatas.feedbackCategories.Count - 1);
		if (feedbackDatas.feedbackCategories.Count > 0)
		{
			i_newFeedbackData.category = feedbackDatas.feedbackCategories[categoryIndex];
		}
		showPosition.Add(false);
		feedbackDatas.feedbackList.Add(i_newFeedbackData);
	}

	public void AddCategory()
	{
		FeedbackEventCategory newCategory = new FeedbackEventCategory();
		feedbackDatas.feedbackCategories.Add(newCategory);
		RecalculateCategoryOptions();
	}

	void AddVFX(FeedbackData _data )
	{
		_data.vfxDataInited = true;
	}

	void RemoveVFX(FeedbackData _data)
	{
		_data.vfxDataInited = false;
	}

	void AddSound(FeedbackData _data)
	{
		_data.soundDataInited = true;
	}

	void RemoveSound(FeedbackData _data )
	{
		_data.soundDataInited = false;
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
		bool i_stringFound = false;
		string[] assetPaths = AssetDatabase.GetAllAssetPaths();
		foreach (string assetPath in assetPaths)
		{
			if (assetPath.EndsWith(".cs"))
			{
				if (File.ReadAllText(assetPath).Contains(_eventName))
				{
					i_stringFound = true;
				}
			}
		}
		return i_stringFound;
	}
}
