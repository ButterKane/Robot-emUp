﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using MyBox;

public enum CameraType { Combat, Adventure, Circle}
public class CameraBehaviour : MonoBehaviour
{
	[Separator("General settings")]
	public int defaultPriority = 9;
	public int enabledPriority = 11;
	public float maxRotation = 40;
	public float maxForwardTranslation = 10;
	public float rotationSpeed = 1;
	public float translationSpeed = 1;

	public bool enableTranslation = true;

	[Separator("Debug values")]
	[ReadOnly] public float wantedAngle;
	[ReadOnly] public float currentDistanceX;
	[ReadOnly] public float currentDistanceY;

	[ReadOnly] public CameraType type;
	[ReadOnly] public CameraZone zone;
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
		transform.localRotation = Quaternion.Lerp(transform.localRotation, i_wantedRotation, Time.deltaTime * rotationSpeed);
		if (enableTranslation)
		{
			float i_wantedTranslation = Mathf.Lerp(-maxForwardTranslation, maxForwardTranslation, (i_yDistance + 1) / 2f);
			Vector3 i_wantedPosition = new Vector3(defaultTranslation.x, defaultTranslation.y, defaultTranslation.z + i_wantedTranslation);
			virtualCamera.transform.localPosition = Vector3.Lerp(virtualCamera.transform.localPosition, i_wantedPosition, Time.deltaTime * translationSpeed);
		}
	}

	void UpdateCircleCamera()
	{
		Vector3 i_middlePosition = Vector3.zero;
		if (GameManager.deadPlayers.Count > 0)
		{
			i_middlePosition = zone.GetPlayersInside()[0].transform.position;
		} else
		{
			i_middlePosition = Vector3.Lerp(GameManager.playerOne.transform.position, GameManager.playerTwo.transform.position, 0.5f);
		}
		Vector3 i_directionToCenter = i_middlePosition - zone.GetCenterPosition();
		Vector3 i_wantedPosition = i_middlePosition;
		i_directionToCenter.y = 0;
		Quaternion i_wantedRotation = Quaternion.LookRotation(-i_directionToCenter);

		transform.localPosition = Vector3.Lerp(transform.localPosition, i_wantedPosition, Time.deltaTime * translationSpeed);
		transform.localRotation = Quaternion.Lerp(transform.localRotation, i_wantedRotation, Time.deltaTime * rotationSpeed);
	}
}
