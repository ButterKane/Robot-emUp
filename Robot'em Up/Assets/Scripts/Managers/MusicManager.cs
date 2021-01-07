using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
	public static MusicManager instance;
	public float minDelayBeforeChangingMusic = 1f;
	private List<MusicInformation> currentMusicInformations = new List<MusicInformation>();
	private float volumeMultiplier;
	private float changeClipDelayCooldown;

	private class MusicInformation
	{
		public AudioSource linkedSource;
		public MusicData linkedMusicData;
	}
	private void Awake ()
	{
		instance = this;
		volumeMultiplier = 1;
	}

	private void Update ()
	{
		if (changeClipDelayCooldown > 0)
		{
			changeClipDelayCooldown -= Time.deltaTime;
		}
	}
	public static void PlayMusic(string _music)
	{
		if (instance.changeClipDelayCooldown > 0) { return; }
		MusicData data = MusicDatas.GetMusicData(_music);
		bool musicAlreadyPlaying = false;
		foreach (MusicInformation s in instance.currentMusicInformations)
		{
			if (s.linkedMusicData != data)
			{
				instance.StartCoroutine(instance.FadeOutSound_C(s.linkedSource, s.linkedMusicData.fadeOutDuration));
			} else if (s.linkedSource.isPlaying)
			{
				musicAlreadyPlaying = true;
			}
		}
		if (musicAlreadyPlaying == false)
		{
			instance.currentMusicInformations.Clear();
			instance.StartCoroutine(instance.FadeInSound_C(data));
			instance.changeClipDelayCooldown = instance.minDelayBeforeChangingMusic;
		}
	}
	public static void ChangeVolume(float _newVolume)
	{
		instance.volumeMultiplier = _newVolume * PlayerPrefs.GetFloat("music.volume", 1f);
		foreach (MusicInformation mi in instance.currentMusicInformations)
		{
			mi.linkedSource.volume = instance.volumeMultiplier;
		}
	}
	public static void ResetVolume()
	{
		instance.volumeMultiplier = PlayerPrefs.GetFloat("music.volume", 1f);
		foreach (MusicInformation mi in instance.currentMusicInformations)
		{
			mi.linkedSource.volume = instance.volumeMultiplier;
		}
	}
	public static void StopMusic(float _fadeOutDuration = 0)
	{
		foreach (MusicInformation s in instance.currentMusicInformations)
		{
			instance.StartCoroutine(instance.StopMusic_C(s.linkedSource, _fadeOutDuration));
		}
	}
	public static void ResumeMusic(float _fadeInDuration = 0)
	{
		foreach (MusicInformation s in instance.currentMusicInformations)
		{
			s.linkedSource.Play();
		}
	}
	public static void ToggleLoop(bool _status)
	{
		foreach (MusicInformation s in instance.currentMusicInformations)
		{
			s.linkedSource.loop = _status;
		}
	}


	IEnumerator FadeInSound_C(MusicData _musicData)
	{
		float duration = _musicData.fadeInDuration;
		AudioSource newSource = gameObject.AddComponent<AudioSource>();
		newSource.clip = _musicData.clip ;
		newSource.Play();
		newSource.loop = _musicData.isLooping;
		for (float i = 0; i < duration; i+=Time.deltaTime)
		{
			newSource.volume = Mathf.Lerp(0f, volumeMultiplier, i / duration);
			yield return null;
		}
		MusicInformation newMusicInf = new MusicInformation();
		newMusicInf.linkedSource = newSource;
		newMusicInf.linkedMusicData = _musicData;
		currentMusicInformations.Add(newMusicInf);
	}

	IEnumerator StopMusic_C(AudioSource _source, float _duration)
	{
		for (float i = 0; i < _duration; i += Time.deltaTime)
		{
			_source.volume = Mathf.Lerp(volumeMultiplier, 0, i / _duration);
			yield return null;
		}
		_source.Stop();
	}
	IEnumerator FadeOutSound_C(AudioSource _source, float _duration)
	{
		yield return StartCoroutine(StopMusic_C(_source, _duration));
		if (_source != null)
		{
			Destroy(_source);
		}
	}
}
