using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MainMenuCameraPivot : MonoBehaviour
{
	public static MainMenuCameraPivot instance;

	private void Awake ()
	{
		instance = this;
	}


	public static void RotateToCredits()
	{
		instance.transform.DORotate(new Vector3(0, 60, 0), 0.2f).SetEase(Ease.OutSine).SetUpdate(false);
	}

	public static void RotateToMainPanel()
	{
		instance.transform.DORotate(Vector3.zero, 0.2f).SetEase(Ease.OutSine).SetUpdate(false);
	}
}
