using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicLoader : MonoBehaviour
{
	public string musicID;

	public void LoadMusic ()
	{
		MusicManager.PlayMusic(musicID);
	}
}
