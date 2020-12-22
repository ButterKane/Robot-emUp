using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DunkReadyPanel : MonoBehaviour
{
	[SerializeField] private Image inputRenderer;
	[SerializeField] private Sprite gamepadInputSprite;
	[SerializeField] private Sprite keyboardInputSprite;

	public void Init(PlayerController.ControllerType ctype)
	{
		switch (ctype)
		{
			case PlayerController.ControllerType.Keyboard:
				inputRenderer.sprite = keyboardInputSprite;
				break;
			case PlayerController.ControllerType.Gamepad:
				inputRenderer.sprite = gamepadInputSprite;
				break;
		}
	}
}
