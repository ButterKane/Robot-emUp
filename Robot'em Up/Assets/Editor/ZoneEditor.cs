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
		List<Tool> internal_restrictedTools = new List<Tool>();
		internal_restrictedTools.Add(Tool.Move);
		internal_restrictedTools.Add(Tool.Rotate);
		internal_restrictedTools.Add(Tool.Scale);

		//Generates main zone object
		GameObject internal_newZone = new GameObject();
		internal_newZone.name = "Fight Zone";
		internal_newZone.AddComponent<ToolRestrictor>().restrictedTools = internal_restrictedTools;

		//Add wave component
		internal_newZone.AddComponent<WaveController>();

		//Generates zone selector object
		GameObject internal_zoneSelector = new GameObject();
		internal_zoneSelector.name = "Camera zone";
		internal_zoneSelector.transform.SetParent(internal_newZone.transform);
		CameraZone camZone = internal_zoneSelector.AddComponent<CameraZone>();
		camZone.GenerateZone(CameraType.Combat);

		//Generate the camera pivot
		GameObject internal_cameraPivot = new GameObject();
		internal_cameraPivot.name = "Camera pivot";
		internal_cameraPivot.transform.SetParent(internal_newZone.transform);
		CameraBehaviour cam = internal_cameraPivot.AddComponent<CameraBehaviour>();
		cam.InitCamera(CameraType.Combat, camZone);
		internal_cameraPivot.AddComponent<ToolRestrictor>().restrictedTools = internal_restrictedTools;

		//Generate the virtual camera
		GameObject internal_virtualCamera = new GameObject();
		internal_virtualCamera.name = "Virtual camera";
		internal_virtualCamera.transform.SetParent(internal_cameraPivot.transform);
		CinemachineVirtualCamera virtualCameraScript = internal_virtualCamera.AddComponent<CinemachineVirtualCamera>();
		virtualCameraScript.m_Lens.FieldOfView = 60;
		internal_virtualCamera.transform.localPosition = new Vector3(0, 20, -30);
		internal_virtualCamera.transform.localRotation = Quaternion.Euler(30, 0, 0);

		internal_newZone.transform.position = new Vector3(0, 0.5f, 0);
		Selection.activeGameObject = internal_zoneSelector;
	}

	[MenuItem("Zone/Create circle zone")]
	static void GenerateCircleZone ()
	{
		//Restricted tool list
		List<Tool> internal_restrictedTools = new List<Tool>();
		internal_restrictedTools.Add(Tool.Move);
		internal_restrictedTools.Add(Tool.Rotate);
		internal_restrictedTools.Add(Tool.Scale);

		//Generates main zone object
		GameObject internal_newZone = new GameObject();
		internal_newZone.name = "Circle Zone";
		internal_newZone.AddComponent<ToolRestrictor>().restrictedTools = internal_restrictedTools;

		//Add wave component
		internal_newZone.AddComponent<WaveController>();

		//Generates zone selector object
		GameObject internal_zoneSelector = new GameObject();
		internal_zoneSelector.name = "Camera zone";
		internal_zoneSelector.transform.SetParent(internal_newZone.transform);
		CameraZone camZone = internal_zoneSelector.AddComponent<CameraZone>();
		camZone.GenerateZone(CameraType.Circle);

		//Generate the camera pivot
		GameObject internal_cameraPivot = new GameObject();
		internal_cameraPivot.name = "Camera pivot";
		internal_cameraPivot.transform.SetParent(internal_newZone.transform);
		CameraBehaviour cam = internal_cameraPivot.AddComponent<CameraBehaviour>();
		cam.InitCamera(CameraType.Circle, camZone);
		internal_cameraPivot.AddComponent<ToolRestrictor>().restrictedTools = internal_restrictedTools;

		//Generate the virtual camera
		GameObject internal_virtualCamera = new GameObject();
		internal_virtualCamera.name = "Virtual camera";
		internal_virtualCamera.transform.SetParent(internal_cameraPivot.transform);
		CinemachineVirtualCamera virtualCameraScript = internal_virtualCamera.AddComponent<CinemachineVirtualCamera>();
		virtualCameraScript.m_Lens.FieldOfView = 60;
		internal_virtualCamera.transform.localPosition = new Vector3(0, 20, -30);
		internal_virtualCamera.transform.localRotation = Quaternion.Euler(30, 0, 0);

		internal_newZone.transform.position = new Vector3(0, 0.5f, 0);
		Selection.activeGameObject = internal_zoneSelector;
	}

	[CustomEditor(typeof(CameraZone)), CanEditMultipleObjects]
	public class CameraZoneHandle : Editor
	{
		protected virtual void OnSceneGUI ()
		{
			CameraZone internal_zone = (CameraZone)target;

			switch (internal_zone.type)
			{
				case CameraType.Combat:
					EditCombatZone(internal_zone);
					break;
				case CameraType.Circle:
					EditCircleZone(internal_zone);
					break;
			}
		}

		void EditCombatZone ( CameraZone _zone)
		{
			EditorGUI.BeginChangeCheck();
			Vector3 cornerA = Handles.FreeMoveHandle(_zone.cornerA_access, Quaternion.identity, 1, Vector3.zero, Handles.SphereHandleCap);
			Vector3 cornerB = Handles.FreeMoveHandle(_zone.cornerB_access, Quaternion.identity, 1, Vector3.zero, Handles.SphereHandleCap);

			if (Tools.current == Tool.Rotate)
			{
				Tools.current = Tool.None;
			}

			Quaternion rot = Handles.Disc(_zone.transform.rotation, _zone.transform.position, new Vector3(0, 1, 0), 3, false, 0);

			Handles.Label(cornerA, "Corner A");
			Handles.Label(cornerB, "Corner B");

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(_zone, "Change Look At Target Position");
				float deltaRotation = Quaternion.Angle(rot, _zone.transform.rotation);
				float dir = (rot.eulerAngles.y < _zone.transform.rotation.eulerAngles.y) ? -1 : 1;
				if (deltaRotation != 0)
				{
					cornerA = RotatePointAroundPivot(cornerA, _zone.transform.position, new Vector3(0, deltaRotation * dir, 0));
					cornerB = RotatePointAroundPivot(cornerB, _zone.transform.position, new Vector3(0, deltaRotation * dir, 0));
				}
				_zone.cornerA_access = cornerA;
				_zone.cornerB_access = cornerB;
				_zone.transform.localRotation = rot;
				_zone.Update();
			}
		}

		void EditCircleZone ( CameraZone _zone )
		{
			if (Tools.current == Tool.Move)
			{
				Tools.current = Tool.None;
			}
			EditorGUI.BeginChangeCheck();
			Vector3 position = Handles.PositionHandle(_zone.transform.position, _zone.transform.rotation);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(_zone, "Change Look At Target Position");
				_zone.transform.position = position;
				_zone.Update();
			}
		}
	}
	public static Vector3 RotatePointAroundPivot ( Vector3 _point, Vector3 _pivot, Vector3 _angle)
	{
		return Quaternion.Euler(_angle) * (_point - _pivot) + _pivot;
	}
}