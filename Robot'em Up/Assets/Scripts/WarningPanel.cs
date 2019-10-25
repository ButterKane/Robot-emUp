using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningPanel : MonoBehaviour
{
	private Animator animator;
	private static WarningPanel instance;
	private GameObject go;
	private void Awake ()
	{
		instance = this;
		animator = GetComponent<Animator>();
		go = gameObject;
		gameObject.SetActive(false);
	}

	public void DisablePanel()
	{
		instance.go.SetActive(false);
	}

	public static void OpenPanel()
	{
		instance.go.SetActive(true);
		instance.animator.SetTrigger("Init");
	}

	public static void ClosePanel()
	{
		instance.animator.SetTrigger("Close");
	}
}
