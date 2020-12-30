using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AssemblingPartPanel : MonoBehaviour
{
	private bool fillAsked = false;
	private Slider fillSlider;
	private float currentFillHoldDuration = 0;
	[HideInInspector] public PlayerController revivedPlayer;
	[HideInInspector] public PlayerController revivingPlayer;

	[SerializeField] private GameObject[] keyboardControls;
	[SerializeField] private GameObject[] gamepadControls;

	private void Awake ()
	{
		fillSlider = transform.Find("Slider").GetComponent<Slider>();
	}

	public void Init()
	{
		switch (revivingPlayer.controllerType)
		{
			case PlayerController.ControllerType.Keyboard:
				foreach (GameObject go in gamepadControls)
				{
					go.SetActive(false);
				}
				break;
			case PlayerController.ControllerType.Gamepad:
				foreach (GameObject go in keyboardControls)
				{
					go.SetActive(false);
				}
				break;
		}
		foreach (PressableUITrigger puit in GetComponentsInChildren<PressableUITrigger>())
		{
			puit.Init(revivingPlayer.playerIndex, revivingPlayer.controllerType);
		}
	}
	private void LateUpdate ()
	{
		transform.position = GameManager.mainCamera.WorldToScreenPoint(revivingPlayer.transform.position);
		if (fillAsked)
		{
			currentFillHoldDuration += Time.deltaTime;
			fillAsked = false;
		} else
		{
			currentFillHoldDuration = 0;
		}
		fillSlider.value = currentFillHoldDuration / revivedPlayer.reviveHoldDuration;
		if (fillSlider.value >= 1)
		{
			revivingPlayer.Revive(revivedPlayer);
			Destroy(this.gameObject);
		}
	}
	public void FillAssemblingSlider()
	{
		fillAsked = true;
	}
}
