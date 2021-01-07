using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SettingsDefaultValues", menuName = "GlobalDatas/Settings")]
public class SettingsDefaultValues : ScriptableObject
{
	[System.Serializable]
	public class SettingDefaultValue
	{
		public string ID;
		public float value;
	}

	public SettingDefaultValue[] defaultValues;
	
	public static float GetDefaultValue(string ID)
	{
		SettingsDefaultValues sdvs = (SettingsDefaultValues)Resources.Load("SettingsDefaultValues");
		foreach (SettingDefaultValue sdv in sdvs.defaultValues)
		{
			if (sdv.ID.Equals(ID))
			{
				return sdv.value;
			}
		}
		return 1;
	}

	public static void ApplyDefaultSettings()
	{
		SettingsDefaultValues sdvs = (SettingsDefaultValues)Resources.Load("SettingsDefaultValues");
		foreach (SettingDefaultValue sdv in sdvs.defaultValues)
		{
			float savedValue = PlayerPrefs.GetFloat(sdv.ID, -1000);
			if (savedValue == -1000)
			{
				PlayerPrefs.SetFloat(sdv.ID, sdv.value);
			}
		}
		SettingsMenu.instance.ApplySettings();
	}
}
