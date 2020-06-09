using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CinematicCamera : MonoBehaviour
{
	private Transform camObj;
	public float rotationDuration = 20;

	public float xPosition = 0;
	public float yPosition = 10;
	public float zPosition = -15;

	public float maxRotationAngle = 360f;
	public Ease easeCurve;
	public KeyCode triggerKey = KeyCode.C;
	private Camera cameraComponent;
	private void Start ()
	{
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		camObj = GetComponentInChildren<Camera>().transform;
		cameraComponent = camObj.GetComponent<Camera>();
		DisableCamera();
	}

	private void Update ()
	{
		if (camObj != null)
		{
			camObj.transform.localPosition = new Vector3(xPosition, yPosition, zPosition);
			camObj.transform.LookAt(transform.position);
			if (Input.GetKeyDown(triggerKey))
			{
				DOTween.PauseAll();
				DisableCamera();
				EnableCamera();
				transform.DORotate(new Vector3(0, maxRotationAngle, 0), rotationDuration, RotateMode.FastBeyond360).OnComplete(DisableCamera).SetEase(easeCurve);
			}
		}
	}

	private void EnableCamera()
	{
		cameraComponent.enabled = true;
	}

	private void DisableCamera()
	{
		cameraComponent.enabled = false;
		transform.localRotation = Quaternion.identity;
	}
}
