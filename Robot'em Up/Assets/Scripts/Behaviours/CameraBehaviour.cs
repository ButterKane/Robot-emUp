using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using MyBox;
using PathCreation;

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

	public Transform focusPoint;
	[Range(0f, 1f)] public float focusImportance;

	public bool enableTranslation = true;

	[Separator("Debug values")]
	[ReadOnly] public float wantedAngle;
	[ReadOnly] public float currentDistanceX;
	[ReadOnly] public float currentDistanceY;

	public CameraType type;
	[ReadOnly] public CameraZone zone;
	private CinemachineVirtualCamera virtualCamera;
	private Quaternion defaultRotation;
	private Vector3 defaultTranslation;
	private bool activated;

	private PathCreator followedPath;
	private float distanceTravelled;

	public void InitCamera ( CameraType _type, CameraZone _zone)
	{
		type = _type;
		zone = _zone;
		followedPath = GetComponentInParent<PathCreator>();
		if (followedPath != null)
		{
			followedPath.pathUpdated += OnPathChanged;
		}
	}

	public void ActivateCamera()
	{
		followedPath = GetComponentInParent<PathCreator>();
		Debug.Log("Activating");
		if (followedPath != null)
		{
			followedPath.pathUpdated += OnPathChanged;
		}
		activated = true;
	}

	public void DesactivateCamera()
	{
		activated = false;
	}

	private void Awake ()
	{
		defaultRotation = transform.localRotation;
		//InitCamera(CameraType.Adventure, null);
		ActivateCamera();
	}

	private void Update ()
	{
		if (virtualCamera == null) { virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>(); defaultTranslation = virtualCamera.transform.localPosition; }

		if (activated)
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
				case CameraType.Adventure:
					UpdateAdventureCamera();
					break;
			}

		} else
		{
			//Disable camera
			virtualCamera.m_Priority = defaultPriority;
		}
	}
	void OnPathChanged ()
	{
		distanceTravelled = followedPath.path.GetClosestDistanceAlongPath(transform.position);
	}

	void UpdateAdventureCamera()
	{
		if (followedPath == null) { return; }
		Vector3 i_middlePosition = Vector3.zero;
		if (GameManager.deadPlayers.Count > 0)
		{
			i_middlePosition = zone.GetPlayersInside()[0].transform.position;
		}
		else
		{
			i_middlePosition = Vector3.Lerp(GameManager.playerOne.transform.position, GameManager.playerTwo.transform.position, 0.5f);
		}
		i_middlePosition = Vector3.Lerp(i_middlePosition, focusPoint.position, focusImportance);
		Quaternion i_wantedRotation = Quaternion.LookRotation(-(virtualCamera.transform.position - i_middlePosition));
		float lerpCoef = Vector3.Distance(transform.position, followedPath.path.GetPointAtDistance(distanceTravelled, EndOfPathInstruction.Stop));
		lerpCoef = Mathf.Clamp(lerpCoef, 1f, 10f);
		transform.position = Vector3.Lerp(transform.position, followedPath.path.GetPointAtDistance(distanceTravelled, EndOfPathInstruction.Stop), Time.deltaTime * translationSpeed / lerpCoef) ;
	//	virtualCamera.transform.LookAt(i_middlePosition);
		distanceTravelled = followedPath.path.GetClosestDistanceAlongPath(i_middlePosition);
		virtualCamera.transform.rotation = Quaternion.Lerp(virtualCamera.transform.rotation, i_wantedRotation, Time.deltaTime * rotationSpeed);
		Vector3 pivotLookDirection = followedPath.path.GetDirectionAtDistance(distanceTravelled);
		pivotLookDirection = Quaternion.AngleAxis(90, Vector3.up) * pivotLookDirection;
		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(pivotLookDirection), Time.deltaTime * rotationSpeed / Quaternion.Angle(transform.rotation, Quaternion.LookRotation(pivotLookDirection)));
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
