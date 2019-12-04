using UnityEditor;
using UnityEngine;
using Cinemachine;
using System.Collections.Generic;

public class ZoneEditor
{
	[MenuItem("Zone/Create combat zone")]
	static void GenerateCombatZone ()
	{
		//Restricted tool list
		List<Tool> restrictedTools = new List<Tool>();
		restrictedTools.Add(Tool.Move);
		restrictedTools.Add(Tool.Rotate);
		restrictedTools.Add(Tool.Scale);

		//Generates main zone object
		GameObject newZone = new GameObject();
		newZone.name = "Fight Zone";
		newZone.AddComponent<ToolRestrictor>().restrictedTools = restrictedTools;

		//Add wave component
		newZone.AddComponent<WaveController>();

		//Generates zone selector object
		GameObject zoneSelector = new GameObject();
		zoneSelector.name = "Camera zone";
		zoneSelector.transform.SetParent(newZone.transform);
		CameraZone camZone = zoneSelector.AddComponent<CameraZone>();

		//Generate the camera pivot
		GameObject cameraPivot = new GameObject();
		cameraPivot.name = "Camera pivot";
		cameraPivot.transform.SetParent(newZone.transform);
		CameraBehaviour cam = cameraPivot.AddComponent<CameraBehaviour>();
		cam.InitCamera(CameraType.Combat, camZone);
		cameraPivot.AddComponent<ToolRestrictor>().restrictedTools = restrictedTools;

		//Generate the virtual camera
		GameObject virtualCamera = new GameObject();
		virtualCamera.name = "Virtual camera";
		virtualCamera.transform.SetParent(cameraPivot.transform);
		CinemachineVirtualCamera virtualCameraScript = virtualCamera.AddComponent<CinemachineVirtualCamera>();
		virtualCameraScript.m_Lens.FieldOfView = 60;
		virtualCamera.transform.localPosition = new Vector3(0, 20, -30);
		virtualCamera.transform.localRotation = Quaternion.Euler(30, 0, 0);

		newZone.transform.position = new Vector3(0, 0.5f, 0);
		Selection.activeGameObject = zoneSelector;
	}

	[MenuItem("Zone/Create adventure zone")]
	static void GenerateAdventureZone ()
	{
		Debug.LogWarning("Can't generate adventure zone yet (Not implemented)");
	}

	[CustomEditor(typeof(CameraZone)), CanEditMultipleObjects]
	public class CameraZoneHandle : Editor
	{
		protected virtual void OnSceneGUI ()
		{
			CameraZone example = (CameraZone)target;

			
			EditorGUI.BeginChangeCheck();
			Vector3 cornerA = Handles.FreeMoveHandle(example.cornerA, Quaternion.identity, 1, Vector3.zero, Handles.SphereHandleCap);
			Vector3 cornerB = Handles.FreeMoveHandle(example.cornerB, Quaternion.identity, 1, Vector3.zero, Handles.SphereHandleCap);

			if (Tools.current == Tool.Rotate)
			{
				Tools.current = Tool.None;
			}

			Quaternion rot = Handles.Disc(example.transform.rotation, example.transform.position, new Vector3(0,1,0), 3, false, 0);

			Handles.Label(cornerA, "Corner A");
			Handles.Label(cornerB, "Corner B");

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(example, "Change Look At Target Position");
				float deltaRotation = Quaternion.Angle(rot, example.transform.rotation);
				float dir = (rot.eulerAngles.y < example.transform.rotation.eulerAngles.y) ? -1 : 1; 
				if (deltaRotation != 0)
				{
					cornerA = RotatePointAroundPivot(cornerA, example.transform.position, new Vector3(0 , deltaRotation * dir, 0));
					cornerB = RotatePointAroundPivot(cornerB, example.transform.position, new Vector3(0, deltaRotation * dir, 0));
				}
				example.cornerA = cornerA;
				example.cornerB = cornerB;
				example.transform.localRotation = rot;
				example.Update();
			}
		}
	}
	public static Vector3 RotatePointAroundPivot ( Vector3 point, Vector3 pivot, Vector3 angle)
	{
		return Quaternion.Euler(angle) * (point - pivot) + pivot;
	}
}