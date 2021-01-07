using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XInputDotNetPure;

public class Setting : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField] private string id = "setting";

	[SerializeField] [TextArea(15, 20)] private string description;
	[Header("Reference")]
	[SerializeField] private Image resetIcon;

	private ISettingValue value;

	private void Awake ()
	{
		value = GetComponentInChildren<ISettingValue>();
		value.Init(this);
		UpdateResetIcon();
	}

	private void UpdateResetIcon()
	{
		float defaultValue = SettingsDefaultValues.GetDefaultValue(id);
		if (value.GetValue().Equals(defaultValue))
		{
			resetIcon.enabled = false;
		} else
		{
			resetIcon.enabled = true;
		}
	}

	public void ResetValue()
	{
		value.ResetValue();
		UpdateResetIcon();
	}

	public void IncreaseValue ()
	{
		value.IncreaseValue();
		UpdateResetIcon();
	}

	public void DecreaseValue()
	{
		value.DecreaseValue();
		UpdateResetIcon();
	}

	public string GetDescription()
	{
		return description;
	}

	public string GetID()
	{
		return id;
	}
}
