using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HighlighterDatas", menuName = "GlobalDatas/HighlighterDatas", order = 1)]
public class HighlighterDatas : ScriptableObject
{
	public float duration;
	public float fadeInDuration;
	public float fadeOutDuration;

	public Color defaultFirstColor = Color.white;
	public Color defaultSecondColor = Color.white;

	public float arrowSpawnHeight;
	public float arrowMinZOffset;
	public float arrowMaxZOffset;
	public float arrowHoverSpeed;

	public float circleZOffset;
	public float circleMinSize;
	public float circleMaxSize;
	public float circleAnimationSpeed;

	public float minEmissionForce;
	public float maxEmissionForce;
	public float emissionLerpSpeed;
}
