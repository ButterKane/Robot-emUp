using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingPreset : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField] private float value;

	public float GetValue()
	{
		return value;
	}
}
