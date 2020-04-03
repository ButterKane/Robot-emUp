using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public enum VFXPosition { EventObject, EventPosition }
public enum VFXDirection { Default, EventDirection, EventNormal, LocalForward, WorldUp}
public enum VibrationTarget { TargetedPlayer, BothPlayers}
[System.Serializable]
public class VibrationData
{
	public VibrationForce force;
	public float duration;
	public VibrationTarget target;
	public AnimationCurve forceCurve;
}

[System.Serializable]
public class SoundPlayData
{
	public string soundName = "";
	public bool attachToTarget;
}


[System.Serializable]
public class VFXData
{
	public GameObject vfxPrefab = default;
	public Vector3 offset;
	public Vector3 scaleMultiplier = new Vector3(1, 1, 1); 
	public VFXDirection direction;
	public VFXPosition position;
	public bool attachToTarget;
}


public class FeedbackCallback
{
	public GameObject vfx;
	public GameObject GetVFX()
	{
		return vfx;
	}
}

public class FeedbackManager
{
	public static FeedbackDatas feedbacksDatas;
	public static FeedbackCallback SendFeedback (string _eventName, Object _target)
	{
		return SendFeedback(_eventName, _target, Vector3.zero, Vector3.forward, Vector3.forward);
	}
	public static FeedbackCallback SendFeedback (string _eventName, Object _target, Vector3 _eventPosition, Vector3 _eventDirection, Vector3 _eventNormal)
	{
		//Data initianilisation
		FeedbackCallback i_callBack = new FeedbackCallback();
		FeedbackData i_feedback = GetFeedbackData(_eventName);
		if (i_feedback == null) { return i_callBack; }
		Component target = _target as Component;

		//Shake camera
		if (i_feedback.shakeData != null && i_feedback.shakeDataInited) { 
			CameraShaker.ShakeCamera(i_feedback.shakeData.intensity, i_feedback.shakeData.duration, i_feedback.shakeData.frequency, i_feedback.shakeData.intensityCurve); 
		}

		//Vibrate gamepad
		if (i_feedback.vibrationData != null && i_feedback.vibrationDataInited)
		{
			switch (i_feedback.vibrationData.target)
			{
				case VibrationTarget.TargetedPlayer:
					if (_target == null) { Debug.LogWarning("Can't make target vibrate"); break; }
					PlayerController player = (PlayerController)target;
					if (player != null)
					{
						VibrationManager.Vibrate(player.playerIndex,i_feedback.vibrationData.duration, i_feedback.vibrationData.force, i_feedback.vibrationData.forceCurve);
					}
					break;
				case VibrationTarget.BothPlayers:
					VibrationManager.Vibrate(PlayerIndex.One,i_feedback.vibrationData.duration, i_feedback.vibrationData.force, i_feedback.vibrationData.forceCurve);
					VibrationManager.Vibrate(PlayerIndex.Two, i_feedback.vibrationData.duration, i_feedback.vibrationData.force, i_feedback.vibrationData.forceCurve);
					break;

			}
		}

		//Play sound
		if (i_feedback.soundData != null && i_feedback.soundDataInited && i_feedback.soundData.soundName != "")
		{
			Transform parent = null;
			if (i_feedback.soundData.attachToTarget)
			{
				parent = target.transform;
			}
			SoundManager.PlaySound(i_feedback.soundData.soundName, target.transform.position, parent);
		}

		//Generate FX
		if (i_feedback.vfxData != null && i_feedback.vfxDataInited && i_feedback.vfxData.vfxPrefab != null)
		{
			Vector3 direction = Vector3.zero;
			switch (i_feedback.vfxData.direction)
			{
				case VFXDirection.Default:
					direction = new Vector3(0, 0, 0);
					break;
				case VFXDirection.EventDirection:
					direction = _eventDirection;
					break;
				case VFXDirection.EventNormal:
					direction = _eventNormal;
					break;
				case VFXDirection.LocalForward:
					direction = target.transform.forward;
					break;
				case VFXDirection.WorldUp:
					direction = new Vector3(0, 1, 0);
					break;
			}
			Transform newParent = null;
			Vector3 position = _eventPosition;
			switch (i_feedback.vfxData.position)
			{
				case VFXPosition.EventObject:
					position = target.transform.position;
					break;
			}
			if (i_feedback.vfxData.attachToTarget)
			{
				newParent = target.transform;
			}
			i_callBack.vfx = FXManager.InstantiateFX(i_feedback.vfxData.vfxPrefab, position + i_feedback.vfxData.offset, false, direction, i_feedback.vfxData.scaleMultiplier, newParent);
		}
		return i_callBack; 
	}

	public static FeedbackData GetFeedbackData(string _name)
	{
		if (feedbacksDatas == null) { feedbacksDatas = Resources.Load<FeedbackDatas>("FeedbackDatas"); }
		foreach (FeedbackData feedbackData in feedbacksDatas.feedbackList)
		{
			if (feedbackData.eventName == _name)
			{
				return feedbackData;
			}
		}
		Debug.LogWarning("No data for event with name " + _name + " found.");
		return null;
	}
}
