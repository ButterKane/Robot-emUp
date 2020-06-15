using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public class LoadingScreen : MonoBehaviour
{
	public static bool loading;
	public static Image image;
	public static Image loadingIcon;
	public static bool instantiated;
	public static float loadingDuration = 1.5f; //Hardcoded value, should retrieve the scene loading time 

	private void Awake ()
	{
		if (instantiated == false)
		{
			image = GetComponent<Image>();
			loadingIcon = image.transform.Find("LoadingIcon").GetComponent<Image>();
			DontDestroyOnLoad(transform.parent);
			instantiated = true;
			loading = false;
		}
	}
	public static void StartLoadingScreen(UnityAction _callback)
	{
		DOTween.CompleteAll();
		Camera.main.gameObject.GetComponent<AudioListener>().enabled = false;
		loadingIcon.color = new Color(0, 0, 0, 0);
		loading = true;
		if (image != null)
		{
			image.DOFade(1f, 1f).OnComplete(() => EndLoadingScreen(_callback));
		} else
		{
			image.DOFade(1f, 0);
			EndLoadingScreen(_callback);
		}
	}

	public static void EndLoadingScreen(UnityAction _callback)
	{
		loadingIcon.color = new Color(0, 0, 0, 1);
		_callback.Invoke();
		image.StartCoroutine(FadeAfterDelay(loadingDuration)) ;
	}

	public static IEnumerator FadeAfterDelay(float _delay)
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		Camera.main.gameObject.GetComponent<AudioListener>().enabled = false;
		yield return new WaitForSeconds(_delay);
		Camera.main.gameObject.GetComponent<AudioListener>().enabled = true;
		loading = false;
		loadingIcon.color = new Color(0, 0, 0, 0);
		image.DOFade(0f, 1f);
	}
}
