using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrabPreview : MonoBehaviour
{
	[SerializeField] private Image inputRequiredRenderer;
	[SerializeField] private Sprite gamepadRequiredInput;
	[SerializeField] private Sprite keyboardRequiredInput;
	public void Init(PlayerController.ControllerType controllerType)
	{
		switch (controllerType)
		{
			case PlayerController.ControllerType.Gamepad:
				inputRequiredRenderer.sprite = gamepadRequiredInput;
				break;
			case PlayerController.ControllerType.Keyboard:
				inputRequiredRenderer.sprite = keyboardRequiredInput;
				break;
		}
	}
}
