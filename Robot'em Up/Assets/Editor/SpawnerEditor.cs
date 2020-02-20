using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Spawner))]
public class SpawnerEditor : Editor
{
	private Spawner spawner;
	private void OnEnable ()
	{
		spawner = (Spawner)target;
	}

	public override void OnInspectorGUI ()
	{
		this.serializedObject.Update();

		EditorGUI.BeginChangeCheck();
		SerializedProperty m_type = serializedObject.FindProperty("type");
		EditorGUILayout.PropertyField(m_type);
		if (EditorGUI.EndChangeCheck())
		{
			spawner.RecalculateEndspawnLocation();
		}

		Transform previousStartPosition = spawner.startPosition;
		EditorGUI.BeginChangeCheck();
		SerializedProperty m_startPosition = serializedObject.FindProperty("startPosition");
		EditorGUILayout.PropertyField(m_startPosition);
		serializedObject.ApplyModifiedProperties();
		if (EditorGUI.EndChangeCheck())
		{
			if (previousStartPosition != null && previousStartPosition.GetComponent<SpawnStartPosition>() != null)
			{
				DestroyImmediate(previousStartPosition.GetComponent<SpawnStartPosition>());
			}
			if (spawner.startPosition != null)
			{
				spawner.startPosition.gameObject.AddComponent<SpawnStartPosition>().linkedSpawner = spawner;
				spawner.RecalculateEndspawnLocation();
			}
		}

		SerializedProperty m_delayBeforeBeingFree = serializedObject.FindProperty("delayBeforeBeingFree");
		EditorGUILayout.PropertyField(m_delayBeforeBeingFree);

		if (spawner.type == SpawnerType.Air)
		{
			SerializedProperty m_zonePreviewDuration = serializedObject.FindProperty("zonePreviewDuration");
			EditorGUILayout.PropertyField(m_zonePreviewDuration);
		}

		SerializedProperty m_spawnDuration = serializedObject.FindProperty("spawnDuration");
		EditorGUILayout.PropertyField(m_spawnDuration);

		if (spawner.type == SpawnerType.Ground)
		{
			EditorGUI.BeginChangeCheck();
			SerializedProperty m_jumpDistance = serializedObject.FindProperty("jumpDistance");
			EditorGUILayout.PropertyField(m_jumpDistance);
			SerializedProperty m_horizontalLerpCurve = serializedObject.FindProperty("horizontalLerpCurve");
			EditorGUILayout.PropertyField(m_horizontalLerpCurve);
			SerializedProperty m_rotationLerpCurve = serializedObject.FindProperty("rotationLerpCurve");
			EditorGUILayout.PropertyField(m_rotationLerpCurve);
			SerializedProperty m_attachToObject = serializedObject.FindProperty("attachSpawnedObject");
			EditorGUILayout.PropertyField(m_attachToObject);
			if (EditorGUI.EndChangeCheck())
			{
				spawner.RecalculateEndspawnLocation();
			}
		}

		if (spawner.type == SpawnerType.Underground)
		{
			SerializedProperty m_attachToObject = serializedObject.FindProperty("attachSpawnedObject");
			EditorGUILayout.PropertyField(m_attachToObject);
		}

		SerializedProperty m_verticalLerpCurve = serializedObject.FindProperty("verticalLerpCurve");
		EditorGUILayout.PropertyField(m_verticalLerpCurve);
		SerializedProperty m_delayBeforeActivation = serializedObject.FindProperty("delayBeforeActivation");
		EditorGUILayout.PropertyField(m_delayBeforeActivation);
		serializedObject.ApplyModifiedProperties();
	}

	public void OnSceneGUI ()
	{
		if (spawner.transform.hasChanged)
		{
			spawner.RecalculateEndspawnLocation();
			spawner.transform.hasChanged = false;
		}
	}
}
