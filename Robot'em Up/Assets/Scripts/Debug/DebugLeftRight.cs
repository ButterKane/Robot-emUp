using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLeftRight : MonoBehaviour
{
	public Vector3 firstPos;
	public Vector3 secondPos;
	public float speedMultiplier;

	private Vector3 startPosition;
	int dir;
	float t;

	private void Awake ()
	{
		startPosition = transform.position;
	}
	void Update()
    {
		t += Time.deltaTime * speedMultiplier * dir;
		transform.position = Vector3.Lerp(startPosition+firstPos, startPosition+secondPos, t);
		if (Vector3.Distance(transform.position, startPosition+secondPos) <= 0.1)
		{
			dir = -1;
		}
		if (Vector3.Distance(transform.position, startPosition+firstPos) <= 0.1)
		{
			dir = 1;
		}
	}
}
