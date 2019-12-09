﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalFader : MonoBehaviour
{
	private Material material;
	public float fadeDelay;
	public float fadeDuration;

	public void Construct(float _delay, float _duration)
	{
		fadeDelay = _delay;
		fadeDuration = _duration;
		material = GetComponent<MeshRenderer>().sharedMaterial = new Material(GetComponent<MeshRenderer>().sharedMaterial);
		StartCoroutine(Fade_C());
	}

	IEnumerator Fade_C ()
	{
		yield return new WaitForSeconds(fadeDelay);
		Color startColor = material.color;
		Color endColor = startColor;
		endColor.a = 0;
		for (float i = 0; i < fadeDuration; i+=Time.deltaTime)
		{
			material.color = Color.Lerp(startColor, endColor, i / fadeDuration);
			yield return null;
		}
		Destroy(gameObject);
	}
}
