using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicLoader : MonoBehaviour
{
	public string musicID;

	public bool loadOnAwake = false;

	private void Start ()
	{
		if (loadOnAwake)
		{
			LoadMusic();
		}
	}
	public void LoadMusic ()
	{
		MusicManager.PlayMusic(musicID);
	}


	public void StopMusic(float time)
	{
		MusicManager.StopMusic(time);
	}
}
