using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using XInputDotNetPure;
using DG.Tweening;

public class SettingsMenu : MonoBehaviour
{
	public static SettingsMenu instance;
	public static float triggerTreshold = 0.49f;

	[Header("Settings")]
	[SerializeField] private float selectionSpeed = 0.1f;

	[Header("References")]
	[SerializeField] private TextMeshProUGUI description;
	[SerializeField] private Image selector;

	private Setting[] settings;
	private Setting currentlySelectedSetting;
	private bool opened;
	private Canvas canvas;

	private bool canSwitchSetting;
	private bool canReset;

	private void Awake ()
	{
		instance = this;
		canvas = GetComponent<Canvas>();
		settings = GetComponentsInChildren<Setting>();
	}

	private void Update ()
	{
		if (currentlySelectedSetting == null || !opened || LoadingScreen.loading) return;
		GetInputs();
	}
	
	private void GetInputs()
	{
		GamePadState state = GamePad.GetState(PlayerIndex.One);
		float newTreshold = triggerTreshold * (1/PlayerPrefs.GetFloat("gamepad.sensivity", 1f));
		if (state.ThumbSticks.Left.X > newTreshold || state.DPad.Right == ButtonState.Pressed)
		{
			currentlySelectedSetting.IncreaseValue();
		}
		if (state.ThumbSticks.Left.X < -newTreshold || state.DPad.Left == ButtonState.Pressed)
		{
			currentlySelectedSetting.DecreaseValue();
		}
		if (state.ThumbSticks.Left.Y > newTreshold || state.DPad.Up == ButtonState.Pressed)
		{
			SelectPreviousSetting();
		}
		else if (state.ThumbSticks.Left.Y < -newTreshold || state.DPad.Down == ButtonState.Pressed)
		{
			SelectNextSetting();
		}
		else if (!canSwitchSetting)
		{
			canSwitchSetting = true;
		}
		if (state.Buttons.B == ButtonState.Pressed)
		{
			Close();
		}
		if (state.Buttons.Start == ButtonState.Pressed)
		{
			Close();
		}
		if (state.Buttons.Back == ButtonState.Pressed)
		{
			if (!canReset) return;
			currentlySelectedSetting.ResetValue();
			canReset = false;
		}
		else
		{
			canReset = true;
		}
	}

	public void Open()
	{
		if (opened) return;
		FeedbackManager.SendFeedback("event.PressSettings", this);
		SelectSetting(PlayerPrefs.GetInt("selectedSetting", 0));
		if (IngameMenu.instance != null)
			IngameMenu.instance.CloseMainPanel();
		opened = true;
		canvas.enabled = true;
	}

	public void Close()
	{
		FeedbackManager.SendFeedback("event.MenuBack", this);
		if (IngameMenu.instance != null)
			IngameMenu.instance.OpenMainPanel();
		if (MainMenu.instance != null)
		{
			MainMenu.instance.CloseSettings();
		}
		opened = false;
		canvas.enabled = false;
		ApplySettings();
	}

	public void SelectSetting(int id)
	{
		PlayerPrefs.SetInt("selectedSetting", id);
		currentlySelectedSetting = settings[id];
		description.text = currentlySelectedSetting.GetDescription();
		selector.transform.DOMove(currentlySelectedSetting.transform.position, selectionSpeed).SetEase(Ease.OutSine).SetUpdate(true);
	}

	private void SelectNextSetting()
	{
		if (!canSwitchSetting) return;
		int newSelectedID = PlayerPrefs.GetInt("selectedSetting", 0);
		newSelectedID++;
		newSelectedID = Mathf.Clamp(newSelectedID, 0, settings.Length-1);
		SelectSetting(newSelectedID);
		canSwitchSetting = false;
	}

	private void SelectPreviousSetting()
	{
		if (!canSwitchSetting) return;
		int newSelectedID = PlayerPrefs.GetInt("selectedSetting", 0);
		newSelectedID--;
		newSelectedID = Mathf.Clamp(newSelectedID, 0, settings.Length - 1);
		SelectSetting(newSelectedID);
		canSwitchSetting = false;
	}


	//Settings that are updated when menu is closed
	public void ApplySettings()
	{
		Time.timeScale = PlayerPrefs.GetFloat("gamespeed", 1f);
		ApplyDirectSettings();
	}


	//Settings that are updated when a value is changed
	public void ApplyDirectSettings()
	{
		if (PostProcessManager.i != null)
		{
			PostProcessManager.i.SetContrast(PlayerPrefs.GetFloat("contrast", 1f));
		}
		if (MusicManager.instance != null)
		{
			MusicManager.ResetVolume();
		}
	}
}
