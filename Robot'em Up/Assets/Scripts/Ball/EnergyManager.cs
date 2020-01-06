using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyManager : MonoBehaviour
{
	public static EnergyManager instance;
	public static EnergyData datas;
	private static float currentEnergy;
	private static float wantedEnergy;

	private void Awake ()
	{
		instance = this;
	}

	private void Update ()
	{
		currentEnergy = Mathf.Lerp(currentEnergy, wantedEnergy, Time.deltaTime * datas.energyLerpSpeed);
	}

	public static float GetEnergy ()
	{
		return wantedEnergy;
	}

	public static float GetDisplayedEnergy()
	{
		return currentEnergy;
	}

	public static float IncreaseEnergy ( float _amount )
	{
		wantedEnergy += _amount;
		wantedEnergy = ClampEnergy(wantedEnergy);
		return wantedEnergy;
	}

	public static float MultiplyEnergy ( float _coef )
	{
		wantedEnergy *= _coef;
		wantedEnergy = ClampEnergy(wantedEnergy);
		return wantedEnergy;
	}

	public static bool DecreaseEnergy ( float _amount )
	{
		if (wantedEnergy < _amount)
		{
			wantedEnergy = 0;
			return false;
		}
		else
		{
			wantedEnergy -= _amount;
		}
		wantedEnergy = ClampEnergy(wantedEnergy);
		return true;
	}

	private static float ClampEnergy ( float _energy )
	{
		return Mathf.Clamp(_energy, 0f, 1f);
	}
}
