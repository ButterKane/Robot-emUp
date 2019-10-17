using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MomentumManager: MonoBehaviour
{
	[Header("Settings")]
	public string nothingYet;

	private static float currentMomentum;
	public void Awake ()
	{
		currentMomentum = 0;
	}

	public static float GetMomentum()
	{
		return currentMomentum;
	}

	public static float IncreaseMomentum(float _amount)
	{
		currentMomentum += _amount;
		currentMomentum = ClampMomentum(currentMomentum);
		return currentMomentum;
	}

	public static float MultiplyMomentum(float _coef)
	{
		currentMomentum *= _coef;
		currentMomentum = ClampMomentum(currentMomentum);
		return currentMomentum;
	}

	public static bool DecreaseMomentum ( float _amount )
	{
		if (currentMomentum < _amount)
		{
			currentMomentum = 0;
			return false;
		} else
		{
			currentMomentum -= _amount;
		}
		currentMomentum = ClampMomentum(currentMomentum);
		return true;
	}

	private static float ClampMomentum(float _momentum)
	{
		return Mathf.Clamp(_momentum, 0f, 1f);
	}
}
