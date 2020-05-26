using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiodeController : MonoBehaviour
{
	public float blueRandomness = 30f;
	public float redRandomness = 30f;
	public float minColorLerpSpeed = 0.8f;
	public float maxColorLerpSpeed = 1.2f;
	public List<MeshRenderer> diodeList = new List<MeshRenderer>();
	public int diodeID;

	Dictionary<MeshRenderer, Color> maxColors;
	Dictionary<MeshRenderer, Color> minColors;
	Dictionary<MeshRenderer, float> lerpSpeed;

	private bool activated;

	private void Awake ()
	{
		DiodeManager.RegisterDiodeController(this, diodeID);

		//Create dictionaries
		minColors = new Dictionary<MeshRenderer, Color>();
		maxColors = new Dictionary<MeshRenderer, Color>();
		lerpSpeed = new Dictionary<MeshRenderer, float>();

		//Create material instances
		foreach (MeshRenderer d in diodeList)
		{
			d.sharedMaterial = new Material(d.sharedMaterial);
			d.sharedMaterial.DisableKeyword("_EMISSION");
			Color currentColor = d.sharedMaterial.GetColor("_EmissionColor");
			Color minColor = new Color(currentColor.r, currentColor.g, currentColor.b - (Random.Range(blueRandomness, blueRandomness * 2f)), 1f);
			Color maxColor = new Color(currentColor.r - (Random.Range(redRandomness, redRandomness * 2f)), currentColor.g, currentColor.b, 1f);
			Debug.Log("Min: " + minColor + " Max: " + maxColor);
			minColors[d] = minColor;
			maxColors[d] = maxColor;
			lerpSpeed[d] = Random.Range(minColorLerpSpeed, maxColorLerpSpeed);
		}
	}
	public void Activate()
	{
		if (!activated)
		{
			activated = true;
			foreach (MeshRenderer mr in diodeList)
			{
				StartCoroutine(ActivationCoroutine_C(mr));
			}
		}
	}

	public void Update ()
	{
		if (activated)
		{
			foreach (MeshRenderer d in diodeList)
			{
				d.sharedMaterial.SetColor("_EmissionColor", Color.Lerp(minColors[d], maxColors[d], Mathf.PingPong(Time.time, lerpSpeed[d])));
			}
		}	
	}

	IEnumerator ActivationCoroutine_C(MeshRenderer r)
	{
		yield return new WaitForSeconds(Random.Range(0f, 2f));

		int iterationCount = Random.Range(5, 8);
		bool activated = false;
		for (int i = 0; i < iterationCount; i++)
		{
			yield return new WaitForSeconds(Random.Range(0.2f, 0.3f) * (1f - ((float)i / (float)iterationCount)));
			if (activated)
			{
				r.sharedMaterial.DisableKeyword("_EMISSION");
				activated = false;
			} else
			{
				r.sharedMaterial.EnableKeyword("_EMISSION");
				activated = true;
			}
		}
		r.sharedMaterial.EnableKeyword("_EMISSION");
	}
}
