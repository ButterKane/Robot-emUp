using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
