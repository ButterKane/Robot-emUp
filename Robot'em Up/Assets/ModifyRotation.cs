using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ModifyRotation : MonoBehaviour
{
	[Header("Settings")]
	public Vector3 newRotation;
	public float transitionDuration;


	public void Rotate()
	{
		transform.DORotate(newRotation, transitionDuration).SetEase(Ease.OutSine);
	}
}
