using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using UnityEngine.Rendering.PostProcessing;


public class MomentumManager: MonoBehaviour
{
	[Header("Settings")]
	public static MomentumData datas;
	private static float currentMomentum;
	private static float wantedMomentum;
	public static MomentumManager instance;
    [ReadOnly] public float postProcessSettingsMod = 1; // Goes from 0 to 1


    private bool exponentialLossEnabled = false;
	private float exponentialLossDuration = 0;
	private float exponentialLossCoef = 1;

	private Bloom bloom;
	private ChromaticAberration chromaticAberration;
	private ColorGrading colorGrading;
    private Grain grain;

	public void Awake ()
	{
		instance = this;
		currentMomentum = 0;
		wantedMomentum = currentMomentum;

		PostProcessProfile i_postProcessVolumeProfile = Camera.main.GetComponent<PostProcessVolume>().profile;
		if (i_postProcessVolumeProfile == null) { Debug.LogWarning("No post process found, momentum won't update it's values"); }

		//Retrieves or add bloom settings
		if (!i_postProcessVolumeProfile.TryGetSettings(out bloom))
		{
			i_postProcessVolumeProfile.AddSettings<Bloom>();
		}
		i_postProcessVolumeProfile.TryGetSettings(out bloom);

		//Retrieves or add chromaticAberration settings
		if (!i_postProcessVolumeProfile.TryGetSettings(out chromaticAberration))
		{
			i_postProcessVolumeProfile.AddSettings<ChromaticAberration>();
		}
		i_postProcessVolumeProfile.TryGetSettings(out chromaticAberration);

		//Retrieves or add colorGrading settings
		if (!i_postProcessVolumeProfile.TryGetSettings(out colorGrading))
		{
			i_postProcessVolumeProfile.AddSettings<ColorGrading>();
		}
		i_postProcessVolumeProfile.TryGetSettings(out colorGrading);

        //Retrieves or add grain settings
        if (!i_postProcessVolumeProfile.TryGetSettings(out grain))
        {
            i_postProcessVolumeProfile.AddSettings<Grain>();
        }
        i_postProcessVolumeProfile.TryGetSettings(out grain);
    }
	private void Update ()
	{
		currentMomentum = Mathf.Lerp(currentMomentum, wantedMomentum, Time.deltaTime * datas.momentumLerpSpeed);
		if (instance.exponentialLossEnabled)
		{
			exponentialLossDuration += Time.deltaTime * exponentialLossCoef;
			wantedMomentum -= Time.deltaTime * exponentialLossDuration;
			wantedMomentum = ClampMomentum(wantedMomentum);
		}
		UpdatePostProcess();
	}

	private void UpdatePostProcess ()
	{
		//Updates bloom
		bloom.intensity.value = GetValue(datas.minMaxBloom)* postProcessSettingsMod;

		//Updates color grading
		colorGrading.temperature.value = GetValue(datas.minMaxTemperature)* postProcessSettingsMod;

		//Updates chromatic aberration
		chromaticAberration.intensity.value = GetValue(datas.minMaxChromaticAberration)* postProcessSettingsMod;

        //Updates grain
        grain.intensity.value = GetValue(datas.minMaxGrain)* postProcessSettingsMod;
    }

	public static float GetMomentum()
	{
		return wantedMomentum;
	}

	public static float GetDisplayedMomentum()
	{
		return currentMomentum;
	}

	public static void EnableMomentumExponentialLoss(float _delayBeforeStarting, float _multiplyCoef)
	{
		instance.exponentialLossCoef = _multiplyCoef;
		instance.StartCoroutine(instance.EnableMomentumContinuousLossAfterDelay(_delayBeforeStarting));
	}
	public static void DisableMomentumExpontentialLoss ()
	{
		instance.StopAllCoroutines();
		instance.exponentialLossEnabled = false;
		instance.exponentialLossDuration = 0;
	}

	public static float IncreaseMomentum(float _amount)
	{
		wantedMomentum += _amount;
		wantedMomentum = ClampMomentum(wantedMomentum);
		return wantedMomentum;
	}

	public static float MultiplyMomentum(float _coef)
	{
		wantedMomentum *= _coef;
		wantedMomentum = ClampMomentum(wantedMomentum);
		return wantedMomentum;
	}

	public static bool DecreaseMomentum ( float _amount )
	{
		if (wantedMomentum < _amount)
		{
			wantedMomentum = 0;
			return false;
		} else
		{
			wantedMomentum -= _amount;
		}
		wantedMomentum = ClampMomentum(wantedMomentum);
		return true;
	}

	private static float ClampMomentum(float _momentum)
	{
		return Mathf.Clamp(_momentum, 0f, 1f);
	}

	public static float GetValue( Vector2 _value )
	{
		return Mathf.Lerp(_value.x, _value.y, currentMomentum);
	}

	IEnumerator EnableMomentumContinuousLossAfterDelay(float _delay)
	{
		yield return new WaitForSeconds(_delay);
		instance.exponentialLossEnabled = true;
	}
}
