using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleColorer : MonoBehaviour
{
	public static Color ReplaceParticleColor(GameObject _FXGameObject, Color _currentColor, Color _newColor)
	{
		foreach (ParticleSystem ps in _FXGameObject.GetComponentsInChildren<ParticleSystem>())
		{
			ChangeParticleSystemColor(ps, _currentColor, _newColor);
		}
		return _newColor;
	}

	public static void ChangeParticleSystemColor(ParticleSystem _ps, Color _currentColor, Color _newColor)
	{
		//Replace startColor
		ParticleSystem.MainModule main = _ps.main;
		switch (main.startColor.mode)
		{
			case ParticleSystemGradientMode.Color:
				var newStartColor = main.startColor;
				if (IsEqualTo(newStartColor.color, _currentColor))
				{
					newStartColor.color = _newColor;
					main.startColor = newStartColor;
				}
				break;
			case ParticleSystemGradientMode.Gradient:
				var sgradient = main.startColor.gradient;
				GradientColorKey[] scolorKeys = new GradientColorKey[sgradient.colorKeys.Length];
				GradientAlphaKey[] salphaKeys = new GradientAlphaKey[sgradient.alphaKeys.Length];
				for (int i = 0; i < sgradient.colorKeys.Length; i++)
				{
					GradientColorKey newKey = sgradient.colorKeys[i];
					//if (IsEqualTo(sgradient.colorKeys[i].color,_currentColor))
					//{
						newKey.color = _newColor;
					//}
					scolorKeys[i] = newKey;
				}
				for (int i = 0; i < sgradient.alphaKeys.Length; i++)
				{
					GradientAlphaKey newKey = sgradient.alphaKeys[i];
					salphaKeys[i] = newKey;
				}
				Gradient newSGradient = new Gradient();
				newSGradient.SetKeys(scolorKeys, salphaKeys);
				main.startColor = newSGradient;
				break;
		}

		//Replace color over lifetime
		ParticleSystem.ColorOverLifetimeModule colorOverLifetime = _ps.colorOverLifetime;
		var colgradient = colorOverLifetime.color.gradient;
		GradientColorKey[] colorKeys = new GradientColorKey[colgradient.colorKeys.Length];
		GradientAlphaKey[] alphaKeys = new GradientAlphaKey[colgradient.alphaKeys.Length];
		for (int i = 0; i < colgradient.colorKeys.Length; i++)
		{
			GradientColorKey newKey = colgradient.colorKeys[i];
			//if (IsEqualTo(colgradient.colorKeys[i].color, _currentColor))
			//{
				newKey.color = _newColor;
			//}
			colorKeys[i] = newKey;
		}
		for (int i = 0; i < colgradient.alphaKeys.Length; i++)
		{
			GradientAlphaKey newKey = colgradient.alphaKeys[i];
			alphaKeys[i] = newKey;
		}
		Gradient newGradient = new Gradient();
		newGradient.SetKeys(colorKeys, alphaKeys);
		colorOverLifetime.color = newGradient;
	}


	public static bool IsEqualTo(Color _firstColor, Color _secondColor)
	{
		return _firstColor.r == _secondColor.r && _firstColor.g == _secondColor.g && _firstColor.b == _secondColor.b;
	}
}
