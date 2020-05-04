﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TransitionRequirement { OnePlayer, MiddlePoint }
public class CameraTransition : MonoBehaviour
{
	public TransitionRequirement transitionCondition = TransitionRequirement.MiddlePoint;
	public CameraBehaviour linkedCamera;
	private void OnTriggerEnter ( Collider other )
	{
		switch (transitionCondition)
		{
			case TransitionRequirement.MiddlePoint:
				if (!linkedCamera.activated && other.tag == "MiddlePoint")
				{
					linkedCamera.ActivateCamera();
				}
				break;
			case TransitionRequirement.OnePlayer:
				if (!linkedCamera.activated && other.tag == "Player")
				{
					linkedCamera.ActivateCamera();
				}
				break;
		}
	}


}
