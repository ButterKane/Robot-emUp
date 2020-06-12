using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBillboard : MonoBehaviour
{
	public Transform followedTransform;
	public float scaleModifier = 20f;
	public int frameDelayBeforeParenting = 2;
	public int yOffset = 50;

	private int currentFrameDelay = 0;
	private bool resized = false;

	private void Start ()
	{
		transform.SetParent(GameManager.mainCanvas.transform, false);
	}
	private void FixedUpdate ()
	{
		if (!resized)
		{
			if (currentFrameDelay < frameDelayBeforeParenting) {
				currentFrameDelay++;
			} else
			{
				resized = true;
				transform.localScale = transform.localScale * scaleModifier;
			}
		}
		if (followedTransform != null && Camera.current != null)
		{
			Vector3 _newPosition = transform.position = Camera.current.WorldToScreenPoint(followedTransform.position);
			_newPosition += Vector3.up * yOffset;
			transform.position = _newPosition;
			transform.localRotation = Quaternion.identity;
		}
	}
}
