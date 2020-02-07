using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class WhiteTransition : MonoBehaviour
{
	private bool inited;
	public Image whiteTransition;
	public float transitionSpeed = 0.5f;
	private void OnTriggerEnter ( Collider other )
	{
		if (!inited && other.GetComponent<PlayerController>() != null)
		{
			inited = true;
			StartCoroutine(WhiteTransition_C());
		}
	}

	IEnumerator WhiteTransition_C()
	{
		Time.timeScale = 0f;
		Color whiteTransitionColor = whiteTransition.color;
		for (float i = 0; i < 1; i+= Time.deltaTime * transitionSpeed)
		{
			whiteTransitionColor.a = i;
			whiteTransition.color = whiteTransitionColor;
			yield return null;
		}
		whiteTransitionColor.a = 1f;
		whiteTransition.color = whiteTransitionColor;
		yield return new WaitForSeconds(1f);
		Time.timeScale = 1f;
		GameManager.LoadSceneByIndex(0);
		yield return null;
	}
}
