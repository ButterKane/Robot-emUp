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
		i_cameraPivot.AddComponent<ToolRestrictor>().restrictedTools = i_restrictedTools;

		//Generate the virtual camera
		GameObject i_virtualCamera = new GameObject();
		i_virtualCamera.name = "Virtual camera";
		i_virtualCamera.transform.SetParent(i_cameraPivot.transform);
		CinemachineVirtualCamera virtualCameraScript = i_virtualCamera.AddComponent<CinemachineVirtualCamera>();
		virtualCameraScript.m_Lens.FieldOfView = 60;
		i_virtualCamera.transform.localPosition = new Vector3(0, 20, -30);
		i_virtualCamera.transform.localRotation = Quaternion.Euler(30, 0, 0);

		CameraBehaviour cam = i_virtualCamera.AddComponent<CameraBehaviour>();
		cam.pivot = i_cameraPivot.transform;
		cam.InitCamera(CameraType.Combat, camZone);
		camZone.linkedCameraBehaviour = cam;

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
		i_cameraPivot.AddComponent<ToolRestrictor>().restrictedTools = i_restrictedTools;

		//Generate the virtual camera
		GameObject i_virtualCamera = new GameObject();
		i_virtualCamera.name = "Virtual camera";
		i_virtualCamera.transform.SetParent(i_cameraPivot.transform);
		CinemachineVirtualCamera virtualCameraScript = i_virtualCamera.AddComponent<CinemachineVirtualCamera>();
		virtualCameraScript.m_Lens.FieldOfView = 60;
		i_virtualCamera.transform.localPosition = new Vector3(0, 20, -30);
		i_virtualCamera.transform.localRotation = Quaternion.Euler(30, 0, 0);
		CameraBehaviour cam = i_virtualCamera.AddComponent<CameraBehaviour>();
		cam.pivot = i_cameraPivot.transform;
		cam.InitCamera(CameraType.Circle, camZone);
		camZone.linkedCameraBehaviour = cam;

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

		//Generate the virtual camera
		GameObject i_virtualCamera = new GameObject();
		i_virtualCamera.name = "Virtual camera";
		i_virtualCamera.transform.SetParent(i_newZone.transform);
		CinemachineVirtualCamera virtualCameraScript = i_virtualCamera.AddComponent<CinemachineVirtualCamera>();
		virtualCameraScript.m_Lens.FieldOfView = 60;
		i_virtualCamera.transform.localPosition = new Vector3(0, 20, -30);
		i_virtualCamera.transform.localRotation = Quaternion.Euler(30, 0, 0);
		CameraBehaviour cam = i_virtualCamera.AddComponent<CameraBehaviour>();
		cam.InitCamera(CameraType.Adventure, null);
		ApplyDefaultSettings(virtualCameraScript);

		//Generate the enter transition object
		GameObject i_enterTransition = new GameObject();
		GameObject i_comeBackTransition = new GameObject();
		i_enterTransition.name = "EnterTransition";
		i_comeBackTransition.name = "ComebackTransition";
		i_enterTransition.transform.SetParent(i_newZone.transform);
		i_comeBackTransition.transform.SetParent(i_newZone.transform);
		i_enterTransition.transform.localPosition = new Vector3(0, 0, -15f);
		i_comeBackTransition.transform.localPosition = new Vector3(0, 0, 15f);
		i_enterTransition.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
		i_comeBackTransition.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
		i_enterTransition.transform.localScale = new Vector3(12f, 3f, 1f);
		i_comeBackTransition.transform.localScale = new Vector3(12f, 3f, 1f);
		SpriteRenderer sp1 = i_enterTransition.AddComponent<SpriteRenderer>();
		sp1.sprite = Resources.Load<Sprite>("CameraEditor/AdventureCameraSeparator");
		SpriteRenderer sp2 = i_comeBackTransition.AddComponent<SpriteRenderer>();
		sp2.sprite = Resources.Load<Sprite>("CameraEditor/AdventureCameraSeparator");
		sp2.color = new Color(1, 0.5f, 0);
		sp1.color = new Color(0, 1f, 1f);
		i_enterTransition.AddComponent<CameraTransition>().linkedCamera = cam;
		i_comeBackTransition.AddComponent<CameraTransition>().linkedCamera = cam;
		BoxCollider col1 = i_enterTransition.AddComponent<BoxCollider>();
		BoxCollider col2 = i_comeBackTransition.AddComponent<BoxCollider>();
		col1.isTrigger = true;
		col2.isTrigger = true;
		col1.size = new Vector3(5, 0.3f, 50);
		col1.center = new Vector3(0, 0, -25);
		col2.size = new Vector3(5, 0.3f, 50);
		col2.center = new Vector3(0, 0, -25);

		i_newZone.transform.position = new Vector3(0, 0.5f, 0);
		Selection.activeGameObject = i_newZone;
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

	public static void ApplyDefaultSettings(CinemachineVirtualCamera _camera )
	{
		CameraDefaultDatas defaultDatas = Resources.Load<CameraDefaultDatas>("CameraAdventureDefaultDatas");
		_camera.m_Lens.FieldOfView = defaultDatas.fov;
		_camera.AddCinemachineComponent<CinemachineFramingTransposer>();
		CinemachineFramingTransposer transposer = _camera.GetCinemachineComponent<CinemachineFramingTransposer>();
		transposer.m_DeadZoneWidth = defaultDatas.deadZoneWidth;
		transposer.m_DeadZoneHeight = defaultDatas.deadZoneHeight;
		transposer.m_DeadZoneDepth = defaultDatas.deadZoneDepth;
		transposer.m_SoftZoneWidth = defaultDatas.softZoneWidth;
		transposer.m_SoftZoneHeight = defaultDatas.softZoneHeight;
		transposer.m_CameraDistance = defaultDatas.distance;
		transposer.m_MinimumDistance = defaultDatas.minDistance;
		transposer.m_MaximumDistance = defaultDatas.maxDistance;
	}
}