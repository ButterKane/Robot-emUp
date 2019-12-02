using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using MyBox;

public enum CameraType { Combat, Adventure}
public class CameraBehaviour : MonoBehaviour
{
	public int minPlayersRequired = 2;
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

	private CameraType type;
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

		List<PlayerController> players = zone.GetPlayersInside();
		if (zone.GetPlayersInside().Count * (1 + GameManager.deadPlayers.Count) >= minPlayersRequired)
		{
			//Enable camera
			virtualCamera.m_Priority = enabledPriority;
			Vector3 middlePosition = Vector3.Lerp(GameManager.playerOne.transform.position, GameManager.playerTwo.transform.position, 0.5f);

			Vector3 directionToCenter = middlePosition - zone.transform.position;
			float xAngle = Vector3.Angle(zone.transform.TransformDirection(new Vector3(0, -1, 0)), directionToCenter);
			float yAngle = Vector3.Angle(zone.transform.TransformDirection(new Vector3(1, 0, 0)), directionToCenter);

			float xDistance = Mathf.Abs(directionToCenter.magnitude * Mathf.Sin(xAngle * Mathf.Deg2Rad));
			float yDistance = Mathf.Abs(directionToCenter.magnitude * Mathf.Sin(yAngle * Mathf.Deg2Rad));

			int xDirection = 1;
			int yDirection = 1;


			float directionAngle = SwissArmyKnife.SignedAngleBetween(zone.transform.TransformDirection(new Vector3(0, -1, 0)), directionToCenter, Vector3.up);
			if (directionAngle >= 0 && directionAngle < 90) { xDirection = -1; yDirection = -1; }
			if (directionAngle >= 90 && directionAngle < 180) { xDirection = -1; yDirection = 1; }
			if (directionAngle >= 180 && directionAngle < 270) { xDirection = 1; yDirection = 1; }
			if (directionAngle >= 270 && directionAngle < 360) { xDirection = 1; yDirection = -1; }

			Vector3 directionToCorner = zone.cornerA - zone.transform.position;
			float xMaxDistance = directionToCorner.magnitude * Mathf.Sin(Vector3.Angle(zone.transform.TransformDirection(new Vector3(0, -1, 0)), directionToCorner) * Mathf.Deg2Rad);
			float yMaxDistance = directionToCorner.magnitude * Mathf.Sin(Vector3.Angle(zone.transform.TransformDirection(new Vector3(1, 0, 0)), directionToCorner) * Mathf.Deg2Rad);

			xDistance = (Mathf.Clamp(xDistance, 0, xMaxDistance) * xDirection)  / xMaxDistance;
			yDistance = (Mathf.Clamp(yDistance, 0, yMaxDistance) * yDirection) / yMaxDistance;

			float wantedRotationAngle = Mathf.Lerp(-maxRotation, maxRotation, (xDistance + 1) / 2f);
			wantedAngle = wantedRotationAngle;
			currentDistanceX = xDistance;
			currentDistanceY = yDistance;


			Quaternion wantedRotation = Quaternion.Euler(defaultRotation.eulerAngles.x, defaultRotation.eulerAngles.y + wantedRotationAngle, defaultRotation.eulerAngles.z);
			transform.localRotation = Quaternion.Lerp(transform.localRotation, wantedRotation, Time.deltaTime * rotationSpeed);
			if (enableTranslation)
			{
				float wantedTranslation = Mathf.Lerp(-maxForwardTranslation, maxForwardTranslation, (yDistance + 1) / 2f);
				Vector3 wantedPosition = new Vector3(defaultTranslation.x, defaultTranslation.y, defaultTranslation.z + wantedTranslation);
				virtualCamera.transform.localPosition = Vector3.Lerp(virtualCamera.transform.localPosition, wantedPosition, Time.deltaTime * translationSpeed);
			}

		} else
		{
			//Disable camera
			virtualCamera.m_Priority = defaultPriority;
		}
	}
}
