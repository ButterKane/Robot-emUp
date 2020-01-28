using UnityEditor;
using UnityEngine;
using Cinemachine;
using System.Collections.Generic;
using PathCreation;

public class ZoneEditor
{
	[MenuItem("CameraZone/Create combat zone")]
	static void GenerateCombatZone ()
	{
		//Restricted tool list
		List<Tool> i_restrictedTools = new List<Tool>();
		i_restrictedTools.Add(Tool.Move);
		i_restrictedTools.Add(Tool.Rotate);
		i_restrictedTools.Add(Tool.Scale);

		//Generates main zone object
		GameObject i_newZone = new GameObject();
		i_newZone.name = "Fight Zone";
		i_newZone.AddComponent<ToolRestrictor>().restrictedTools = i_restrictedTools;

		//Add wave component
		i_newZone.AddComponent<WaveController>();

		//Generates zone selector object
		GameObject i_zoneSelector = new GameObject();
		i_zoneSelector.name = "Camera zone";
		i_zoneSelector.transform.SetParent(i_newZone.transform);
		CameraZone camZone = i_zoneSelector.AddComponent<CameraZone>();
		camZone.GenerateZone(CameraType.Combat);

		//Generate the camera pivot
		GameObject i_cameraPivot = new GameObject();
		i_cameraPivot.name = "Camera pivot";
		i_cameraPivot.transform.SetParent(i_newZone.transform);
		CameraBehaviour cam = i_cameraPivot.AddComponent<CameraBehaviour>();
		cam.InitCamera(CameraType.Combat, camZone);
		i_cameraPivot.AddComponent<ToolRestrictor>().restrictedTools = i_restrictedTools;
		camZone.linkedCameraBehaviour = cam;

		//Generate the virtual camera
		GameObject i_virtualCamera = new GameObject();
		i_virtualCamera.name = "Virtual camera";
		i_virtualCamera.transform.SetParent(i_cameraPivot.transform);
		CinemachineVirtualCamera virtualCameraScript = i_virtualCamera.AddComponent<CinemachineVirtualCamera>();
		virtualCameraScript.m_Lens.FieldOfView = 60;
		i_virtualCamera.transform.localPosition = new Vector3(0, 20, -30);
		i_virtualCamera.transform.localRotation = Quaternion.Euler(30, 0, 0);

		i_newZone.transform.position = new Vector3(0, 0.5f, 0);
		Selection.activeGameObject = i_zoneSelector;
	}

	[MenuItem("CameraZone/Create circle zone")]
	static void GenerateCircleZone ()
	{
		//Restricted tool list
		List<Tool> i_restrictedTools = new List<Tool>();
		i_restrictedTools.Add(Tool.Move);
		i_restrictedTools.Add(Tool.Rotate);
		i_restrictedTools.Add(Tool.Scale);

		//Generates main zone object
		GameObject i_newZone = new GameObject();
		i_newZone.name = "Circle Zone";
		i_newZone.AddComponent<ToolRestrictor>().restrictedTools = i_restrictedTools;

		//Add wave component
		i_newZone.AddComponent<WaveController>();

		//Generates zone selector object
		GameObject i_zoneSelector = new GameObject();
		i_zoneSelector.name = "Camera zone";
		i_zoneSelector.transform.SetParent(i_newZone.transform);
		CameraZone camZone = i_zoneSelector.AddComponent<CameraZone>();
		camZone.GenerateZone(CameraType.Circle);

		//Generate the camera pivot
		GameObject i_cameraPivot = new GameObject();
		i_cameraPivot.name = "Camera pivot";
		i_cameraPivot.transform.SetParent(i_newZone.transform);
		CameraBehaviour cam = i_cameraPivot.AddComponent<CameraBehaviour>();
		cam.InitCamera(CameraType.Circle, camZone);
		i_cameraPivot.AddComponent<ToolRestrictor>().restrictedTools = i_restrictedTools;
		camZone.linkedCameraBehaviour = cam;

		//Generate the virtual camera
		GameObject i_virtualCamera = new GameObject();
		i_virtualCamera.name = "Virtual camera";
		i_virtualCamera.transform.SetParent(i_cameraPivot.transform);
		CinemachineVirtualCamera virtualCameraScript = i_virtualCamera.AddComponent<CinemachineVirtualCamera>();
		virtualCameraScript.m_Lens.FieldOfView = 60;
		i_virtualCamera.transform.localPosition = new Vector3(0, 20, -30);
		i_virtualCamera.transform.localRotation = Quaternion.Euler(30, 0, 0);

		i_newZone.transform.position = new Vector3(0, 0.5f, 0);
		Selection.activeGameObject = i_zoneSelector;
	}

	[MenuItem("CameraZone/Create adventure camera")]
	static void GenerateAdventureZone ()
	{
		//Restricted tool list
		List<Tool> i_restrictedTools = new List<Tool>();
		i_restrictedTools.Add(Tool.Move);
		i_restrictedTools.Add(Tool.Rotate);
		i_restrictedTools.Add(Tool.Scale);

		//Generates main zone object
		GameObject i_newZone = new GameObject();
		i_newZone.name = "Adventure Zone";
		i_newZone.AddComponent<ToolRestrictor>().restrictedTools = i_restrictedTools;


		//Generates zone selector object
		GameObject i_zoneSelector = new GameObject();
		i_zoneSelector.name = "Camera path";
		i_zoneSelector.transform.SetParent(i_newZone.transform);
		i_zoneSelector.AddComponent<PathCreator>();

		//Generate the camera pivot
		GameObject i_cameraPivot = new GameObject();
		i_cameraPivot.name = "Camera pivot";
		i_cameraPivot.transform.SetParent(i_zoneSelector.transform);
		CameraBehaviour cam = i_cameraPivot.AddComponent<CameraBehaviour>();
		cam.InitCamera(CameraType.Adventure, null);
		i_cameraPivot.AddComponent<ToolRestrictor>().restrictedTools = i_restrictedTools;

		//Generate the virtual camera
		GameObject i_virtualCamera = new GameObject();
		i_virtualCamera.name = "Virtual camera";
		i_virtualCamera.transform.SetParent(i_cameraPivot.transform);
		CinemachineVirtualCamera virtualCameraScript = i_virtualCamera.AddComponent<CinemachineVirtualCamera>();
		virtualCameraScript.m_Lens.FieldOfView = 60;
		i_virtualCamera.transform.localPosition = new Vector3(0, 20, -30);
		i_virtualCamera.transform.localRotation = Quaternion.Euler(30, 0, 0);

		i_newZone.transform.position = new Vector3(0, 0.5f, 0);
		Selection.activeGameObject = i_zoneSelector;
	}

	[CustomEditor(typeof(CameraZone)), CanEditMultipleObjects]
	public class CameraZoneHandle : Editor
	{
		protected virtual void OnSceneGUI ()
		{
			CameraZone i_zone = (CameraZone)target;

			switch (i_zone.type)
			{
				case CameraType.Combat:
					EditCombatZone(i_zone);
					break;
				case CameraType.Circle:
					EditCircleZone(i_zone);
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