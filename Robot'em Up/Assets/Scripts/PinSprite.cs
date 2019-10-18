using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinSprite : MonoBehaviour
{
	private Vector3 startPosition;

	private void Awake ()
	{
		startPosition = transform.position;
	}

	private void LateUpdate ()
	{
		transform.position = startPosition;
	}
}
