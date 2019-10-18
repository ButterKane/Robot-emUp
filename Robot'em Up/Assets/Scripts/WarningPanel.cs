using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningPanel : MonoBehaviour
{
	private Animator animator;
	private void Awake ()
	{
		animator = GetComponent<Animator>();
	}

	public void DisablePanel()
	{
		gameObject.SetActive(false);
	}

	public void OpenPanel()
	{
		gameObject.SetActive(true);
		animator.SetTrigger("Init");
	}

	public void ClosePanel()
	{
		animator.SetTrigger("Close");
	}
}
