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
		ShakeData internal_shakeData = new ShakeData(_intensity, _duration, _frequency);
		if (currentShake != null)
		{
			if (_intensity > currentShake.intensity)
			{
				shakeList.Add(currentShake);
				currentShake = internal_shakeData;
				shakeList.Add(internal_shakeData);
			} else
			{
				shakeList.Add(internal_shakeData);
			}
		} else
		{
			currentShake = internal_shakeData;
			shakeList.Add(internal_shakeData);
		}
	}

	public static void UpdateShakes()
	{
		List<ShakeData> internal_newShakeData = new List<ShakeData>();
		foreach (ShakeData shakeData in shakeList)
		{
			shakeData.durationLeft -= Time.deltaTime;
			if (shakeData.durationLeft > 0)
			{
				internal_newShakeData.Add(shakeData);
			}
		}
		shakeList = internal_newShakeData;
		if (currentShake != null && currentShake.durationLeft <= 0) { shakeList.Remove(currentShake); currentShake = GetNextShakeData(); }
	}

	public static ShakeData GetNextShakeData()
	{
		if (shakeList.Count <= 0)
		{
			return null;
		}
		ShakeData internal_biggestShake = shakeList[0];
		float internal_biggestValue = shakeList[0].intensity;
		foreach (ShakeData shakeData in shakeList)
		{
			if (shakeData.intensity > internal_biggestValue)
			{
				internal_biggestValue = shakeData.intensity;
				internal_biggestShake = shakeData;
			}
		}
		return internal_biggestShake;
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

	public CinemachineBasicMultiChannelPerlin _perlin;
	public CinemachineVirtualCamera _virtualCamera;

	public ShakeData currentShake;

	protected virtual void Awake ()
	{
		_virtualCamera = GetVirtualCamera();
	}

	public CinemachineVirtualCamera GetVirtualCamera()
	{
		CinemachineBrain internal_brain = Camera.main.gameObject.GetComponent<CinemachineBrain>();
		if (internal_brain == null) { return null; }

		ICinemachineCamera internal_virtualCameraEnum = internal_brain.ActiveVirtualCamera;
		if (internal_virtualCameraEnum == null) { return null; }

		GameObject internal_virtualCamGO = internal_brain.ActiveVirtualCamera.VirtualCameraGameObject;
		if (internal_virtualCamGO == null) { return null; }

		CinemachineVirtualCamera internal_virtualCam = internal_virtualCamGO.GetComponent<CinemachineVirtualCamera>();
		if (internal_virtualCam == null) { return null; }

		_perlin = internal_virtualCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
		if (_perlin == null)
		{
			_perlin = internal_virtualCam.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
			_perlin.m_NoiseProfile = Resources.Load("NoiseProfile") as NoiseSettings;
		}
		return internal_virtualCam;
	}

	private void Update ()
	{
		_virtualCamera = GetVirtualCamera();
		if (_virtualCamera == null) { return ; }
		currentShake = CameraShaker.currentShake;
		CameraShaker.UpdateShakes();
		if (currentShake != null)
		{
			float internal_momentumMultiplier = MomentumManager.GetValue(MomentumManager.datas.screenShakeMultiplier);
			_perlin.m_AmplitudeGain = currentShake.intensity * internal_momentumMultiplier;
			_perlin.m_FrequencyGain = currentShake.frequency;
		} else
		{
			_perlin.m_AmplitudeGain = 0;
			_perlin.m_FrequencyGain = 0;
		}
	}
}
