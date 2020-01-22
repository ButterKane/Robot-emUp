using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlighter : MonoBehaviour
{
	public static Highlighter instance;
	private HighlighterDatas datas;
	private List<MaterialReplacer> materialReplacers = new List<MaterialReplacer>();
	private GameObject circle;
	private GameObject arrow;
	private float remainingLifeTime;
	private float passedTime;
	private Coroutine fadeCoroutine;
	private Transform previousParent;
	private bool isOnBall;

	/*
	Stores all the previous materials on the renderers
	Replace all the materials by the "Highlight" material
	Fade-in:
		- Highlight circle grows from the center and opacity will fade in
		- Highlight arrow will spawn from above, go down and opacity will fade in
		- New materials emissive starts at 1 and goes up

	Update:
		- Highlight circle scales up and down
		- Highlight arrow goes up and down
		- New material emissive goes heavy and light

	Fade-out:
		- Highlight  circle opacity fade out
		- Highlight arrow opacity fade out
		- New materials emissive goes from current to 1
	*/
	
	public static void HighlightBall (bool _heritRemainingTime = false)
	{
		HighlighterDatas datas = Resources.Load<HighlighterDatas>("HighlighterDatas");
		instance = HighlightObject(BallBehaviour.instance.transform, datas.defaultFirstColor, datas.defaultSecondColor, _heritRemainingTime) ;
		if (instance != null)
		{
			instance.isOnBall = true;
		}
	}

	public static void DetachBallFromPlayer ()
	{
		Highlighter highlighter = instance;
		if (highlighter == null) { return; }
		if (!highlighter.isOnBall)
		{
			HighlightBall(true);
		}
	}

	public static Highlighter HighlightObject(Transform _object, Color _color, Color _secondaryColor, bool _heritRemainingTime = false)
	{
		HighlighterDatas datas = Resources.Load<HighlighterDatas>("HighlighterDatas");
		float remainingTime = datas.duration;
		if (instance != null) { 
			if (_heritRemainingTime)
			{
				remainingTime = instance.remainingLifeTime;
				if (remainingTime <= 0) { return null; }
			}
			Destroy(instance);
		}

		Highlighter highlighter = _object.GetComponent<Highlighter>();
		if (highlighter != null)
		{
			Destroy(highlighter);
		}
		Highlighter newHighlighter = _object.gameObject.AddComponent<Highlighter>();
		newHighlighter.remainingLifeTime = remainingTime;
		Shader.SetGlobalColor("_HighlightColor", _color);
		Shader.SetGlobalColor("_SecondOutlineColor", _secondaryColor);
		newHighlighter.StartCoroutine(newHighlighter.Init_C());
		instance = newHighlighter;
		return newHighlighter;
	}

	public void Init ()
	{
		//Generates circle, arrow, and replace materials
		CheckIfHeldByPlayer();

		datas = Resources.Load<HighlighterDatas>("HighlighterDatas");
		Shader.SetGlobalFloat("_MinEmissiveIntensity", datas.minEmissionForce);
		Shader.SetGlobalFloat("_MaxEmissiveIntensity", datas.maxEmissionForce);
		Shader.SetGlobalFloat("_EmissiveLerpSpeed", datas.emissionLerpSpeed);
		materialReplacers = new List<MaterialReplacer>();
		foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
		{
			if (renderer.GetType() == typeof(ParticleSystemRenderer)) { continue; }
			MaterialReplacer newReplacer = renderer.gameObject.AddComponent<MaterialReplacer>();
			materialReplacers.Add(newReplacer);
			newReplacer.ReplaceMaterial(Resources.Load<Material>("HighlightResource/M_HighlightedObject"));
		}

		circle = Instantiate(Resources.Load<GameObject>("HighlightResource/HighlightCircle"));
		circle.transform.SetParent(transform);
		circle.transform.localPosition = Vector3.zero + new Vector3(0, datas.circleZOffset, 0);
		circle.transform.localRotation = Quaternion.identity;
		circle.transform.localScale = Vector3.one * datas.circleMinSize;

		arrow = Instantiate(Resources.Load<GameObject>("HighlightResource/HighlightArrow"));
		arrow.transform.SetParent(transform);
		arrow.transform.localPosition = Vector3.zero + new Vector3(0, datas.arrowMinZOffset, 0);
		arrow.transform.localRotation = Quaternion.identity;
		arrow.transform.localScale = Vector3.one;

		previousParent = transform.parent;
		passedTime = 0;
		fadeCoroutine = StartCoroutine(FadeIn_C());
	}

	private void Update ()
	{
		if (isOnBall && transform.parent != previousParent)
		{
			CheckIfHeldByPlayer();
		}
	}

	private void CheckIfHeldByPlayer()
	{
		if (isOnBall)
		{
			PlayerController potentialPlayer = GetComponentInParent<PlayerController>();
			if (potentialPlayer)
			{
				HighlightObject(potentialPlayer.transform.Find("Model"), potentialPlayer.highlightedColor, potentialPlayer.highlightedSecondColor, true);
				Destroy(this);
			}
		}
	}

	private void LateUpdate ()
	{
		if (arrow != null)
		{
			arrow.transform.LookAt(arrow.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
			circle.transform.rotation = Quaternion.LookRotation(Vector3.up);
		}
		if (circle != null)
		{
			circle.transform.position = transform.position;
		}
		if (remainingLifeTime > 0 && fadeCoroutine != null)
		{
			passedTime += Time.deltaTime;
			if (circle)
			{
				circle.transform.localScale = Vector3.Lerp(Vector3.one * datas.circleMinSize, Vector3.one * datas.circleMaxSize, 1 - ((Mathf.Cos(passedTime * datas.circleAnimationSpeed) + 1f) / 2f));
			}
			if (arrow)
			{
				arrow.transform.position = Vector3.Lerp(new Vector3(transform.position.x, transform.position.y + datas.arrowMinZOffset, transform.position.z), new Vector3(transform.position.x, transform.position.y + datas.arrowMaxZOffset, transform.position.z), 1 - ((Mathf.Cos(passedTime * datas.arrowHoverSpeed) + 1f) / 2f));
			}
			remainingLifeTime -= Time.deltaTime;
			if (remainingLifeTime <= 0)
			{
				fadeCoroutine = StartCoroutine(FadeOut_C());
			}
		}
	}

	private void OnDestroy ()
	{
		//Deletes circle, arrow, and reset material
		foreach (MaterialReplacer replacer in materialReplacers)
		{
			Destroy(replacer);
		}
		if (circle)
		{
			Destroy(circle.gameObject);
		}
		if (arrow)
		{
			Destroy(arrow.gameObject);
		}
	}

	public IEnumerator FadeIn_C()
	{
		SpriteRenderer circleSprite = circle.GetComponentInChildren<SpriteRenderer>();
		SpriteRenderer arrowSprite = arrow.GetComponent<SpriteRenderer>();
		Vector3 arrowPosition = arrow.transform.localPosition;
		for (float i = 0; i < datas.fadeInDuration; i+= Time.deltaTime)
		{
			Color circleColor = circleSprite.color;
			circleColor.a = Mathf.Lerp(0f, 1f, i / datas.fadeInDuration);
			circleSprite.color = circleColor;
			circle.transform.localScale = Vector3.one * Mathf.Lerp(0, datas.circleMinSize, i / datas.fadeInDuration);

			Color arrowColor = arrowSprite.color;
			arrowColor.a = Mathf.Lerp(0f, 1f, i / datas.fadeInDuration);
			arrowSprite.color = arrowColor;
			arrow.transform.localPosition = Vector3.Lerp(arrowPosition + Vector3.up * datas.arrowSpawnHeight, arrowPosition, i / datas.fadeInDuration);

			Shader.SetGlobalFloat("_EmissiveMultiplier", i / datas.fadeInDuration);
			yield return null;
		}
	}

	public IEnumerator FadeOut_C()
	{
		SpriteRenderer circleSprite = circle.GetComponentInChildren<SpriteRenderer>();
		SpriteRenderer arrowSprite = arrow.GetComponent<SpriteRenderer>();
		float circleScale = circle.transform.localScale.x;
		for (float i = 0; i < datas.fadeOutDuration; i += Time.deltaTime)
		{
			Color circleColor = circleSprite.color;
			circleColor.a = Mathf.Lerp(1f, 0f, i / datas.fadeOutDuration);
			circleSprite.color = circleColor;
			circle.transform.localScale = Vector3.one * Mathf.Lerp(circleScale, 0, i / datas.fadeOutDuration);

			Color arrowColor = arrowSprite.color;
			arrowColor.a = Mathf.Lerp(1f, 0f, i / datas.fadeOutDuration);
			arrowSprite.color = arrowColor;

			Shader.SetGlobalFloat("_EmissiveMultiplier", 1f - (i / datas.fadeOutDuration));
			yield return null;
		}
		instance = null;
		Destroy(this);
	}

	IEnumerator Init_C()
	{
		yield return new WaitForEndOfFrame();
		Init();
	}
}
