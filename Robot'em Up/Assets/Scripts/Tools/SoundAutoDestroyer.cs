using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundAutoDestroyer : MonoBehaviour
{
	AudioSource source;
	private void Awake ()
	{
		source = GetComponent<AudioSource>();
	}
	private void Update ()
	{
		if (!source.loop && !source.isPlaying)
		{
			Destroy(this.gameObject);
		}
	}
}
