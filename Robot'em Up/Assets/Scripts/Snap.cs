using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class Snap : MonoBehaviour
{
	[Separator("Settings")]
	public SnapType snapType;
	public bool isInfinite;
	[ConditionalField(nameof(isInfinite), inverse: true)] public float duration = 1;
	public float angleTreshold;

	private void Awake ()
	{
		SnapController.snappedObjects.Add(this);
		if (!isInfinite)
		{
			StartCoroutine(KillAfterDelay_C(duration));
		}
	}

	IEnumerator KillAfterDelay_C(float _duration)
	{
		yield return new WaitForSeconds(_duration);
		SnapController.snappedObjects.Remove(this);
		Destroy(this);
	}
}
