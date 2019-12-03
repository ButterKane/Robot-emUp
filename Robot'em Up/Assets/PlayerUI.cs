using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{

	//Global settings
	public bool displayHealth;
	public bool displayDashes;

	//Health settings
	public float healthFadeInSpeed;
	public float healthFadeOutSpeed;
	public float healthShowDuration;
	public float healthGainStartScale;
	public float healthGainEndScale;
	public float healthLosseStartScale;
	public float healthLosseEndScale;
	public AnimationCurve healthAnimationCurve;

	//Dash settings
	public float dashFadeInSpeed;
	public float dashFadeOutSpeed;
	public float dashShowDuration;
	public Image dashBar;
	public Color dashBarColor;


	public void DisplayHealth(float _duration, float _fadeInSpeed, float _fadeOutSpeed, float _startScale, float _endScale, AnimationCurve _scaleCurve )
	{

	}

	public void DisplayDashes(float _duration, float _fadeInSpeed, float _fadeOutSpeed)
	{

	}
}
