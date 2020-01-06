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
				var internal_newStartColor = main.startColor;
				if (IsEqualTo(internal_newStartColor.color, _currentColor))
				{
					internal_newStartColor.color = _newColor;
					main.startColor = internal_newStartColor;
				}
				break;

			case ParticleSystemGradientMode.Gradient:
				var internal_sgradient = main.startColor.gradient;
				GradientColorKey[] scolorKeys = new GradientColorKey[internal_sgradient.colorKeys.Length];
				GradientAlphaKey[] salphaKeys = new GradientAlphaKey[internal_sgradient.alphaKeys.Length];
				for (int i = 0; i < internal_sgradient.colorKeys.Length; i++)
				{
					GradientColorKey newKey = internal_sgradient.colorKeys[i];
					//if (IsEqualTo(sgradient.colorKeys[i].color,_currentColor))
					//{
						newKey.color = _newColor;
					//}
					scolorKeys[i] = newKey;
				}
				for (int i = 0; i < internal_sgradient.alphaKeys.Length; i++)
				{
					GradientAlphaKey newKey = internal_sgradient.alphaKeys[i];
					salphaKeys[i] = newKey;
				}
				Gradient internal_newSGradient = new Gradient();
				internal_newSGradient.SetKeys(scolorKeys, salphaKeys);
				main.startColor = internal_newSGradient;
				break;
		}

		//Replace color over lifetime
		ParticleSystem.ColorOverLifetimeModule internal_colorOverLifetime = _ps.colorOverLifetime;
		var internal_colgradient = internal_colorOverLifetime.color.gradient;
		GradientColorKey[] internal_colorKeys = new GradientColorKey[internal_colgradient.colorKeys.Length];
		GradientAlphaKey[] internal_alphaKeys = new GradientAlphaKey[internal_colgradient.alphaKeys.Length];
		for (int i = 0; i < internal_colgradient.colorKeys.Length; i++)
		{
			GradientColorKey internal_newKey = internal_colgradient.colorKeys[i];
			//if (IsEqualTo(colgradient.colorKeys[i].color, _currentColor))
			//{
				internal_newKey.color = _newColor;
			//}
			internal_colorKeys[i] = internal_newKey;
		}
		for (int i = 0; i < internal_colgradient.alphaKeys.Length; i++)
		{
			GradientAlphaKey internal_newKey = internal_colgradient.alphaKeys[i];
			internal_alphaKeys[i] = internal_newKey;
		}
		Gradient newGradient = new Gradient();
		newGradient.SetKeys(internal_colorKeys, internal_alphaKeys);
		internal_colorOverLifetime.color = newGradient;
	}


	public static bool IsEqualTo(Color _firstColor, Color _secondColor)
	{
		return _firstColor.r == _secondColor.r && _firstColor.g == _secondColor.g && _firstColor.b == _secondColor.b;
	}
}
