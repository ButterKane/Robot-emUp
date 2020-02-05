using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PuzzleActivator), true)]
public class PuzzleActivatorEditor : Editor
{
	private PuzzleActivator activator;
	private void OnEnable ()
	{
		activator = (PuzzleActivator)target;
	}
	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI();
		serializedObject.Update();
		if (activator.wire != null)
		{
			GUILayout.Label("Wire settings", EditorStyles.centeredGreyMiniLabel);
			GUILayout.Space(10);

			SerializedObject i_serializedWire = new SerializedObject(serializedObject.FindProperty("wire").objectReferenceValue);
			i_serializedWire.Update();

			EditorGUI.BeginChangeCheck();
			SerializedProperty m_wireDefaultColor = i_serializedWire.FindProperty("defaultColor");
			EditorGUILayout.PropertyField(m_wireDefaultColor);
			SerializedProperty m_wireActivatedColor = i_serializedWire.FindProperty("activatedColor");
			EditorGUILayout.PropertyField(m_wireActivatedColor);
			SerializedProperty m_wireWidth = i_serializedWire.FindProperty("width");
			EditorGUILayout.PropertyField(m_wireWidth);
			SerializedProperty m_activationSpeed = i_serializedWire.FindProperty("activationSpeed");
			EditorGUILayout.PropertyField(m_activationSpeed);
			SerializedProperty m_desactivationSpeed = i_serializedWire.FindProperty("desactivationSpeed");
			EditorGUILayout.PropertyField(m_desactivationSpeed);

			GUILayout.Space(10);
			UpdateWire();

			if (GUI.changed)
				UpdateWire();


			i_serializedWire.ApplyModifiedProperties();
		}


		if (activator.wire == null)
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Generate Wire", GUILayout.Width(150), GUILayout.Height(35)))
			{
				GenerateWire();
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Drag existing wire: ", GUILayout.Width(130));
			SerializedProperty m_wire = serializedObject.FindProperty("wire");
			EditorGUILayout.PropertyField(m_wire, GUIContent.none);
			GUILayout.EndHorizontal();

		} else
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Remove Wire", GUILayout.Width(150), GUILayout.Height(35)))
			{
				DeleteWire();
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			SerializedObject i_serializedWire = new SerializedObject(serializedObject.FindProperty("wire").objectReferenceValue);
			i_serializedWire.Update();

			EditorGUI.BeginChangeCheck();
			SerializedProperty m_target = i_serializedWire.FindProperty("target");
			EditorGUILayout.PropertyField(m_target);
			if (GUI.changed)
			{
				i_serializedWire.ApplyModifiedProperties();
				activator.wire.AutoTrace();
			}


			i_serializedWire.ApplyModifiedProperties();
		}
		serializedObject.ApplyModifiedProperties();
	}

	void GenerateWire()
	{
		serializedObject.Update();
		GameObject newWire = new GameObject();
		newWire.name = "Wire";
		newWire.transform.SetParent(activator.transform);
		newWire.transform.localPosition = Vector3.zero;
		activator.wire = newWire.AddComponent<Wire>();
		activator.wire.Init();
		Selection.activeGameObject = newWire.gameObject;
		serializedObject.ApplyModifiedProperties();
		EditorUtility.SetDirty(target);
	}

	void UpdateWire()
	{
		activator.wire.ApplySettings();
	}

	void DeleteWire()
	{
		DestroyImmediate(activator.wire.gameObject);
	}

}
