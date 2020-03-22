using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(Grabbable)), CanEditMultipleObjects]
public class GrabbableEditor : Editor
{
	private Grabbable grabbable;
	private GrabbableDatas datas;
	private void OnEnable ()
	{
		grabbable = (Grabbable)target;
		datas = GrabbableDatas.GetDatas();
		UpdateInformations();
	}

	void UpdateInformations()
	{
		int id = 0;
		List<GrabbableInformation> newGiList = new List<GrabbableInformation>();
		foreach (GrabbableInformation gi in grabbable.GetComponentsInChildren<GrabbableInformation>())
		{
			newGiList.Add(gi);
			gi.name = "GrabbableInformation[" + id + "]";
			gi.id = id;
			id++;
		}
		grabbable.grabbableInformation = newGiList;
	}

	public void OnSceneGUI ()
	{
		for (int i = 0; i < grabbable.grabbableInformation.Count; i++)
		{
			GrabbableInformation gi = grabbable.grabbableInformation[i];
			if (gi.previewLine.sharedMaterial != datas.editorLineRendererMaterial)
			{
				gi.previewLine.sharedMaterial = datas.editorLineRendererMaterial;
			}
			if (gi.previewLine.startWidth != datas.editorLineRendererWidth)
			{
				gi.previewLine.startWidth = datas.editorLineRendererWidth;
				gi.previewLine.endWidth = datas.editorLineRendererWidth;
			}
			//Move collider
			Handles.Label(gi.transform.position, "GrabInfo[" + gi.id + "]");
			EditorGUI.BeginChangeCheck();
			Vector3 newColliderPosition = Handles.PositionHandle(gi.transform.position, Quaternion.identity);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(gi, "Change grabbable information collider position");
				gi.transform.position = newColliderPosition;
				grabbable.UpdateLine();
			}

			//Move target
			EditorGUI.BeginChangeCheck();
			Vector3 newTargetPosition = Handles.PositionHandle(gi.targetedPosition.position, Quaternion.identity);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(gi, "Change grabbable information target position");
				gi.targetedPosition.position = newTargetPosition;
				grabbable.UpdateLine();
			}
		}
	}

	public override void OnInspectorGUI ()
	{
		serializedObject.Update();

		EditorGUILayout.BeginVertical(EditorStyles.helpBox);
		for (int i = 0; i < grabbable.grabbableInformation.Count; i++)
		{
			EditorGUILayout.BeginHorizontal();
			{
				//Pawn state editor
				EditorGUILayout.LabelField("Information found with ID: " + grabbable.grabbableInformation[i].id, EditorStyles.boldLabel);
				if (GUILayout.Button(EditorGUIUtility.IconContent("winbtn_win_close"), GUILayout.Width(80), GUILayout.ExpandHeight(true)))
				{
					RemoveGrabbableInformation(grabbable.grabbableInformation[i]);
				}
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("+", GUILayout.Width(30), GUILayout.Height(30)))
		{
			AddNewGrabbableInformation();
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		EditorUtility.SetDirty(target);
		serializedObject.ApplyModifiedProperties();
	}

	void AddNewGrabbableInformation()
	{
		GrabbableInformation newInf = new GameObject().AddComponent<GrabbableInformation>();

		//Generate holder
		newInf.gameObject.transform.SetParent(grabbable.transform);
		newInf.gameObject.name = "GrabbableInformation";
		newInf.transform.localPosition = new Vector3(1, 0, 1);

		//Generate hit position
		newInf.targetedPosition = new GameObject().transform;
		newInf.targetedPosition.name = "TargetedPosition";
		newInf.targetedPosition.SetParent(newInf.transform);
		newInf.targetedPosition.transform.localPosition = new Vector3(1, 0, 1);

		//Generate preview line
		newInf.previewLine = newInf.gameObject.AddComponent<LineRenderer>();
		newInf.previewLine.material = datas.editorLineRendererMaterial;
		newInf.previewLine.startWidth = datas.editorLineRendererWidth;
		newInf.previewLine.endWidth = datas.editorLineRendererWidth;

		//Generate trigger
		newInf.trigger = newInf.gameObject.AddComponent<SphereCollider>();
		newInf.trigger.isTrigger = true;
		newInf.trigger.center = Vector3.zero;
		newInf.trigger.gameObject.tag = "GrabbableTrigger";

		UpdateInformations();
	}

	void RemoveGrabbableInformation(GrabbableInformation _inf)
	{
		if (grabbable.grabbableInformation.Contains(_inf))
		{
			grabbable.grabbableInformation.Remove(_inf);
			DestroyImmediate(_inf.gameObject);
		}
		UpdateInformations();
	}
}
