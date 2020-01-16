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
				var i_newStartColor = main.startColor;
				if (IsEqualTo(i_newStartColor.color, _currentColor))
				{
					i_newStartColor.color = _newColor;
					main.startColor = i_newStartColor;
				}
				break;

			case ParticleSystemGradientMode.Gradient:
				var i_sgradient = main.startColor.gradient;
				GradientColorKey[] scolorKeys = new GradientColorKey[i_sgradient.colorKeys.Length];
				GradientAlphaKey[] salphaKeys = new GradientAlphaKey[i_sgradient.alphaKeys.Length];
				for (int i = 0; i < i_sgradient.colorKeys.Length; i++)
				{
					GradientColorKey newKey = i_sgradient.colorKeys[i];
					//if (IsEqualTo(sgradient.colorKeys[i].color,_currentColor))
					//{
						newKey.color = _newColor;
					//}
					scolorKeys[i] = newKey;
				}
				for (int i = 0; i < i_sgradient.alphaKeys.Length; i++)
				{
					GradientAlphaKey newKey = i_sgradient.alphaKeys[i];
					salphaKeys[i] = newKey;
				}
				Gradient i_newSGradient = new Gradient();
				i_newSGradient.SetKeys(scolorKeys, salphaKeys);
				main.startColor = i_newSGradient;
				break;
		}

		//Replace color over lifetime
		ParticleSystem.ColorOverLifetimeModule i_colorOverLifetime = _ps.colorOverLifetime;
		var i_colgradient = i_colorOverLifetime.color.gradient;
		GradientColorKey[] i_colorKeys = new GradientColorKey[i_colgradient.colorKeys.Length];
		GradientAlphaKey[] i_alphaKeys = new GradientAlphaKey[i_colgradient.alphaKeys.Length];
		for (int i = 0; i < i_colgradient.colorKeys.Length; i++)
		{
			GradientColorKey i_newKey = i_colgradient.colorKeys[i];
			//if (IsEqualTo(colgradient.colorKeys[i].color, _currentColor))
			//{
				i_newKey.color = _newColor;
			//}
			i_colorKeys[i] = i_newKey;
		}
		for (int i = 0; i < i_colgradient.alphaKeys.Length; i++)
		{
			GradientAlphaKey i_newKey = i_colgradient.alphaKeys[i];
			i_alphaKeys[i] = i_newKey;
		}
		Gradient newGradient = new Gradient();
		newGradient.SetKeys(i_colorKeys, i_alphaKeys);
		i_colorOverLifetime.color = newGradient;
	}


	public static bool IsEqualTo(Color _firstColor, Color _secondColor)
	{
		return _firstColor.r == _secondColor.r && _firstColor.g == _secondColor.g && _firstColor.b == _secondColor.b;
	}
}
