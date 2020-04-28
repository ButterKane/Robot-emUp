using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using MyBox;

public enum CameraCustomType { Combat, Adventure, Circle}
public class CameraBehaviour : MonoBehaviour
{
	[Separator("General settings")]
	public int defaultPriority = 9;
	public int enabledPriority = 11;
	public float maxRotation = 40;
	public float maxForwardTranslation = 10;
	public float rotationSpeed = 1;
	public float translationSpeed = 1;

	public Transform focusPoint;
	[Range(0f, 1f)] public float focusImportance;

	public bool enableTranslation = true;

	[Separator("Debug values")]
	[ReadOnly] public float wantedAngle;
	[ReadOnly] public float currentDistanceX;
	[ReadOnly] public float currentDistanceY;

	public CameraCustomType type;
	[ReadOnly] public CameraZone zone;
	private CinemachineVirtualCamera virtualCamera;
	private Quaternion defaultRotation;
	private Vector3 defaultTranslation;
	[ReadOnly] public bool activated;

	private GameObject followPoint;
	public float minCameraDistance = 30;
	public float maxCameraDistance = 30;
	public Transform pivot;

	public static List<CameraBehaviour> allCameras = new List<CameraBehaviour>();

	public void InitCamera ( CameraCustomType _type, CameraZone _zone)
	{
		type = _type;
		zone = _zone;
	}

	[ExecuteAlways]
	void OnDrawGizmos ()
	{
		Gizmos.DrawIcon(transform.position, "SpriteCollider", true);
	}

	public void ActivateCamera()
	{
		if (allCameras != null)
		{
			foreach (CameraBehaviour b in allCameras)
			{
				if (b != this)
					b.DesactivateCamera();
			}
		}
		activated = true;
		CinemachineFramingTransposer transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
		if (transposer != null)
		{
			transposer.m_MinimumDistance = minCameraDistance;
			transposer.m_MaximumDistance = maxCameraDistance;
		}
		virtualCamera.m_Follow = followPoint.transform;
		virtualCamera.m_LookAt = followPoint.transform;
	}

	public void DesactivateCamera() 
	{
		activated = false;
	}

	private void Awake ()
	{
		if (Application.isPlaying)
		{
			if (allCameras == null) { allCameras = new List<CameraBehaviour>(); }
			if (!allCameras.Contains(this))
			{
				allCameras.Add(this);
			}
			if (pivot == null) { pivot = transform.parent; }
			defaultRotation = pivot.transform.localRotation;
			followPoint = new GameObject();
			followPoint.transform.SetParent(this.transform);
			followPoint.name = "FollowPoint";
		}
	}

	private void LateUpdate ()
	{
		if (Application.isPlaying)
		{
			if (virtualCamera == null) { virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>(); defaultTranslation = virtualCamera.transform.localPosition; }
			if (virtualCamera == null) { virtualCamera = GetComponent<CinemachineVirtualCamera>(); defaultTranslation = virtualCamera.transform.localPosition; }

			if (activated)
			{
				//Enable camera
				virtualCamera.m_Priority = enabledPriority;
				switch (type)
				{
					case CameraCustomType.Combat:
						UpdateCombatCamera();
						break;
					case CameraCustomType.Circle:
						UpdateCircleCamera();
						break;
					case CameraCustomType.Adventure:
						UpdateAdventureCamera();
						break;
				}

			}
			else
			{
				//Disable camera
				virtualCamera.m_Priority = defaultPriority;
			}
		}
	}

	void UpdateAdventureCamera()
	{
		Vector3 i_middlePosition;
		if (GameManager.deadPlayers.Count > 0)
		{
			if (GameManager.alivePlayers.Count > 0)
			{
				i_middlePosition = GameManager.alivePlayers[0].transform.position;
			} else
			{
				i_middlePosition = Vector3.Lerp(GameManager.playerOne.transform.position, GameManager.playerTwo.transform.position, 0.5f);
			}
		}
		else
		{
			i_middlePosition = Vector3.Lerp(GameManager.playerOne.transform.position, GameManager.playerTwo.transform.position, 0.5f);
		}
		if (focusPoint != null)
		{
			i_middlePosition = Vector3.Lerp(i_middlePosition, focusPoint.position, focusImportance);
		}
		followPoint.transform.position = i_middlePosition;
	}

