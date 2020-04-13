using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class WarningPanel : MonoBehaviour
{
	private Animator animator;
	private static WarningPanel instance;
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

		gameObject.SetActive(false);
	}

	private void DisablePanel()
	{
		instance.gameObject.SetActive(false);
	}

	public static void OpenPanel()
	{
		instance.gameObject.SetActive(true);
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
}
