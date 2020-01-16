using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public enum VFXPosition { EventObject, EventPosition }
public enum VFXDirection { Default, EventDirection, EventNormal, LocalForward, WorldForward}
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


[System.Serializable]
public class FeedbackData : ScriptableObject
{
	public string eventName; //Each event will trigger specific feedbacks (Vibration or screenShake
	public ShakeData shakeData = null;
	public VibrationData vibrationData = null;
	public bool shakeDataInited;
	public bool vibrationDataInited;
	public bool soundDataInited;
	public bool vfxDataInited;
	public bool eventCalled = false;
	public FeedbackEventCategory category;
	public SoundPlayData soundData;
	public VFXData vfxData;
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
	public static FeedbackCallback SendFeedback (string _eventName, Object _target)
	{
		return SendFeedback(_eventName, _target, Vector3.zero, Vector3.forward, Vector3.forward);
	}
	public static FeedbackCallback SendFeedback (string _eventName, Object _target, Vector3 _eventPosition, Vector3 _eventDirection, Vector3 _eventNormal)
	{
		FeedbackCallback i_callBack = new FeedbackCallback();
		FeedbackData feedback = GetFeedbackData(_eventName);
		if (feedback == null) { return i_callBack; }
		if (feedback.shakeData != null && feedback.shakeDataInited) { CameraShaker.ShakeCamera(feedback.shakeData.intensity, feedback.shakeData.duration, feedback.shakeData.frequency, feedback.shakeData.intensityCurve); }
		if (feedback.vibrationData != null && feedback.vibrationDataInited)
		{
			switch (feedback.vibrationData.target)
			{
				case VibrationTarget.TargetedPlayer:
					if (_target == null) { Debug.LogWarning("Can't make target vibrate"); return i_callBack; }
					Component target = _target as Component;
					PlayerController player = target.GetComponent<PlayerController>();
					if (player != null)
					{
						VibrationManager.Vibrate(player.playerIndex,feedback.vibrationData.duration, feedback.vibrationData.force, feedback.vibrationData.forceCurve);
					}
					break;
				case VibrationTarget.BothPlayers:
					VibrationManager.Vibrate(PlayerIndex.One,feedback.vibrationData.duration, feedback.vibrationData.force, feedback.vibrationData.forceCurve);
					VibrationManager.Vibrate(PlayerIndex.Two, feedback.vibrationData.duration, feedback.vibrationData.force, feedback.vibrationData.forceCurve);
					break;

			}
		}
		if (feedback.soundData != null && feedback.soundDataInited)
		{
			Component target = _target as Component;
			Transform parent = null;
			if (feedback.soundData.attachToTarget)
			{
				parent = target.transform;
			}
			SoundManager.PlaySound(feedback.soundData.soundName, target.transform.position, parent);
		}
		if (feedback.vfxData != null && feedback.vfxDataInited && feedback.vfxData.vfxPrefab != null)
		{
			Component target = _target as Component;
			Vector3 direction = Vector3.zero;
			switch (feedback.vfxData.direction)
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
				case VFXDirection.WorldForward:
					direction = new Vector3(0, 1, 0);
					break;
			}
			Transform newParent = null;
			Vector3 position = _eventPosition;
			switch (feedback.vfxData.position)
			{
				case VFXPosition.EventObject:
					position = target.transform.position;
					break;
			}
			if (feedback.vfxData.attachToTarget)
			{
				newParent = target.transform;
			}
			i_callBack.vfx = FXManager.InstantiateFX(feedback.vfxData.vfxPrefab, position + feedback.vfxData.offset, false, direction, feedback.vfxData.scaleMultiplier, newParent);
		}
		return i_callBack;
	}

	public static FeedbackData GetFeedbackData(string _name)
	{
		FeedbackDatas feedbacksDatas = Resources.Load<FeedbackDatas>("FeedbackDatas");
		foreach (FeedbackData feedbackData in feedbacksDatas.feedbackList)
		{
			if (feedbackData.eventName == _name)
			{
				return feedbackData;
			}
		}
		Debug.LogWarning("No data for sound with name " + _name + " found.");
		return null;
	}
}
