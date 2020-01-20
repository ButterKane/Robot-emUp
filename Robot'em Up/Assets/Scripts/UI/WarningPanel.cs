using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class WarningPanel : MonoBehaviour
{
	private Animator animator;
	private static WarningPanel instance;
	private GameObject go;
	[MinMaxRange(0, 1)] public RangedFloat vignetteIntensity;
	public Color vignetteColor;
	public float flashSpeed = 1f;

	private Vignette vignette;
	private void Awake ()
	{
		instance = this;
		PostProcessProfile i_postProcessVolumeProfile = Camera.main.GetComponent<PostProcessVolume>().profile;
		if (!i_postProcessVolumeProfile.TryGetSettings(out vignette))
		{
			i_postProcessVolumeProfile.AddSettings<Vignette>();
		}
		i_postProcessVolumeProfile.TryGetSettings(out vignette);
		vignette.color.value = vignetteColor;
		animator = GetComponent<Animator>();
		go = gameObject;
		gameObject.SetActive(false);
	}

	public void DisablePanel()
	{
		instance.go.SetActive(false);
	}

	public void DisableVignette()
	{
		StartCoroutine(DisableVignette_C(0.3f));
	}
	public static void OpenPanel()
	{
		instance.go.SetActive(true);
		instance.animator.SetTrigger("Init");
		instance.vignette.enabled.value = true;
	}

	public static void ClosePanel()
	{
		instance.animator.SetTrigger("Close");
	}

	public static void ClosePanelInstantly()
	{
		instance.vignette.intensity.value = 0;
		instance.vignette.enabled.value = false;
		instance.DisablePanel();
	}

	private void Update ()
	{
		vignette.intensity.value = Mathf.Lerp(vignetteIntensity.Min, vignetteIntensity.Max, Mathf.PingPong(Time.time * flashSpeed, 1f));
	}

	IEnumerator DisableVignette_C(float _disableDuration)
	{
		float i_vignetteCurrentIntensity = vignette.intensity.value;
		for (float i = 0; i < _disableDuration; i+=Time.deltaTime)
		{
			vignette.intensity.value = Mathf.Lerp(i_vignetteCurrentIntensity, 0, i/ _disableDuration);
			yield return new WaitForEndOfFrame();
		}
		instance.vignette.enabled.value = false;
	}
}
