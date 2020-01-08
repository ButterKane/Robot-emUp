﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;


public enum VibrationTarget { TargetedPlayer, BothPlayers}
[System.Serializable]
public class VibrationData
{
	public VibrationForce force;
	public float duration;
	public VibrationTarget target;
}

[System.Serializable]
public class SoundPlayData
{
	public string soundName;
	public bool attachToTarget;
}

[System.Serializable]
public class FeedbackData
{
	public string eventName; //Each event will trigger specific feedbacks (Vibration or screenShake
	public ShakeData shakeData = null;
	public VibrationData vibrationData = null;
	public bool shakeDataInited;
	public bool vibrationDataInited;
	public bool eventCalled = false;
	public FeedbackEventCategory category;
	public SoundPlayData soundData;
	public bool soundDataInited;
}

public class FeedbackManager
{
	public static void SendFeedback(string _eventName, Object _target)
	{
		FeedbackData feedback = GetFeedbackData(_eventName);
		if (feedback.shakeData != null && feedback.shakeDataInited) { CameraShaker.ShakeCamera(feedback.shakeData.intensity, feedback.shakeData.duration, feedback.shakeData.frequency); }
		if (feedback.vibrationData != null && feedback.vibrationDataInited)
		{
			switch (feedback.vibrationData.target)
			{
				case VibrationTarget.TargetedPlayer:
					if (_target == null) { Debug.LogWarning("Can't make target vibrate"); return; }
					Component target = _target as Component;
					PlayerController player = target.GetComponent<PlayerController>();
					if (player != null)
					{
						VibrationManager.Vibrate(player.playerIndex,feedback.vibrationData.duration, feedback.vibrationData.force);
					}
					break;
				case VibrationTarget.BothPlayers:
					VibrationManager.Vibrate(PlayerIndex.One,feedback.vibrationData.duration, feedback.vibrationData.force);
					VibrationManager.Vibrate(PlayerIndex.Two, feedback.vibrationData.duration, feedback.vibrationData.force);
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
