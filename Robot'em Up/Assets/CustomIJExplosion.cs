using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomIJExplosion : MonoBehaviour
{
	public List<GameObject> explodedVisuals;
	public List<GameObject> initialVisuals;
	public float delayBeforeExplosion;
	public float fxSizeMultiplier = 1f;
	public GameObject explosionFX;

	private void Awake ()
	{
		foreach (GameObject e in explodedVisuals)
		{
			e.SetActive(false);
		}
		foreach (GameObject i in initialVisuals)
		{
			i.SetActive(true);
		}
	}

	public void Explode()
	{
		StartCoroutine(ExplodeAfterDelay_C());
	}

	IEnumerator ExplodeAfterDelay_C()
	{
		yield return new WaitForSeconds(delayBeforeExplosion);
		GameObject newFX = Instantiate(explosionFX, transform.position, Quaternion.identity);
		newFX.transform.localScale = Vector3.one * 10f * fxSizeMultiplier;

		foreach (GameObject e in explodedVisuals)
		{
			e.SetActive(true);
		}
		foreach (GameObject i in initialVisuals)
		{
			i.SetActive(false);
		}
	}
}
