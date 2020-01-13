using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public enum VibrationForce
{
	VeryHeavy,
	Heavy,
	Medium,
	Light,
	VeryLight,
}

[ExecuteAlways]
public class VibrationManager : MonoBehaviour
{
	public static void Vibrate ( PlayerIndex _playerIndex, float _duration, VibrationForce _force )
	{
		Vibrator vibrator = new GameObject().AddComponent<Vibrator>();
		vibrator.StartCoroutine(vibrator.Vibrate_C(_playerIndex, _duration, _force));
	}

}

public class Vibrator : MonoBehaviour
{
	public IEnumerator Vibrate_C ( PlayerIndex _playerIndex, float _duration, VibrationForce _force )
	{
		float i_momentumMultiplier;
		#if !UNITY_EDITOR
						i_momentumMultiplier = MomentumManager.GetValue(MomentumManager.datas.vibrationMultiplier);
		#endif
		#if UNITY_EDITOR
				i_momentumMultiplier = 1f;
		#endif
		for (float i = 0; i < _duration; i += Time.deltaTime)
		{
			switch (_force)
			{
				case VibrationForce.VeryLight:
					GamePad.SetVibration(_playerIndex, 0.1f * i_momentumMultiplier, 0.1f * i_momentumMultiplier);
					break;
				case VibrationForce.Light:
					GamePad.SetVibration(_playerIndex, 0.2f * i_momentumMultiplier, 0.2f * i_momentumMultiplier);
					break;
				case VibrationForce.Medium:
					GamePad.SetVibration(_playerIndex, 0.3f * i_momentumMultiplier, 0.3f * i_momentumMultiplier);
					break;
				case VibrationForce.Heavy:
					GamePad.SetVibration(_playerIndex, 0.4f * i_momentumMultiplier, 0.4f * i_momentumMultiplier);
					break;
				case VibrationForce.VeryHeavy:
					GamePad.SetVibration(_playerIndex, 0.5f * i_momentumMultiplier, 0.5f * i_momentumMultiplier);
					break;
			}
			yield return new WaitForEndOfFrame();
		}
		GamePad.SetVibration(_playerIndex, 0f, 0f);
		DestroyImmediate(this.gameObject);
	}
}

