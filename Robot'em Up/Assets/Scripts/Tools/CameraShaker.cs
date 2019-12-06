using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShaker : MonoBehaviour
{
	public static List<ShakeData> shakeList = new List<ShakeData>();
	public static ShakeData currentShake;
	public static ShakeEffect cameraShaker;

	public static void ShakeCamera ( float _intensity, float _duration, float _frequency )
	{
		if (cameraShaker == null)
		{
			cameraShaker = new GameObject().AddComponent<ShakeEffect>();
			cameraShaker.gameObject.name = "Camera Shaker";
		}
		ShakeData shakeData = new ShakeData(_intensity, _duration, _frequency);
		if (currentShake != null)
		{
			if (_intensity > currentShake.intensity)
			{
				shakeList.Add(currentShake);
				currentShake = shakeData;
				shakeList.Add(shakeData);
			} else
			{
				shakeList.Add(shakeData);
			}
		} else
		{
			currentShake = shakeData;
			shakeList.Add(shakeData);
		}
	}

	public static void UpdateShakes()
	{
		List<ShakeData> newShakeData = new List<ShakeData>();
		foreach (ShakeData shakeData in shakeList)
		{
			shakeData.durationLeft -= Time.deltaTime;
			if (shakeData.durationLeft > 0)
			{
				newShakeData.Add(shakeData);
			}
		}
		shakeList = newShakeData;
		if (currentShake != null && currentShake.durationLeft <= 0) { shakeList.Remove(currentShake); currentShake = GetNextShakeData(); }
	}

	public static ShakeData GetNextShakeData()
	{
		if (shakeList.Count <= 0)
		{
			return null;
		}
		ShakeData biggestShake = shakeList[0];
		float biggestValue = shakeList[0].intensity;
		foreach (ShakeData shakeData in shakeList)
		{
			if (shakeData.intensity > biggestValue)
			{
				biggestValue = shakeData.intensity;
				biggestShake = shakeData;
			}
		}
		return biggestShake;
	}
}

[System.Serializable]
public class ShakeData
{
	public ShakeData (float _intensity, float _duration, float _frequency)
	{
		duration = _duration;
		intensity = _intensity;
		frequency = _frequency;
		durationLeft = _duration;
	}
	public float duration;
	public float intensity;
	public float frequency;

	public float durationLeft;
}

public class ShakeEffect : MonoBehaviour {

	protected CinemachineBasicMultiChannelPerlin _perlin;
	protected CinemachineVirtualCamera _virtualCamera;

	public ShakeData currentShake;

	protected virtual void Awake ()
	{
		CinemachineBrain brain = Camera.main.gameObject.GetComponent<CinemachineBrain>();
		if (brain == null) { return; }
		ICinemachineCamera virtualCameraEnum = brain.ActiveVirtualCamera;
		if (virtualCameraEnum == null) { return; }
		GameObject virtualCamGO = brain.ActiveVirtualCamera.VirtualCameraGameObject;
		if (virtualCamGO == null) { return; }
		CinemachineVirtualCamera _virtualCam = virtualCamGO.GetComponent<CinemachineVirtualCamera>();
		_perlin = _virtualCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
		if (_perlin == null)
		{
			_perlin = _virtualCam.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
			_perlin.m_NoiseProfile = Resources.Load("NoiseProfile") as NoiseSettings;
		}
	}

	private void Update ()
	{
		if (_virtualCamera == null) { return; }
		currentShake = CameraShaker.currentShake;
		CameraShaker.UpdateShakes();
		if (currentShake != null)
		{
			_perlin.m_AmplitudeGain = currentShake.intensity;
			_perlin.m_FrequencyGain = currentShake.frequency;
		} else
		{
			_perlin.m_AmplitudeGain = 0;
			_perlin.m_FrequencyGain = 0;
		}
	}
}
