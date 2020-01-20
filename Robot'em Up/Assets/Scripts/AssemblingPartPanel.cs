using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AssemblingPartPanel : MonoBehaviour
{
	private bool fillAsked = false;
	private Slider fillSlider;
	private float currentFillHoldDuration = 0;
	[HideInInspector] public PlayerController revivedPlayer;
	[HideInInspector] public PlayerController revivingPlayer;

	private void Awake ()
	{
		fillSlider = transform.Find("Slider").GetComponent<Slider>();
	}
	private void LateUpdate ()
	{
		if (fillAsked)
		{
			currentFillHoldDuration += Time.deltaTime;
			fillAsked = false;
		} else
		{
			currentFillHoldDuration = 0;
		}
		fillSlider.value = currentFillHoldDuration / revivedPlayer.reviveHoldDuration;
		if (fillSlider.value >= 1)
		{
			revivingPlayer.Revive(revivedPlayer);
			Destroy(this.gameObject);
		}
	}
	public void FillAssemblingSlider()
	{
		fillAsked = true;
	}
}