	void UpdateCombatCamera()
	{
		Vector3 i_middlePosition = Vector3.Lerp(GameManager.playerOne.transform.position, GameManager.playerTwo.transform.position, 0.5f);
		Vector3 i_directionToCenter = i_middlePosition - zone.GetCenterPosition();
		float i_xAngle = Vector3.Angle(zone.transform.TransformDirection(new Vector3(0, -1, 0)), i_directionToCenter);
		float i_yAngle = Vector3.Angle(zone.transform.TransformDirection(new Vector3(1, 0, 0)), i_directionToCenter);

		float i_xDistance = Mathf.Abs(i_directionToCenter.magnitude * Mathf.Sin(i_xAngle * Mathf.Deg2Rad));
		float i_yDistance = Mathf.Abs(i_directionToCenter.magnitude * Mathf.Sin(i_yAngle * Mathf.Deg2Rad));

		int i_xDirection = 1;
		int i_yDirection = 1;

		float i_directionAngle = SwissArmyKnife.SignedAngleBetween(zone.transform.TransformDirection(new Vector3(0, -1, 0)), i_directionToCenter, Vector3.up);
		if (i_directionAngle >= 0 && i_directionAngle < 90) { i_xDirection = -1; i_yDirection = -1; }
		if (i_directionAngle >= 90 && i_directionAngle < 180) { i_xDirection = -1; i_yDirection = 1; }
		if (i_directionAngle >= 180 && i_directionAngle < 270) { i_xDirection = 1; i_yDirection = 1; }
		if (i_directionAngle >= 270 && i_directionAngle < 360) { i_xDirection = 1; i_yDirection = -1; }

		Vector3 i_directionToCorner = zone.cornerA_access - zone.transform.position;
		float i_xMaxDistance = i_directionToCorner.magnitude * Mathf.Sin(Vector3.Angle(zone.transform.TransformDirection(new Vector3(0, -1, 0)), i_directionToCorner) * Mathf.Deg2Rad);
		float i_yMaxDistance = i_directionToCorner.magnitude * Mathf.Sin(Vector3.Angle(zone.transform.TransformDirection(new Vector3(1, 0, 0)), i_directionToCorner) * Mathf.Deg2Rad);

		i_xDistance = (Mathf.Clamp(i_xDistance, 0, i_xMaxDistance) * i_xDirection) / i_xMaxDistance;
		i_yDistance = (Mathf.Clamp(i_yDistance, 0, i_yMaxDistance) * i_yDirection) / i_yMaxDistance;

		float i_wantedRotationAngle = Mathf.Lerp(-maxRotation, maxRotation, (i_xDistance + 1) / 2f);
		wantedAngle = i_wantedRotationAngle;
		currentDistanceX = i_xDistance;
		currentDistanceY = i_yDistance;

		Quaternion i_wantedRotation = Quaternion.Euler(defaultRotation.eulerAngles.x, defaultRotation.eulerAngles.y + i_wantedRotationAngle, defaultRotation.eulerAngles.z);
		pivot.transform.localRotation = Quaternion.Lerp(pivot.transform.localRotation, i_wantedRotation, Time.deltaTime * rotationSpeed);
		if (enableTranslation)
		{
			float i_wantedTranslation = Mathf.Lerp(-maxForwardTranslation, maxForwardTranslation, (i_yDistance + 1) / 2f);
			Vector3 i_wantedPosition = new Vector3(defaultTranslation.x, defaultTranslation.y, defaultTranslation.z + i_wantedTranslation);
            virtualCamera.transform.localPosition = Vector3.Lerp(virtualCamera.transform.localPosition, i_wantedPosition, Time.deltaTime * translationSpeed);
		}
	}

	void UpdateCircleCamera()
	{
		Vector3 i_middlePosition;
		if (GameManager.deadPlayers.Count > 0)
		{
			if (GameManager.alivePlayers.Count > 0)
			{
				i_middlePosition = GameManager.alivePlayers[0].transform.position;
			}
			else
			{
				i_middlePosition = Vector3.Lerp(GameManager.playerOne.transform.position, GameManager.playerTwo.transform.position, 0.5f);
			}
		}
		else
		{
			i_middlePosition = Vector3.Lerp(GameManager.playerOne.transform.position, GameManager.playerTwo.transform.position, 0.5f);
		}
		Vector3 i_directionToCenter = i_middlePosition - zone.GetCenterPosition();
		Vector3 i_wantedPosition = i_middlePosition;
		i_directionToCenter.y = 0;
		Quaternion i_wantedRotation = Quaternion.LookRotation(-i_directionToCenter);
		pivot.transform.position = Vector3.Lerp(pivot.transform.position, i_wantedPosition, Time.deltaTime * translationSpeed);
		pivot.transform.rotation = Quaternion.Lerp(pivot.transform.rotation, i_wantedRotation, Time.deltaTime * rotationSpeed);
	}
}
