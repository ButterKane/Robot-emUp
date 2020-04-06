using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SoundDatas))]
public class SoundEditor : Editor
{
	GUIStyle headerStyle;
	SoundDatas soundDatas;

	public void OnValidate ()
	{
		soundDatas = (SoundDatas)target;
	}


	public override void OnInspectorGUI ()
	{
		soundDatas = (SoundDatas)target;
		this.serializedObject.Update();

		GUIStyle i_headerStyle = new GUIStyle(EditorStyles.helpBox);
		i_headerStyle.alignment = TextAnchor.MiddleCenter;
		i_headerStyle.fontSize = 20;
		i_headerStyle.fontStyle = FontStyle.Bold;

		GUILayout.BeginVertical(EditorStyles.helpBox);
		{
			GUI.color = Color.gray;
			GUILayout.Box("Global settings", i_headerStyle);
			GUILayout.Space(10);
			GUI.color = Color.white;
			GUILayout.Space(10);

			//None yet
		}
		GUILayout.EndVertical();

		GUILayout.BeginVertical(EditorStyles.helpBox);
		{
			GUI.color = Color.gray;
			GUILayout.Box("Sound datas", i_headerStyle);
			GUILayout.Space(10);
			GUI.color = Color.white;
			GUILayout.Space(10);

			for (int i = 0; i < soundDatas.soundList.Count; i++) {
				GUI.color = new Color(0.8f, 0.8f, 0.8f);
				GUILayout.BeginVertical(EditorStyles.helpBox);
				{
					SoundData soundData = soundDatas.soundList[i];
					GUILayout.Label(soundDatas.soundList[i].soundName, EditorStyles.centeredGreyMiniLabel);
					soundData.soundName = EditorGUILayout.TextField(soundData.soundName);
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
						SerializedProperty m_volumeMultiplier = serializedObject.FindProperty("soundList.Array.data[" + i + "].volumeMultiplier");
						GUILayout.Label("Volume multiplier", GUILayout.Width(100));
						EditorGUILayout.PropertyField(m_volumeMultiplier, GUIContent.none, GUILayout.Width(150));
						GUILayout.Space(10);
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
						} else
						{
							newSoundItem.playChances = 0f;
						}
						soundData.soundList.Add(newSoundItem);
					}
					if (GUILayout.Button("Remove sound", GUILayout.Width(100), GUILayout.Height(20)))
					{
						soundDatas.soundList.Remove(soundDatas.soundList[i]);
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
				if (GUILayout.Button("Add sound", GUILayout.Width(100), GUILayout.Height(30)))
				{
					AddSound();
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

	void AddSound()
	{
		SoundData i_newSoundData = new SoundData();
		i_newSoundData.soundName = "New sound " + (soundDatas.soundList.Count + 1);
		i_newSoundData.soundList = new List<Sound>();
		Sound newSound = new Sound();
		newSound.playChances = 1f;
		i_newSoundData.soundList.Add(newSound);
		soundDatas.soundList.Add(i_newSoundData);
	}
}
