using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class VibrationData
{
	public VibrationForce force;
	public float duration;
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
}

public class FeedbackManager
{
	public static void SendFeedback(string _eventName, Object _target)
	{
		FeedbackData feedback = GetFeedbackData(_eventName);
		if (feedback.shakeData != null && feedback.shakeDataInited) { CameraShaker.ShakeCamera(feedback.shakeData.intensity, feedback.shakeData.duration, feedback.shakeData.frequency); }
		if (feedback.vibrationData != null && feedback.vibrationDataInited && _target != null)
		{
			Component target = _target as Component;
			PlayerController player = target.GetComponent<PlayerController>();
			if (player != null)
			{
				player.Vibrate(feedback.vibrationData.duration, feedback.vibrationData.force);
			}
		}
	}

	public static FeedbackData GetFeedbackData(string _name)
	{
		FeedbacksDatas feedbacksDatas = Resources.Load<FeedbacksDatas>("FeedbacksDatas");
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
