using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[ExecuteAlways]
public class CameraShaker : MonoBehaviour
{
	public static List<ShakeData> shakeList = new List<ShakeData>();
	public static ShakeData currentShake;
	public static ShakeEffect cameraShaker;
    //public static float shakeSettingsMod = 1;
	public static void ShakeEditorCamera (Camera _camera, float _intensity, float _duration, float _frequency, AnimationCurve _intensityCurve)
	{
		EditorCameraShaker shaker = _camera.gameObject.AddComponent<EditorCameraShaker>();
		shaker.Init(_duration, _intensity, _frequency, _intensityCurve);
	}

	public static void ShakeCamera ( float _intensity, float _duration, float _frequency, AnimationCurve _intensityCurve )
	{
		if (_intensity == 0 || _duration == 0) { return; }
		if (cameraShaker == null)
		{
			cameraShaker = new GameObject().AddComponent<ShakeEffect>();
			cameraShaker.gameObject.name = "Camera Shaker";
		}
		ShakeData i_shakeData = new ShakeData(_intensity * (PlayerPrefs.GetFloat("REU_Screenshake_intensity", 100)/100), _duration, _frequency);
		i_shakeData.intensityCurve = _intensityCurve;
		if (currentShake != null)
		{
			if (_intensity > currentShake.intensity)
			{
				shakeList.Add(currentShake);
				currentShake = i_shakeData;
				shakeList.Add(i_shakeData);
			} else
			{
				shakeList.Add(i_shakeData);
			}
		} else
		{
			currentShake = i_shakeData;
			shakeList.Add(i_shakeData);
		}
	}

	public static void UpdateShakes()
	{
		List<ShakeData> i_newShakeData = new List<ShakeData>();
		foreach (ShakeData shakeData in shakeList)
		{
			shakeData.durationLeft -= Time.deltaTime;
			if (shakeData.durationLeft > 0)
			{
				i_newShakeData.Add(shakeData);
			}
		}
		shakeList = i_newShakeData;
		if (currentShake != null && currentShake.durationLeft <= 0) { shakeList.Remove(currentShake); currentShake = GetNextShakeData(); }
	}

	public static ShakeData GetNextShakeData()
	{
		if (shakeList.Count <= 0)
		{
			return null;
		}
		ShakeData i_biggestShake = shakeList[0];
		float i_biggestValue = shakeList[0].intensity;
		foreach (ShakeData shakeData in shakeList)
		{
			if (shakeData.intensity > i_biggestValue)
			{
				i_biggestValue = shakeData.intensity;
				i_biggestShake = shakeData;
			}
		}
		return i_biggestShake;
	}
}

[System.Serializable]
public class ShakeData
{
	public ShakeData ( float _intensity, float _duration, float _frequency)
	{
		duration = _duration;
		intensity = _intensity;
		frequency = _frequency;
		durationLeft = _duration;
		intensityCurve = new AnimationCurve();
		intensityCurve.AddKey(new Keyframe(0, 1));
		intensityCurve.AddKey(new Keyframe(1, 1));
	}
	public float duration;
	public float intensity;
	public float frequency;
	public AnimationCurve intensityCurve;

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
		if (Camera.main.gameObject == null) { return null; }
		CinemachineBrain i_brain = Camera.main.gameObject.GetComponent<CinemachineBrain>();
		if (i_brain == null) { return null; }

		ICinemachineCamera i_virtualCameraEnum = i_brain.ActiveVirtualCamera;
		if (i_virtualCameraEnum == null) { return null; }

		GameObject i_virtualCamGO = i_brain.ActiveVirtualCamera.VirtualCameraGameObject;
		if (i_virtualCamGO == null) { return null; }

		CinemachineVirtualCamera i_virtualCam = i_virtualCamGO.GetComponent<CinemachineVirtualCamera>();
		if (i_virtualCam == null) { return null; }

		_perlin = i_virtualCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
		if (_perlin == null)
		{
			_perlin = i_virtualCam.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
			_perlin.m_NoiseProfile = Resources.Load("NoiseProfile") as NoiseSettings;
		}
		return i_virtualCam;
	}

	private void Update ()
	{
		_virtualCamera = GetVirtualCamera();
		if (_virtualCamera == null) { return ; }
		currentShake = CameraShaker.currentShake;
		CameraShaker.UpdateShakes();
		if (currentShake != null)
		{
			float i_momentumMultiplier = MomentumManager.GetValue(MomentumManager.datas.screenShakeMultiplier);
			float i_curveMultiplier = currentShake.intensityCurve.Evaluate(1f - (currentShake.durationLeft / currentShake.duration));
			_perlin.m_AmplitudeGain = currentShake.intensity * i_momentumMultiplier * i_curveMultiplier;
			_perlin.m_FrequencyGain = currentShake.frequency * i_curveMultiplier;
		} else
		{
			_perlin.m_AmplitudeGain = 0;
			_perlin.m_FrequencyGain = 0;
		}
	}
}

[ExecuteInEditMode]
public class EditorCameraShaker : MonoBehaviour
{
	public void Init (float _duration, float _intensity, float _frequency, AnimationCurve _intensityCurve)
	{
		StartCoroutine(ShakeEditorCamera_C(_duration, _intensity, _frequency, _intensityCurve));
	}

	IEnumerator ShakeEditorCamera_C(float _duration, float _intensity, float _frequency, AnimationCurve _intensityCurve)
	{
		Vector3 initialPosition = transform.position;
		for (float i = 0; i < _duration; i+= Time.deltaTime)
		{
			transform.position = initialPosition+ Random.insideUnitSphere * _intensity * 0.1f * _intensityCurve.Evaluate(i/_duration);
			yield return null;
		}
		transform.position = initialPosition;
		DestroyImmediate(this);
		yield return null;
	}
}
