using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
#pragma warning disable 0649

public class EnergyGauge : MonoBehaviour
{
	[SerializeField] private Image gaugeFillLerped;
	[SerializeField] private Image gaugeFillRay;
	[SerializeField] private Animator textAnimator;

	float previousValue;
	private Vector3 initialScale;

	private void Update ()
	{
		UpdateGauge();
		if (Input.GetKeyDown(KeyCode.I))
		{
			if (EnergyManager.GetEnergy() >= 0.99f)
			{
				EnergyManager.DecreaseEnergy(1f);
			} else
			{
				EnergyManager.IncreaseEnergy(1f);
			}
		}
	}
	public void UpdateGauge()
	{
		float currentValue = EnergyManager.GetEnergy();
		gaugeFillLerped.fillAmount = Mathf.Lerp(gaugeFillLerped.fillAmount, EnergyManager.GetDisplayedEnergy(), Time.deltaTime * 8);
		gaugeFillRay.fillAmount = gaugeFillLerped.fillAmount;

		if (Mathf.Abs(currentValue - previousValue) >= 0.1f)
		{
			transform.DOShakeScale(0.1f, 0.1f).OnComplete(ResetScale);
		}
		previousValue = currentValue;

		if (EnergyManager.GetEnergy() >= 0.99f)
		{
			textAnimator.SetBool("dunkReady", true);
		} else
		{
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
