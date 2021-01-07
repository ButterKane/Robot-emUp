using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SettingValueSelector : MonoBehaviour, ISettingValue
{
	public static float selectionAnimationSpeed = 0.1f;

	public static float minDelayBeforeSwitch = 0.2f;
	public static float maxDelayBeforeSwitch = 0.5f;
	public static float timeToGetToMinDelay = 3f;

	[Header("References")]
	[SerializeField] private SettingPreset[] presets = default; //List of available presets
	[SerializeField] private Image selector = default; //Image to show selected preset

	private SettingPreset currentlySelectedPreset;
	private int currentlySelectedPresetIndex;
	private Setting linkedSetting;

	private bool modifyingValue = false;

	private float currentDelayBeforeSwitch;
	private float switchingDuration;
	private bool canSwitchValue;

	private float lastSwitchedTime;

	public void Init ( Setting setting )
	{
		linkedSetting = setting;

		//Get saved value
		float savedValue = PlayerPrefs.GetFloat(linkedSetting.GetID(), -1f);
		if (savedValue == -1)
		{
			savedValue = SettingsDefaultValues.GetDefaultValue(linkedSetting.GetID());
		}
		//Select the preset closest to saved value
		float closestDifference = Mathf.Infinity;
		int closestIndex = 0;
		for (int i = 0; i < presets.Length; i++)
		{
			float difference = Mathf.Abs(Mathf.Abs(presets[i].GetValue()) - Mathf.Abs(savedValue));
			if (difference < closestDifference) { closestIndex = i; closestDifference = difference; }
		}
		SelectPreset(closestIndex);
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

	public void ResetValue()
	{
		float defaultValue = SettingsDefaultValues.GetDefaultValue(linkedSetting.GetID());
		float closestDifference = Mathf.Infinity;
		int closestIndex = 0;
		for (int i = 0; i < presets.Length; i++)
		{
			float difference = Mathf.Abs(Mathf.Abs(presets[i].GetValue()) - Mathf.Abs(defaultValue));
			if (difference < closestDifference) { closestIndex = i; closestDifference = difference; }
		}
		SelectPreset(closestIndex);
	}

	public void SelectPreset(int index)
	{
		if (index >= presets.Length) index = 0;
		if (index < 0) index = presets.Length - 1;

		currentlySelectedPreset = presets[index];
		currentlySelectedPresetIndex = index;

		Invoke("MoveSelector", Time.deltaTime);
		PlayerPrefs.SetFloat(linkedSetting.GetID(), GetValue());
		SettingsMenu.instance.ApplyDirectSettings();
	}

	private void MoveSelector()
	{
		//Place selector at correct position
		selector.transform.SetParent(currentlySelectedPreset.transform, true);
		selector.transform.DOMove(currentlySelectedPreset.transform.position, selectionAnimationSpeed).SetEase(Ease.OutSine).SetUpdate(true); ;
		selector.rectTransform.DOSizeDelta(currentlySelectedPreset.GetComponent<RectTransform>().sizeDelta, selectionAnimationSpeed).SetEase(Ease.OutSine).SetUpdate(true); ;
	}

	public void DecreaseValue ()
	{
		modifyingValue = true;
		if (!canSwitchValue) return;
		SelectPreset(currentlySelectedPresetIndex - 1);
		lastSwitchedTime = Time.unscaledTime;
		canSwitchValue = false;
	}

	public float GetValue ()
	{
		return currentlySelectedPreset.GetValue();
	}

	public void IncreaseValue ()
	{
		modifyingValue = true;
		if (!canSwitchValue) return;
		SelectPreset(currentlySelectedPresetIndex + 1);
		lastSwitchedTime = Time.unscaledTime;
		canSwitchValue = false;
	}
}
