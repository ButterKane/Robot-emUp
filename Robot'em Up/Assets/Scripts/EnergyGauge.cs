using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
#pragma warning disable 0649

public class EnergyGauge : MonoBehaviour
{
	[SerializeField] private Image gaugeFillInstant;
	[SerializeField] private Image gaugeFillLerped;

	float previousValue;
	private Vector3 initialScale;

	private void Update ()
	{
		UpdateGauge();
	}
	public void UpdateGauge()
	{
		gaugeFillInstant.fillAmount = EnergyManager.GetEnergy();
		gaugeFillLerped.fillAmount = Mathf.Lerp(gaugeFillLerped.fillAmount, EnergyManager.GetDisplayedEnergy(), Time.deltaTime * 2f);
		float currentValue = gaugeFillInstant.fillAmount;

		if (Mathf.Abs(currentValue - previousValue) >= 0.1f)
		{
			transform.DOShakeScale(0.1f, 0.1f).OnComplete(ResetScale);
		}
		previousValue = currentValue;
	}

	private void Awake ()
	{
		initialScale = transform.localScale;
	}

	public void ResetScale ()
	{
		transform.DOScale(initialScale, 0.1f);
	}
}
