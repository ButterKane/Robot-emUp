 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundData
{
	public string soundName;
	public List<Sound> soundList;
	[Range(0f, 1f)] public float volumeMultiplier = 1f;

	public void SetPlayRate(Sound _sound, float _newPlayRate)
	{
		if (soundList.Count <= 1) { return; }
		float internal_difference = 0f;
		float internal_otherTotal = 0;
		foreach (Sound wep in soundList)
		{
			if (wep == _sound)
			{
				internal_difference = _newPlayRate - wep.playChances;
				wep.playChances = _newPlayRate;
			}
			else
			{
				internal_otherTotal += wep.playChances;
			}
		}

		//Reduce or improve all the other probabilities (So it keep a maximum of 1)
		foreach (Sound wep in soundList)
		{
			if (wep != _sound)
			{
				if (internal_difference > 0)
				{
					if (internal_otherTotal != 0)
					{
						float force = wep.playChances / internal_otherTotal;
						wep.playChances -= force * internal_difference;
					}
				}
				else
				{
					if ((float)(soundList.Count - 1) - internal_otherTotal != 0)
					{
						float internal_force = (1f - wep.playChances) / ((float)(soundList.Count - 1) - internal_otherTotal);
						wep.playChances -= internal_force * internal_difference;
					}
				}
			}
			wep.playChances = Mathf.Clamp(wep.playChances, 0f, 1f);
		}
	}
	public void RemoveSound(Sound _sound )
	{
		float internal_difference = _sound.playChances;
		soundList.Remove(_sound);
		float internal_totalProba = 0;
		foreach (Sound wep in soundList)
		{
			internal_totalProba += wep.playChances;
		}

		//Reduce or improve all the other probabilities (So it keep a maximum of 1)
		foreach (Sound wep in soundList)
		{
			if (internal_totalProba != 0)
			{
				float internal_force = wep.playChances / internal_totalProba;
				wep.playChances += internal_force * internal_difference;
				wep.playChances = Mathf.Clamp(wep.playChances, 0f, 1f);
			}
			else
			{
				wep.playChances = 1;
			}
		}
	}
}

[System.Serializable]
public class Sound
{
	public AudioClip clip;
	[Range(0f, 1f)] public float playChances;
}

public class SoundManager
{
	public static void PlaySound(string _soundName, Vector3 _worldPosition, Transform _parent = null)
	{
		SoundData internal_soundData = GetSoundData(_soundName);
		AudioClip clip = GetSoundClip(internal_soundData);
		if (clip == null) { Debug.LogWarning("Warning: Clip not found."); return; }
		GameObject internal_newSoundPlayer = new GameObject();
		internal_newSoundPlayer.name = "SoundPlayer";
		AudioSource internal_newAudioSource = internal_newSoundPlayer.AddComponent<AudioSource>();
		internal_newAudioSource.spatialBlend = 0.65f;
		internal_newAudioSource.maxDistance = 100;
		internal_newAudioSource.volume = internal_soundData.volumeMultiplier;
		if (_parent != null)
		{
			internal_newAudioSource.transform.SetParent(_parent);
		}
		internal_newAudioSource.gameObject.AddComponent<SoundAutoDestroyer>();
		internal_newAudioSource.transform.position = _worldPosition;
		internal_newAudioSource.clip = clip;
		internal_newAudioSource.Play();
	}

	public static SoundData GetSoundData(string _soundName)
	{
		SoundDatas internal_soundDatas = Resources.Load<SoundDatas>("SoundDatas");
		foreach (SoundData soundData in internal_soundDatas.soundList)
		{
			if (soundData.soundName == _soundName)
			{
				return soundData;
			}
		}
		Debug.LogWarning("No data for sound with name " + _soundName + " found.");
		return null;
	}
	public static AudioClip GetSoundClip(SoundData _soundData)
	{
		float internal_pickChances = Random.value;
		int internal_chosenIndex = 0;
		float internal_cumulativeChances = _soundData.soundList[0].playChances;
		while (internal_pickChances > internal_cumulativeChances && internal_chosenIndex < _soundData.soundList.Count)
		{
			internal_chosenIndex++;
			internal_cumulativeChances += _soundData.soundList[internal_chosenIndex].playChances;
		}
		return _soundData.soundList[internal_chosenIndex].clip;
	}
}
