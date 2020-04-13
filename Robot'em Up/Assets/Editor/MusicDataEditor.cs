using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(MusicDatas)), CanEditMultipleObjects]
public class MusicDataEditor : Editor
{
	private MusicDatas datas;
	private void OnEnable ()
	{
		datas = (MusicDatas)target;
	}
	public override void OnInspectorGUI ()
	{
		serializedObject.Update();

		EditorGUILayout.BeginVertical();
		for (int i = 0; i < datas.musicDatas.Count; i++)
		{
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			{
				MusicData foundData = datas.musicDatas[i];
				EditorGUILayout.BeginHorizontal();
				{
					//Pawn state editor
					SerializedProperty m_id = serializedObject.FindProperty("musicDatas.Array.data[" + i + "].id");
					GUILayout.Label("Name: ", GUILayout.Width(150));
					EditorGUILayout.PropertyField(m_id, GUIContent.none);
					if (GUILayout.Button(EditorGUIUtility.IconContent("winbtn_win_close"), GUILayout.Width(80), GUILayout.ExpandHeight(true)))
					{
						RemoveMusic(datas.musicDatas[i]);
					}
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				{
					SerializedProperty m_clip = serializedObject.FindProperty("musicDatas.Array.data[" + i + "].clip");
					EditorGUILayout.PropertyField(m_clip);
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				{
					SerializedProperty m_isLooping = serializedObject.FindProperty("musicDatas.Array.data[" + i + "].isLooping");
					EditorGUILayout.PropertyField(m_isLooping);
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				{
					SerializedProperty m_fadeIn = serializedObject.FindProperty("musicDatas.Array.data[" + i + "].fadeInDuration");
					GUILayout.Label("Fade in duration: ", GUILayout.Width(150));
					EditorGUILayout.PropertyField(m_fadeIn, GUIContent.none);

					SerializedProperty m_fadeOut = serializedObject.FindProperty("musicDatas.Array.data[" + i + "].fadeOutDuration");
					GUILayout.Label("Fade out duration: ", GUILayout.Width(150));
					EditorGUILayout.PropertyField(m_fadeOut, GUIContent.none);
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();
			GUILayout.Space(10);
		}
		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("+", GUILayout.Width(30), GUILayout.Height(30)))
		{
			AddNewMusic();
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		EditorUtility.SetDirty(target);
		serializedObject.ApplyModifiedProperties();
	}

	private void RemoveMusic(MusicData _data)
	{
		if (datas.musicDatas.Contains(_data))
		{
			datas.musicDatas.Remove(_data);
		}
	}
	private void AddNewMusic ()
	{
		MusicData newData = new MusicData();
		newData.id = "New music [" + datas.musicDatas.Count + "]";
		datas.musicDatas.Add(newData);
	}
}
