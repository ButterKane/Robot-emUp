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
		float difference = 0f;
		float otherTotal = 0;
		foreach (Sound wep in soundList)
		{
			if (wep == _sound)
			{
				difference = _newPlayRate - wep.playChances;
				wep.playChances = _newPlayRate;
			}
			else
			{
				otherTotal += wep.playChances;
			}
		}

		//Reduce or improve all the other probabilities (So it keep a maximum of 1)
		foreach (Sound wep in soundList)
		{
			if (wep != _sound)
			{
				if (difference > 0)
				{
					if (otherTotal != 0)
					{
						float force = wep.playChances / otherTotal;
						wep.playChances -= force * difference;
					}
				}
				else
				{
					if ((float)(soundList.Count - 1) - otherTotal != 0)
					{
						float force = (1f - wep.playChances) / ((float)(soundList.Count - 1) - otherTotal);
						wep.playChances -= force * difference;
					}
				}
			}
			wep.playChances = Mathf.Clamp(wep.playChances, 0f, 1f);
		}
	}
	public void RemoveSound(Sound _sound )
	{
		float difference = _sound.playChances;
		soundList.Remove(_sound);
		float totalProba = 0;
		foreach (Sound wep in soundList)
		{
			totalProba += wep.playChances;
		}

		//Reduce or improve all the other probabilities (So it keep a maximum of 1)
		foreach (Sound wep in soundList)
		{
			if (totalProba != 0)
			{
				float force = wep.playChances / totalProba;
				wep.playChances += force * difference;
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
		SoundData soundData = GetSoundData(_soundName);
		AudioClip clip = GetSoundClip(soundData);
		if (clip == null) { Debug.LogWarning("Warning: Clip not found."); return; }
		GameObject newSoundPlayer = new GameObject();
		newSoundPlayer.name = "SoundPlayer";
		AudioSource newAudioSource = newSoundPlayer.AddComponent<AudioSource>();
		newAudioSource.spatialBlend = 0.65f;
		newAudioSource.maxDistance = 100;
		newAudioSource.volume = soundData.volumeMultiplier;
		if (_parent != null)
		{
			newAudioSource.transform.SetParent(_parent);
		}
		newAudioSource.gameObject.AddComponent<SoundAutoDestroyer>();
		newAudioSource.transform.position = _worldPosition;
		newAudioSource.clip = clip;
		newAudioSource.Play();
	}

	public static SoundData GetSoundData(string _soundName)
	{
		SoundDatas soundDatas = Resources.Load<SoundDatas>("SoundDatas");
		foreach (SoundData soundData in soundDatas.soundList)
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
		float pickChances = Random.value;
		int chosenIndex = 0;
		float cumulativeChances = _soundData.soundList[0].playChances;
		while (pickChances > cumulativeChances && chosenIndex < _soundData.soundList.Count)
		{
			chosenIndex++;
			cumulativeChances += _soundData.soundList[chosenIndex].playChances;
		}
		return _soundData.soundList[chosenIndex].clip;
	}
}
