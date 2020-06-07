 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif

[System.Serializable]
public class SoundData
{
	public string soundName;
	public float delay = 0;
	public List<Sound> soundList = new List<Sound>();
	[Range(0f, 1f)] public float volumeMultiplier = 1f;

	public void SetPlayProbability(Sound _sound, float _newProbability)
	{
		if (soundList.Count <= 1) { return; }
		float i_difference = 0f;
		float i_otherTotal = 0;
		foreach (Sound wep in soundList)
		{
			if (wep == _sound)
			{
				i_difference = _newProbability - wep.playChances;
				wep.playChances = _newProbability;
			}
			else
			{
				i_otherTotal += wep.playChances;
			}
		}

		//Reduce or improve all the other probabilities (So it keep a maximum of 1)
		foreach (Sound wep in soundList)
		{
			if (wep != _sound)
			{
				if (i_difference > 0)
				{
					if (i_otherTotal != 0)
					{
						float force = wep.playChances / i_otherTotal;
						wep.playChances -= force * i_difference;
					}
				}
				else
				{
					if ((float)(soundList.Count - 1) - i_otherTotal != 0)
					{
						float i_force = (1f - wep.playChances) / ((float)(soundList.Count - 1) - i_otherTotal);
						wep.playChances -= i_force * i_difference;
					}
				}
			}
			wep.playChances = Mathf.Clamp(wep.playChances, 0f, 1f);
		}
	}
	public void RemoveSound(Sound _sound )
	{
		float i_difference = _sound.playChances;
		soundList.Remove(_sound);
		float i_totalProba = 0;
		foreach (Sound wep in soundList)
		{
			i_totalProba += wep.playChances;
		}

		//Reduce or improve all the other probabilities (So it keep a maximum of 1)
		foreach (Sound wep in soundList)
		{
			if (i_totalProba != 0)
			{
				float i_force = wep.playChances / i_totalProba;
				wep.playChances += i_force * i_difference;
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
#if UNITY_EDITOR
	public static EditorCoroutine soundCoroutine;
#endif
	public static void PlaySound(SoundData _soundData, Vector3 _worldPosition, Transform _parent = null)
	{
		if (_soundData == null) { return; }
		if (_soundData.soundList == null || _soundData.soundList.Count <= 0) { return; }
		AudioClip clip = GetSoundClip(_soundData);
		if (clip == null) { return; }
		GameObject i_newSoundPlayer = new GameObject();
		i_newSoundPlayer.name = "SoundPlayer";
		AudioSource i_newAudioSource = i_newSoundPlayer.AddComponent<AudioSource>();
		i_newAudioSource.spatialBlend = 0.65f;
		i_newAudioSource.maxDistance = 100;
		i_newAudioSource.volume = _soundData.volumeMultiplier;
		if (_parent != null)
		{
			i_newAudioSource.transform.SetParent(_parent);
		} 
		i_newAudioSource.gameObject.AddComponent<SoundAutoDestroyer>();
		i_newAudioSource.transform.position = _worldPosition;
		i_newAudioSource.clip = clip;
		if (_soundData.delay > 0)
		{
			i_newAudioSource.PlayDelayed(_soundData.delay);
		} else
		{
			i_newAudioSource.Play();
		}
	}

	public static AudioClip GetSoundClip(SoundData _soundData)
	{
		float i_pickChances = Random.value;
		int i_chosenIndex = 0;
		float i_cumulativeChances = _soundData.soundList[0].playChances;
		while (i_pickChances > i_cumulativeChances && i_chosenIndex < _soundData.soundList.Count)
		{
			i_chosenIndex++;
			i_cumulativeChances += _soundData.soundList[i_chosenIndex].playChances;
		}
		if (_soundData.soundList[i_chosenIndex].clip == null)
		{
			Debug.LogWarning("Warning: Clip '" + _soundData.soundName + "' not found."); return null;
		}
		return _soundData.soundList[i_chosenIndex].clip;
	}

	public static void PlaySoundInEditor ( SoundData _soundData )
	{
#if UNITY_EDITOR
		if (_soundData.delay > 0)
		{
			if (soundCoroutine != null) { EditorCoroutineUtility.StopCoroutine(soundCoroutine); }
			soundCoroutine = EditorCoroutineUtility.StartCoroutine(PlaySoundInEditor_C(GetSoundClip(_soundData), 0, false, _soundData.delay), _soundData);
		}
		else
		{
			PlaySoundInEditor(GetSoundClip(_soundData));
		}
#endif
	}

	public static IEnumerator PlaySoundInEditor_C( AudioClip _clip, int _startSample = 0, bool _loop = false, float _delay = 0)
	{
		float waitTimer = 0;
		while (waitTimer < _delay)
		{
			yield return new WaitForFixedUpdate();
			waitTimer += Time.deltaTime;
		}
		PlaySoundInEditor(_clip, _startSample, _loop);
	}
	public static void PlaySoundInEditor ( AudioClip _clip, int _startSample = 0, bool _loop = false )
	{
#if UNITY_EDITOR
		StopAllClips();
		System.Reflection.Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
		System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
		System.Reflection.MethodInfo method = audioUtilClass.GetMethod(
			"PlayClip",
			System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public,
			null,
			new System.Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
			null
		);
		method.Invoke(
			null,
			new object[] { _clip, _startSample, _loop }
		);
#endif
	}
	public static void StopAllClips ()
	{
#if UNITY_EDITOR
		System.Reflection.Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
		System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
		System.Reflection.MethodInfo method = audioUtilClass.GetMethod(
			"StopAllClips",
			System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public,
			null,
			new System.Type[] { },
			null
		);
		method.Invoke(
			null,
			new object[] { }
		);
#endif
	}
}
