using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinusLight : MonoBehaviour
{
	public float sinusSpeed;
	public float rotationSpeed;
	public Light light;

	private float defaultIntensity;
	private Vector3 defaultScale;


	private void Awake ()
	{
		defaultIntensity = light.intensity;
		defaultScale = transform.localScale;
	}

	private void Update ()
	{
		float sinValue = Mathf.Sin(Time.time * sinusSpeed);
		transform.Rotate(transform.up, Time.deltaTime * rotationSpeed);
		transform.localScale = Vector3.Lerp(Vector3.zero, defaultScale, sinValue);
		light.intensity = Mathf.Lerp(0f, defaultIntensity, sinValue);
	}
}
