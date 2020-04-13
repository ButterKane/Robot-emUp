using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MusicDatas", menuName = "GlobalDatas/MusicDatas")]
public class MusicDatas : ScriptableObject
{
	public static MusicDatas datas;
	public List<MusicData> musicDatas = new List<MusicData>();

	public static MusicDatas GetMusicDatas()
	{
		return Resources.Load<MusicDatas>("MusicDatas");
	}

	public static MusicData GetMusicData(string _name)
	{
		if (datas == null) { datas = GetMusicDatas(); }
		foreach (MusicData d in datas.musicDatas)
		{
			if (d.id == _name)
			{
				return d;
			}
		}
		return null;
	}
}

[System.Serializable]
public class MusicData
{
	public string id = "New music";
	public AudioClip clip;
	public bool isLooping = true;
	public float fadeInDuration = 1f;
	public float fadeOutDuration = 1f;
}
