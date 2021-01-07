using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class SettingValueSlider : MonoBehaviour, ISettingValue
{
	public static float selectionAnimationSpeed = 0.1f;

	public static float minDelayBeforeSwitch = 0.05f;
	public static float maxDelayBeforeSwitch = 0.2f;
	public static float timeToGetToMinDelay = 0.75f;
	public static float changeAmount = 0.1f;

	[Header("References")]
	[SerializeField] private Slider slider = default;
	[SerializeField] private TextMeshProUGUI sliderValueText = default;

	private Setting linkedSetting;

	private bool modifyingValue = false;

	private float currentDelayBeforeSwitch;
	private float switchingDuration;
	private bool canSwitchValue;

	private float lastSwitchedTime;
	private float speedMultiplier;

	public void Init ( Setting setting )
	{
		linkedSetting = setting;
		speedMultiplier = (Mathf.Abs(slider.maxValue) + Mathf.Abs(slider.minValue)) / 2f;

		//Get saved value
		float savedValue = PlayerPrefs.GetFloat(linkedSetting.GetID(), -1f);
		if (savedValue == -1)
		{
			SetSliderValue(SettingsDefaultValues.GetDefaultValue(linkedSetting.GetID()));
		}
		else
		{
			SetSliderValue(savedValue);
		}
	}

	private void Update ()
	{
		if (!canSwitchValue)
		{
			if ((Time.unscaledTime - lastSwitchedTime) > currentDelayBeforeSwitch) { canSwitchValue = true; }
		}
	}

	public void LateUpdate ()
	{
		//Player is pressing trigger
		if (modifyingValue)
		{
			modifyingValue = false;
			switchingDuration += Time.unscaledDeltaTime;
			currentDelayBeforeSwitch = Mathf.Lerp(maxDelayBeforeSwitch, minDelayBeforeSwitch, switchingDuration / timeToGetToMinDelay);
		}
		else
		//Player is not pressing trigger
		{
			switchingDuration = 0;
			canSwitchValue = true;
		}
	}

	public void ResetValue ()
	{
		float defaultValue = SettingsDefaultValues.GetDefaultValue(linkedSetting.GetID());
		SetSliderValue(defaultValue);
	}

	private void SetSliderValue(float value)
	{
		slider.DOValue(value, selectionAnimationSpeed).SetEase(Ease.OutSine).SetUpdate(true);
		sliderValueText.text = value.ToString();
		PlayerPrefs.SetFloat(linkedSetting.GetID(), value);
		SettingsMenu.instance.ApplyDirectSettings();
	}

	public void DecreaseValue ()
	{
		modifyingValue = true;
		if (!canSwitchValue) return;
		lastSwitchedTime = Time.unscaledTime;
		float newValue = GetValue() - (changeAmount * speedMultiplier);
		newValue = Mathf.Clamp(newValue, slider.minValue, slider.maxValue);
		newValue = (Mathf.Round(newValue * 10f)) / 10f;
		canSwitchValue = false;
		SetSliderValue(newValue);
	}

	public float GetValue ()
	{
		return PlayerPrefs.GetFloat(linkedSetting.GetID(), 1f);
	}

	public void IncreaseValue ()
	{
		modifyingValue = true;
		if (!canSwitchValue) return;
		lastSwitchedTime = Time.unscaledTime;
		float newValue = GetValue() + (changeAmount * speedMultiplier);
		newValue = Mathf.Clamp(newValue, slider.minValue, slider.maxValue);
		newValue = (Mathf.Round(newValue * 10f)) / 10f;
		canSwitchValue = false;
		SetSliderValue(newValue);
	}
}
