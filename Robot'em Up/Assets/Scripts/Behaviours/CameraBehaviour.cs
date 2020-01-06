using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using MyBox;

public enum CameraType { Combat, Adventure, Circle}
public class CameraBehaviour : MonoBehaviour
{
	public int defaultPriority = 9;
	public int enabledPriority = 11;
	public float maxRotation = 40;
	public float maxForwardTranslation = 10;
	public float rotationSpeed = 1;
	public float translationSpeed = 1;

	public bool enableTranslation = true;

	[Separator("Debug values")]
	public float wantedAngle;
	public float currentDistanceX;
	public float currentDistanceY;

	public CameraType type;
	public CameraZone zone;
	private CinemachineVirtualCamera virtualCamera;
	private Quaternion defaultRotation;
	private Vector3 defaultTranslation;

	public void InitCamera ( CameraType _type, CameraZone _zone)
	{
		type = _type;
		zone = _zone;
	}

	private void Awake ()
	{
		defaultRotation = transform.localRotation;
	}

	private void Update ()
	{
		if (zone == null) { return; }
		if (virtualCamera == null) { virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>(); defaultTranslation = virtualCamera.transform.localPosition; }

		if (zone.IsZoneActivated())
		{
			//Enable camera
			virtualCamera.m_Priority = enabledPriority;
			switch (type)
			{
				case CameraType.Combat:
					UpdateCombatCamera();
					break;
				case CameraType.Circle:
					UpdateCircleCamera();
					break;
			}

		} else
		{
			//Disable camera
			virtualCamera.m_Priority = defaultPriority;
		}
	}

	void UpdateCombatCamera()
	{
		Vector3 internal_middlePosition = Vector3.Lerp(GameManager.playerOne.transform.position, GameManager.playerTwo.transform.position, 0.5f);
		Vector3 internal_directionToCenter = internal_middlePosition - zone.GetCenterPosition();
		float internal_xAngle = Vector3.Angle(zone.transform.TransformDirection(new Vector3(0, -1, 0)), internal_directionToCenter);
		float internal_yAngle = Vector3.Angle(zone.transform.TransformDirection(new Vector3(1, 0, 0)), internal_directionToCenter);

		float internal_xDistance = Mathf.Abs(internal_directionToCenter.magnitude * Mathf.Sin(internal_xAngle * Mathf.Deg2Rad));
		float internal_yDistance = Mathf.Abs(internal_directionToCenter.magnitude * Mathf.Sin(internal_yAngle * Mathf.Deg2Rad));

		int internal_xDirection = 1;
		int internal_yDirection = 1;

		float internal_directionAngle = SwissArmyKnife.SignedAngleBetween(zone.transform.TransformDirection(new Vector3(0, -1, 0)), internal_directionToCenter, Vector3.up);
		if (internal_directionAngle >= 0 && internal_directionAngle < 90) { internal_xDirection = -1; internal_yDirection = -1; }
		if (internal_directionAngle >= 90 && internal_directionAngle < 180) { internal_xDirection = -1; internal_yDirection = 1; }
		if (internal_directionAngle >= 180 && internal_directionAngle < 270) { internal_xDirection = 1; internal_yDirection = 1; }
		if (internal_directionAngle >= 270 && internal_directionAngle < 360) { internal_xDirection = 1; internal_yDirection = -1; }

		Vector3 internal_directionToCorner = zone.cornerA - zone.transform.position;
		float internal_xMaxDistance = internal_directionToCorner.magnitude * Mathf.Sin(Vector3.Angle(zone.transform.TransformDirection(new Vector3(0, -1, 0)), internal_directionToCorner) * Mathf.Deg2Rad);
		float internal_yMaxDistance = internal_directionToCorner.magnitude * Mathf.Sin(Vector3.Angle(zone.transform.TransformDirection(new Vector3(1, 0, 0)), internal_directionToCorner) * Mathf.Deg2Rad);

		internal_xDistance = (Mathf.Clamp(internal_xDistance, 0, internal_xMaxDistance) * internal_xDirection) / internal_xMaxDistance;
		internal_yDistance = (Mathf.Clamp(internal_yDistance, 0, internal_yMaxDistance) * internal_yDirection) / internal_yMaxDistance;

		float internal_wantedRotationAngle = Mathf.Lerp(-maxRotation, maxRotation, (internal_xDistance + 1) / 2f);
		wantedAngle = internal_wantedRotationAngle;
		currentDistanceX = internal_xDistance;
		currentDistanceY = internal_yDistance;

		Quaternion internal_wantedRotation = Quaternion.Euler(defaultRotation.eulerAngles.x, defaultRotation.eulerAngles.y + internal_wantedRotationAngle, defaultRotation.eulerAngles.z);
		transform.localRotation = Quaternion.Lerp(transform.localRotation, internal_wantedRotation, Time.deltaTime * rotationSpeed);
		if (enableTranslation)
		{
			float internal_wantedTranslation = Mathf.Lerp(-maxForwardTranslation, maxForwardTranslation, (internal_yDistance + 1) / 2f);
			Vector3 internal_wantedPosition = new Vector3(defaultTranslation.x, defaultTranslation.y, defaultTranslation.z + internal_wantedTranslation);
			virtualCamera.transform.localPosition = Vector3.Lerp(virtualCamera.transform.localPosition, internal_wantedPosition, Time.deltaTime * translationSpeed);
		}
	}

	void UpdateCircleCamera()
	{
		Vector3 internal_middlePosition = Vector3.zero;
		if (GameManager.deadPlayers.Count > 0)
		{
			internal_middlePosition = zone.GetPlayersInside()[0].transform.position;
		} else
		{
			internal_middlePosition = Vector3.Lerp(GameManager.playerOne.transform.position, GameManager.playerTwo.transform.position, 0.5f);
		}
		Vector3 internal_directionToCenter = internal_middlePosition - zone.GetCenterPosition();
		Vector3 internal_wantedPosition = internal_middlePosition;
		internal_directionToCenter.y = 0;
		Quaternion internal_wantedRotation = Quaternion.LookRotation(-internal_directionToCenter);

		transform.localPosition = Vector3.Lerp(transform.localPosition, internal_wantedPosition, Time.deltaTime * translationSpeed);
		transform.localRotation = Quaternion.Lerp(transform.localRotation, internal_wantedRotation, Time.deltaTime * rotationSpeed);
	}
}
