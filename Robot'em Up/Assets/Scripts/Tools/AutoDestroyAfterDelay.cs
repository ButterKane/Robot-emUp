using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroyAfterDelay : MonoBehaviour
{
	public float delay;
	public AutoDestroyAfterDelay(float _delay)
	{
		delay = _delay;
	}

	private void Update ()
	{
		if (delay < 0)
		{
			Destroy(this.gameObject);
		} else
		{
			delay -= Time.deltaTime;
		}
	}
}
