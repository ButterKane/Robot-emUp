﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class WarningPanel : MonoBehaviour
{
	private Animator animator;
	private static WarningPanel instance;
	private GameObject go;
	[MinMaxSlider] public Vector2 vignetteIntensity;
	public Color vignetteColor;
	public float flashSpeed = 1f;

	private Vignette vignette;
	private void Awake ()
	{
		instance = this;
		PostProcessProfile postProcessVolumeProfile = Camera.main.GetComponent<PostProcessVolume>().profile;
		if (!postProcessVolumeProfile.TryGetSettings(out vignette))
		{
			postProcessVolumeProfile.AddSettings<Vignette>();
		}
		postProcessVolumeProfile.TryGetSettings(out vignette);
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

	private void Update ()
	{
		vignette.intensity.value = Mathf.Lerp(vignetteIntensity.x, vignetteIntensity.y, Mathf.PingPong(Time.time * flashSpeed, 1f));
	}

	IEnumerator DisableVignette_C(float _disableDuration)
	{
		float vignetteCurrentIntensity = vignette.intensity.value;
		for (float i = 0; i < _disableDuration; i+=Time.deltaTime)
		{
			vignette.intensity.value = Mathf.Lerp(vignetteCurrentIntensity, 0, i/ _disableDuration);
			yield return new WaitForEndOfFrame();
		}
		instance.vignette.enabled.value = false;
	}
}
