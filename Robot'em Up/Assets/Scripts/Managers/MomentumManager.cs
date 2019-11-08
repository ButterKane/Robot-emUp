using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

[CreateAssetMenu(fileName = "MomentumData", menuName = "GlobalDatas/Momentum", order = 1)]
public class MomentumData: ScriptableObject
{
	[Separator("Momentum global settings")]
	public float momentumLerpSpeed;

	[MinMaxRange(0f, 3f)] public RangedFloat playerSpeedMultiplier;
	[MinMaxRange(0f, 3f)] public RangedFloat dashRecoverSpeedMultiplier;
	[MinMaxRange(0f, 3f)] public RangedFloat ballSpeedMultiplier;
	[MinMaxRange(0f, 3f)] public RangedFloat energyGainMultiplier;
	[MinMaxRange(0f, 3f)] public RangedFloat ballDamageMultiplier;
	[MinMaxRange(0f, 3f)] public RangedFloat enemySpeedMultiplier;
	[MinMaxRange(0f, 3f)] public RangedFloat enemySpawnRateMultiplier;

	[Separator("Momentum gain settings")]
	[Range(0f, 1f)] public float momentumGainedOnPass;
	[Range(0f, 1f)] public float momentumGainedOnHit;
	[Range(0f, 1f)] public float momentumGainedOnDunk;
	[Range(0f, 1f)] public float momentumGainedOnPerfectReception;

	[Separator("Momentum loss settings")]
	public float minPassDelayBeforeMomentumLoss;
	public float momentumLossSpeedIfNoPass;
	[Range(0f, 1f)] public float momentumLossOnFightEnd;
	[Range(0f, 1f)] public float momentumLossOnDamage;
	[Range(0f, 1f)] public float momentumLossWhenBallHitTheGround;
}
public class MomentumManager: MonoBehaviour
{
	[Header("Settings")]
	public static MomentumData datas;
	private static float currentMomentum;
	private static float wantedMomentum;
	public static MomentumManager instance;

	private bool exponentialLossEnabled = false;
	private float exponentialLossDuration = 0;
	private float exponentialLossCoef = 1;

	public void Awake ()
	{
		instance = this;
		currentMomentum = 0;
		wantedMomentum = currentMomentum;
	}
	private void Update ()
	{
		currentMomentum = Mathf.Lerp(currentMomentum, wantedMomentum, Time.deltaTime * datas.momentumLerpSpeed);
		if (instance.exponentialLossEnabled)
		{
			exponentialLossDuration += Time.deltaTime * exponentialLossCoef;
			wantedMomentum -= Time.deltaTime * exponentialLossDuration;
			wantedMomentum = ClampMomentum(wantedMomentum);
		}
	}

	public static float GetMomentum()
	{
		return currentMomentum;
	}

	public static void EnableMomentumExponentialLoss(float _delayBeforeStarting, float _multiplyCoef)
	{
		instance.exponentialLossCoef = _multiplyCoef;
		instance.StartCoroutine(instance.EnableMomentumContinuousLossAfterDelay(_delayBeforeStarting));
	}
	public static void DisableMomentumExpontentialLoss ()
	{
		instance.StopAllCoroutines();
		instance.exponentialLossEnabled = false;
		instance.exponentialLossDuration = 0;
	}

	public static float IncreaseMomentum(float _amount)
	{
		wantedMomentum += _amount;
		wantedMomentum = ClampMomentum(wantedMomentum);
		return wantedMomentum;
	}

	public static float MultiplyMomentum(float _coef)
	{
		wantedMomentum *= _coef;
		wantedMomentum = ClampMomentum(wantedMomentum);
		return wantedMomentum;
	}

	public static bool DecreaseMomentum ( float _amount )
	{
		if (wantedMomentum < _amount)
		{
			wantedMomentum = 0;
			return false;
		} else
		{
			wantedMomentum -= _amount;
		}
		wantedMomentum = ClampMomentum(wantedMomentum);
		return true;
	}

	private static float ClampMomentum(float _momentum)
	{
		return Mathf.Clamp(_momentum, 0f, 1f);
	}

	public static float GetValue( RangedFloat _value )
	{
		return Mathf.Lerp(_value.Min, _value.Max, currentMomentum);
	}

	IEnumerator EnableMomentumContinuousLossAfterDelay(float _delay)
	{
		yield return new WaitForSeconds(_delay);
		instance.exponentialLossEnabled = true;
	}
}
