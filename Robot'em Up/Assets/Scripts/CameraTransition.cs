using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraTransitionRequierement { OnePlayer, TwoPlayer, MiddlePoint }
public class CameraTransition : MonoBehaviour
{
	public CameraTransitionRequierement transitionCondition = CameraTransitionRequierement.OnePlayer;
	public CameraBehaviour linkedCamera;

	private void OnTriggerEnter ( Collider other )
	{
		if (!linkedCamera.activated && other.tag == "Player")
		{
			linkedCamera.ActivateCamera();
		}
	}

}
