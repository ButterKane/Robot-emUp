using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.EditorCoroutines.Editor;

[CustomEditor(typeof(FeedbackDatas))]
public class FeedbackEditor : Editor
{
	FeedbackDatas feedbackDatas;
	public List<bool> showPosition;
	string[] categoryOptions;
	int selectedCategoryIndex;
	EditorCoroutine recalculationCoroutine;
	float recalculationProgression;
	string nameSearched;

	public void OnValidate ()
	{
		if (feedbackDatas.feedbackList.Count < 1)
		{
			UpdateFeedbackList();
		}
	}
	private void OnEnable ()
	{
		feedbackDatas = (FeedbackDatas)target;
		UpdateFeedbackList();
		showPosition = new List<bool>();
		for (int i = 0; i < feedbackDatas.feedbackList.Count; i++)
		{
			showPosition.Add(false);
		}
		selectedCategoryIndex = feedbackDatas.feedbackCategories.Count;
		RecalculateCategoryOptions();
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

		if (feedbackDatas.feedbackList.Count < 1)
		{
			UpdateFeedbackList();
		}

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

			//Event implementation check (Removed temporarly)
			/*
			if (recalculationCoroutine == null)
			{
				if (GUILayout.Button("Check for implemented events\n (May cause severe lag, save before)", i_buttonStyle, GUILayout.Height(100)))
				{
					RecalculateEventIntegration();
				}
			} else
			{
				GUILayout.Label("Checking implementations...", i_headerStyle); 
				EditorGUILayout.Slider(recalculationProgression, 0, 100);
			}
			*/
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
			GUILayout.Label("Category: ", EditorStyles.largeLabel);
			List<string> i_categoryOptionsWithAll = categoryOptions.ToList();
			i_categoryOptionsWithAll.Add("All");

			selectedCategoryIndex = EditorGUILayout.Popup(selectedCategoryIndex, i_categoryOptionsWithAll.ToArray(), GUILayout.Width(100));

			GUILayout.Label("Name: ", EditorStyles.largeLabel);
			nameSearched = EditorGUILayout.TextField(nameSearched);

			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			for (int i = 0; i < feedbackDatas.feedbackList.Count; i++)
			{
				if (selectedCategoryIndex < feedbackDatas.feedbackCategories.Count && feedbackDatas.feedbackCategories[selectedCategoryIndex] != feedbackDatas.feedbackList[i].category) { continue; }
				if (nameSearched != null && nameSearched != "" && !feedbackDatas.feedbackList[i].eventName.Contains(nameSearched)) { continue; }
				GUI.color = new Color(0.8f, 0.8f, 0.8f);
				FeedbackData i_feedbackData = feedbackDatas.feedbackList[i];
				SerializedObject i_serializedFeedbackData = new SerializedObject(serializedObject.FindProperty("feedbackList.Array.data[" + i + "]").objectReferenceValue);
				i_serializedFeedbackData.Update();
				GUILayout.BeginVertical(EditorStyles.helpBox);
					{
					GUILayout.BeginHorizontal();
					showPosition[i] = EditorGUILayout.Foldout(showPosition[i], feedbackDatas.feedbackList[i].eventName) ;
					if (GUILayout.Button("Preview", GUILayout.Width(100), GUILayout.Height(20)))
					{
						PreviewFeedback(feedbackDatas.feedbackList[i]);
						return;
					}
					GUI.color = i_feedbackData.category.displayColor;
					int index = EditorGUILayout.Popup(GetCategoryIndex(feedbackDatas.feedbackList[i].category), categoryOptions, GUILayout.Width(150));
					if (index != -1)
					{
						feedbackDatas.feedbackList[i].category = feedbackDatas.feedbackCategories[index];
					}
					GUI.color = new Color(0.8f, 0.8f, 0.8f);
					if (GUILayout.Button(EditorGUIUtility.IconContent("winbtn_win_close"), GUILayout.Width(20), GUILayout.Height(20)))
					{
						RemoveEvent(feedbackDatas.feedbackList[i].eventName);
						break;
					}
					/*
					if (i_feedbackData.eventCalled)
					{
						EditorGUILayout.LabelField(EditorGUIUtility.IconContent("d_winbtn_mac_max"), GUILayout.Height(20), GUILayout.Width(20));
					}
					else
					{
						EditorGUILayout.LabelField(EditorGUIUtility.IconContent("d_winbtn_mac_close"), GUILayout.Height(20), GUILayout.Width(20));
					}
					*/

					GUILayout.EndHorizontal();
					if (showPosition[i])
					{

						GUILayout.BeginHorizontal();
						EditorGUI.BeginChangeCheck();
						string newEventName = EditorGUILayout.TextField(i_feedbackData.eventName);
						if (EditorGUI.EndChangeCheck())
						{
							RenameAsset(i_feedbackData.eventName, newEventName);
							i_feedbackData.eventName = newEventName;
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
									SerializedProperty m_force = i_serializedFeedbackData.FindProperty("vibrationData.force");
									GUILayout.Label("Force: ", GUILayout.Width(100));
									EditorGUILayout.PropertyField(m_force, GUIContent.none);
									EditorGUILayout.EndHorizontal();

									EditorGUILayout.BeginHorizontal();
									SerializedProperty m_forceCurve = i_serializedFeedbackData.FindProperty("vibrationData.forceCurve");
									GUILayout.Label("Force curve: ", GUILayout.Width(100));
									EditorGUILayout.PropertyField(m_forceCurve, GUIContent.none);
									EditorGUILayout.EndHorizontal();

									EditorGUILayout.BeginHorizontal();
									SerializedProperty m_duration = i_serializedFeedbackData.FindProperty("vibrationData.duration");
									GUILayout.Label("Duration: ", GUILayout.Width(100));
									EditorGUILayout.PropertyField(m_duration, GUIContent.none);
									EditorGUILayout.EndHorizontal();

									EditorGUILayout.BeginHorizontal();
									SerializedProperty m_target = i_serializedFeedbackData.FindProperty("vibrationData.target");
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
									SerializedProperty m_duration = i_serializedFeedbackData.FindProperty("shakeData.duration");
									GUILayout.Label("Duration: ", GUILayout.Width(100));
									EditorGUILayout.PropertyField(m_duration, GUIContent.none);
									EditorGUILayout.EndHorizontal();

									EditorGUILayout.BeginHorizontal();
									SerializedProperty m_intensity = i_serializedFeedbackData.FindProperty("shakeData.intensity");
									GUILayout.Label("Force: ", GUILayout.Width(100));
									EditorGUILayout.PropertyField(m_intensity, GUIContent.none);
									EditorGUILayout.EndHorizontal();

									EditorGUILayout.BeginHorizontal();
									SerializedProperty m_intensityCurve = i_serializedFeedbackData.FindProperty("shakeData.intensityCurve");
									GUILayout.Label("Force curve: ", GUILayout.Width(100));
									EditorGUILayout.PropertyField(m_intensityCurve, GUIContent.none);
									EditorGUILayout.EndHorizontal();

									EditorGUILayout.BeginHorizontal();
									SerializedProperty m_frequency = i_serializedFeedbackData.FindProperty("shakeData.frequency");
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
								SoundData soundData = i_feedbackData.soundData;

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

								for (int y = 0; y < soundData.soundList.Count; y++)
								{
									GUILayout.BeginHorizontal();
									{
										GUILayout.Label("Clip", GUILayout.Width(100));
										soundData.soundList[y].clip = (AudioClip)EditorGUILayout.ObjectField(soundData.soundList[y].clip, typeof(AudioClip), false, GUILayout.Width(150));

										if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.Play"), GUILayout.Width(20), GUILayout.Height(20)))
										{
											SoundManager.PlaySoundInEditor(soundData.soundList[y].clip, 0, false);
										}
										GUILayout.Label("Play Chances", GUILayout.Width(100));
										EditorGUI.BeginChangeCheck();
										float probaSliderValue = EditorGUILayout.Slider(Mathf.Round(soundData.soundList[y].playChances * 100f) / 100f, 0f, 1f);
										if (EditorGUI.EndChangeCheck())
										{
											soundData.SetPlayProbability(soundData.soundList[y], probaSliderValue);
										}
										if (GUILayout.Button(EditorGUIUtility.IconContent("winbtn_win_close"), GUILayout.Width(20), GUILayout.Height(20)))
										{
											soundData.RemoveSound(soundData.soundList[y]);
											return;
										}
									}
									GUILayout.EndHorizontal();
								}

								GUILayout.BeginHorizontal();
								{
									GUILayout.FlexibleSpace();
									GUILayout.Space(10);
									SerializedProperty m_volumeMultiplier = i_serializedFeedbackData.FindProperty("soundData.volumeMultiplier");
									GUILayout.Label("Volume multiplier", GUILayout.Width(100));
									EditorGUILayout.PropertyField(m_volumeMultiplier, GUIContent.none, GUILayout.Width(150));
									GUILayout.Space(10);
									SerializedProperty m_delay = i_serializedFeedbackData.FindProperty("soundData.delay");
									GUILayout.Label("Delay", GUILayout.Width(100));
									EditorGUILayout.PropertyField(m_delay, GUIContent.none, GUILayout.Width(150));
									GUILayout.FlexibleSpace();
								}
								GUILayout.EndHorizontal();

								GUILayout.BeginHorizontal();
								GUILayout.FlexibleSpace();
								if (GUILayout.Button("Add clip", GUILayout.Width(100), GUILayout.Height(20)))
								{
									Sound newSoundItem = new Sound();
									newSoundItem.clip = null;
									if (soundData.soundList.Count == 0)
									{
										newSoundItem.playChances = 1f;
									}
									else
									{
										newSoundItem.playChances = 0f;
									}
									soundData.soundList.Add(newSoundItem);
								}
								GUILayout.FlexibleSpace();
								GUILayout.EndHorizontal();
								GUILayout.Space(10);
							}
						}
						GUILayout.EndVertical();

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
								SerializedProperty m_prefab = i_serializedFeedbackData.FindProperty("vfxData.vfxPrefab");
								GUILayout.Label("Prefab: ", GUILayout.Width(100));
								EditorGUILayout.PropertyField(m_prefab, GUIContent.none);
								EditorGUILayout.EndHorizontal();

								EditorGUILayout.BeginHorizontal();
								SerializedProperty m_offset = i_serializedFeedbackData.FindProperty("vfxData.offset");
								GUILayout.Label("Offset: ", GUILayout.Width(100));
								EditorGUILayout.PropertyField(m_offset, GUIContent.none);

								SerializedProperty m_scaleMultiplier = i_serializedFeedbackData.FindProperty("vfxData.scaleMultiplier");
								GUILayout.Label("Scale: ", GUILayout.Width(100));
								EditorGUILayout.PropertyField(m_scaleMultiplier, GUIContent.none);
								EditorGUILayout.EndHorizontal();

								EditorGUILayout.BeginHorizontal();
								SerializedProperty m_direction = i_serializedFeedbackData.FindProperty("vfxData.direction");
								GUILayout.Label("Direction: ", GUILayout.Width(150));
								EditorGUILayout.PropertyField(m_direction, GUIContent.none);

								SerializedProperty m_position = i_serializedFeedbackData.FindProperty("vfxData.position");
								GUILayout.Label("Position: ", GUILayout.Width(150));
								EditorGUILayout.PropertyField(m_position, GUIContent.none);
								EditorGUILayout.EndHorizontal();

								EditorGUILayout.BeginHorizontal();
								SerializedProperty m_attachToTarget = i_serializedFeedbackData.FindProperty("vfxData.attachToTarget");
								GUILayout.Label("Attach to target: ", GUILayout.Width(100));
								EditorGUILayout.PropertyField(m_attachToTarget, GUIContent.none);
								EditorGUILayout.EndHorizontal();
							}
						}
						GUILayout.EndVertical();

					}
					GUILayout.EndVertical();
				}
				i_serializedFeedbackData.ApplyModifiedProperties();
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

	public void PreviewFeedback( FeedbackData _data )
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
		FeedbackData i_newFeedbackData = ScriptableObject.CreateInstance<FeedbackData>();
		i_newFeedbackData.shakeData = new ShakeData(0,0,0);
		i_newFeedbackData.vibrationData = new VibrationData();
		i_newFeedbackData.vibrationData.forceCurve = new AnimationCurve();
		i_newFeedbackData.vibrationData.forceCurve.AddKey(new Keyframe(0, 1));
		i_newFeedbackData.vibrationData.forceCurve.AddKey(new Keyframe(1, 1));
		i_newFeedbackData.eventName = "event.null (" + (feedbackDatas.feedbackList.Count + 1) + ")";
		i_newFeedbackData.soundData = new SoundData();
		i_newFeedbackData.vfxData = null;
		int categoryIndex = Mathf.Clamp(selectedCategoryIndex, 0, feedbackDatas.feedbackCategories.Count - 1);
		if (feedbackDatas.feedbackCategories.Count > 0)
		{
			i_newFeedbackData.category = feedbackDatas.feedbackCategories[categoryIndex];
		}
		showPosition.Add(false);
		feedbackDatas.feedbackList.Add(i_newFeedbackData);
		AssetDatabase.CreateAsset(i_newFeedbackData, "Assets/Resources/FeedbackToolResources/Datas/" + i_newFeedbackData.eventName + ".asset");
		AssetDatabase.SaveAssets();
	}

	public void RemoveEvent(string _eventName)
	{
		AssetDatabase.DeleteAsset("Assets/Resources/FeedbackToolResources/Datas/" + _eventName + ".asset");
		AssetDatabase.SaveAssets();
		UpdateFeedbackList();
	}

	public void RenameAsset( string _previousName, string _newName )
	{
		AssetDatabase.RenameAsset("Assets/Resources/FeedbackToolResources/Datas/" + _previousName + ".asset", _newName);
	}

	public void AddCategory()
	{
		FeedbackEventCategory newCategory = new FeedbackEventCategory();
		feedbackDatas.feedbackCategories.Add(newCategory);
		RecalculateCategoryOptions();
	}

	void AddVFX( FeedbackData _data )
	{
		_data.vfxDataInited = true;
	}

	void RemoveVFX( FeedbackData _data )
	{
		_data.vfxDataInited = false;
	}

	void AddSound( FeedbackData _data )
	{
		_data.soundDataInited = true;
	}

	void RemoveSound( FeedbackData _data )
	{
		_data.soundDataInited = false;
	}

	void AddVibration( FeedbackData _data )
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

	void UpdateFeedbackList()
	{
		feedbackDatas = (FeedbackDatas)target;
		this.serializedObject.Update();
		List<FeedbackData> feedbackDataList = new List<FeedbackData>();
		foreach (FeedbackData obj in Resources.LoadAll<FeedbackData>("FeedbackToolResources/Datas"))
		{
			feedbackDataList.Add(obj);
			AssetDatabase.RenameAsset("Assets/Resources/FeedbackToolResources/Datas/" + obj.name + ".asset", obj.eventName);
		}
		feedbackDatas.feedbackList = feedbackDataList;
		this.serializedObject.ApplyModifiedProperties();
		if (GUI.changed)
		{
			EditorUtility.SetDirty(target);
		}
	}

	void RecalculateEventIntegration()
	{
		recalculationProgression = 0;
		if (recalculationCoroutine != null) { EditorCoroutineUtility.StopCoroutine(recalculationCoroutine); }
		recalculationCoroutine = EditorCoroutineUtility.StartCoroutine(RecalculateEventIntegration_C(), this);
	}

	IEnumerator RecalculateEventIntegration_C()
	{
		List<FeedbackData> feedbackList = feedbackDatas.feedbackList;
		List<bool> isIntegratedList = new List<bool>();
		for (int i = 0; i < feedbackList.Count; i++)
		{
			isIntegratedList.Add(false);
		}
		for (int i = 0; i < feedbackList.Count; i++)
		{
			FeedbackData feedbackData = feedbackList[i];
			string i_eventName = feedbackData.eventName;
			string[] assetPaths = AssetDatabase.GetAllAssetPaths();
			int assetCount = 0;
			foreach (string assetPath in assetPaths)
			{
				assetCount++;
				if (assetCount > 25)
				{
					assetCount = 0;
					yield return null;
				}
				if (assetPath.EndsWith(".cs"))
				{
					if (File.ReadAllText(assetPath).Contains(i_eventName))
					{
						isIntegratedList[i] = true;
						break;
					}
				}
				if (assetPath.EndsWith(".prefab"))
				{
					GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));
					foreach (Component c in prefab.GetComponents(typeof(Component)))
					{
						System.Type tempType = c.GetType();
						FieldInfo[] tempFields = tempType.GetFields();
						foreach (FieldInfo field in tempFields)
						{
							if (!field.IsStatic)
							{
								if (field.FieldType != typeof(string))
								{
									continue;
								}
								else
								{
									if (field.GetValue(c).ToString() == i_eventName)
									{
										isIntegratedList[i] = true;
										break;
									}
								}
							}
						}
					}
				}
			}
			recalculationProgression += Mathf.RoundToInt((1f/(float)feedbackList.Count)*100f);
			yield return null;
		}

		yield return null;
		for (int i = 0; i < feedbackDatas.feedbackList.Count; i++)
		{
			if (i >= isIntegratedList.Count)
			{
				feedbackDatas.feedbackList[i].eventCalled = false;
			}
			else
			{
				feedbackDatas.feedbackList[i].eventCalled = isIntegratedList[i];
			}
		}
		Debug.Log("Event implementation recalculation end");
		recalculationCoroutine = null;
		yield return null;
	}
}
