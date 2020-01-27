using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CameraBehaviour))]
public class CameraBehaviourEditor : Editor
{
	CameraBehaviour behaviour;
	private void OnEnable ()
	{
		behaviour = (CameraBehaviour)target;
	}
	public override void OnInspectorGUI ()
	{
		this.serializedObject.Update();
		GUILayout.Space(20);
		if (behaviour.type == CameraType.Circle || behaviour.type == CameraType.Combat)
		{
			EditorGUILayout.BeginHorizontal();
			SerializedProperty m_maxRotation = serializedObject.FindProperty("maxRotation");
			GUILayout.Label("Max rotation: ", GUILayout.Width(120));
			EditorGUILayout.PropertyField(m_maxRotation, GUIContent.none);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			SerializedProperty m_maxForwardTranslation = serializedObject.FindProperty("maxForwardTranslation");
			GUILayout.Label("Max Z translation: ", GUILayout.Width(120));
			EditorGUILayout.PropertyField(m_maxForwardTranslation, GUIContent.none);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			SerializedProperty m_rotationSpeed = serializedObject.FindProperty("rotationSpeed");
			GUILayout.Label("Rotation Speed: ", GUILayout.Width(120));
			EditorGUILayout.PropertyField(m_rotationSpeed, GUIContent.none);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			SerializedProperty m_translationSpeed = serializedObject.FindProperty("translationSpeed");
			GUILayout.Label("Translation Speed: ", GUILayout.Width(120));
			EditorGUILayout.PropertyField(m_translationSpeed, GUIContent.none);
			EditorGUILayout.EndHorizontal();
		}

		if (behaviour.type == CameraType.Adventure)
		{
			EditorGUILayout.BeginHorizontal();
			SerializedProperty m_focusPoint = serializedObject.FindProperty("focusPoint");
			GUILayout.Label("Focus Point: ", GUILayout.Width(120));
			EditorGUILayout.PropertyField(m_focusPoint, GUIContent.none);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			SerializedProperty m_focusImportance = serializedObject.FindProperty("focusImportance");
			GUILayout.Label("Focus Importance: ", GUILayout.Width(120));
			EditorGUILayout.PropertyField(m_focusImportance, GUIContent.none);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			SerializedProperty m_minDistance = serializedObject.FindProperty("minCameraDistance");
			GUILayout.Label("Min Cam Distance: ", GUILayout.Width(120));
			EditorGUILayout.PropertyField(m_minDistance, GUIContent.none);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			SerializedProperty m_maxDistance = serializedObject.FindProperty("maxCameraDistance");
			GUILayout.Label("Max Cam Distance: ", GUILayout.Width(120));
			EditorGUILayout.PropertyField(m_maxDistance, GUIContent.none);
			EditorGUILayout.EndHorizontal();
		}

		GUILayout.Space(10);
		serializedObject.ApplyModifiedProperties();
	}
}
