using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShaker : MonoBehaviour
{
	public static void ShakeCamera ( float _intensity, float _duration, float _frequency )
	{
		ShakeEffect shaker = new GameObject().AddComponent<ShakeEffect>();
		shaker.gameObject.name = "CameraShaker";
		shaker.ShakeCamera(_duration, _intensity, _frequency);
	}
}

public class ShakeEffect : MonoBehaviour {

	protected Vector3 _initialPosition;
	protected Quaternion _initialRotation;

	protected CinemachineBasicMultiChannelPerlin _perlin;
	protected CinemachineVirtualCamera _virtualCamera;

	protected virtual void Awake ()
	{

		CinemachineVirtualCamera _virtualCam = Camera.main.gameObject.GetComponent<CinemachineBrain>().ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVirtualCamera>();
		_perlin = _virtualCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
		if (_perlin == null)
		{
			_perlin = _virtualCam.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
			_perlin.m_NoiseProfile = Resources.Load("NoiseProfile") as NoiseSettings;
		}
	}

	public virtual void ShakeCamera ( float duration, float amplitude, float frequency )
	{
		StartCoroutine(ShakeCameraCo(duration, amplitude, frequency));
	}

	protected virtual IEnumerator ShakeCameraCo ( float duration, float amplitude, float frequency )
	{
		_perlin.m_AmplitudeGain = amplitude;
		_perlin.m_FrequencyGain = frequency;
		yield return new WaitForSeconds(duration);
		CameraReset();
	}

	public virtual void CameraReset ()
	{
		_perlin.m_AmplitudeGain = 0;
		_perlin.m_FrequencyGain = 0;
	}
}
