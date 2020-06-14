using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BumpEffectUI : MonoBehaviour
{
	private void Awake ()
	{
		transform.DOShakeScale(1, 0.25f).SetLoops(-1);
	}
}
