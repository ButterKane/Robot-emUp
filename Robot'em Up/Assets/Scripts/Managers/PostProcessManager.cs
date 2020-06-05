using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessManager : MonoBehaviour
{
    public static PostProcessManager i;

    private Bloom bloom;
    private ChromaticAberration chromaticAberration;
    private ColorGrading colorGrading;
    private Grain grain;
    private float defaultContrast = 0;

    private void Awake()
    {
        i = this;

        PostProcessProfile i_postProcessVolumeProfile = Camera.main.GetComponent<PostProcessVolume>().profile;
        if (i_postProcessVolumeProfile == null) { Debug.LogWarning("No post process found, won't update its values"); }

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

    public void MomentumUpdatePostProcess(MomentumData _datas, float _momentumValue)
    {
        //Updates bloom
        bloom.intensity.value = GetValue(_datas.minMaxBloom, _momentumValue);

        //Updates color grading
        colorGrading.temperature.value = GetValue(_datas.minMaxTemperature, _momentumValue);

        //Updates chromatic aberration
        chromaticAberration.intensity.value = GetValue(_datas.minMaxChromaticAberration, _momentumValue);

        //Updates grain
        grain.intensity.value = GetValue(_datas.minMaxGrain, _momentumValue);
    }

    public void UpdateContrastWithSettings()
    {
        colorGrading.contrast.value = PlayerPrefs.GetFloat("REU_Contrast", defaultContrast);
    }

    public static float GetValue(Vector2 _value, float _retrievedMomentumValue)
    {
        return Mathf.Lerp(_value.x, _value.y, _retrievedMomentumValue);
    }
}
