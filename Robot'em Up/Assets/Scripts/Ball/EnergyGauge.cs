using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
#pragma warning disable 0649

public class EnergyGauge : MonoBehaviour
{
	[SerializeField] private Color defaultGaugeColor;
	[SerializeField] private Color fullGaugeColor;

	[SerializeField] private Image gaugeFillLerped;
	[SerializeField] private Image gaugeFillRay;
	[SerializeField] private Animator textAnimator;

	float previousValue;
	private Vector3 initialScale;

	private void Update ()
	{
		UpdateGauge();
	}
	public void UpdateGauge()
	{
		float internal_currentValue = EnergyManager.GetEnergy();
		gaugeFillLerped.fillAmount = Mathf.Lerp(gaugeFillLerped.fillAmount, EnergyManager.GetDisplayedEnergy(), Time.deltaTime * 8);
		gaugeFillRay.fillAmount = gaugeFillLerped.fillAmount;

		if (Mathf.Abs(internal_currentValue - previousValue) >= 0.1f)
		{
			transform.DOShakeScale(0.1f, 0.1f).OnComplete(ResetScale);
		}
		previousValue = internal_currentValue;

		if (EnergyManager.GetEnergy() >= 0.99f)
		{
			textAnimator.SetBool("dunkReady", true);
			gaugeFillLerped.color = new Color(fullGaugeColor.r, fullGaugeColor.g, fullGaugeColor.b, gaugeFillLerped.color.a);
			gaugeFillRay.color = new Color(fullGaugeColor.r, fullGaugeColor.g, fullGaugeColor.b, gaugeFillLerped.color.a);
		} else
		{
			gaugeFillLerped.color = new Color(defaultGaugeColor.r, defaultGaugeColor.g, defaultGaugeColor.b, gaugeFillLerped.color.a);
			gaugeFillRay.color = new Color(defaultGaugeColor.r, defaultGaugeColor.g, defaultGaugeColor.b, gaugeFillLerped.color.a);
			textAnimator.SetBool("dunkReady", false);
		}
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
