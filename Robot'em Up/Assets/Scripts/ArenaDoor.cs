using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaDoor : MonoBehaviour
{
	public bool closeOnArenaStart;
	public Counter currentCounter;
	public float delayBeforeOpening = 1f;

	private Animator animator;
	private void Awake ()
	{
		animator = GetComponent<Animator>();
		if (!closeOnArenaStart)
		{
			animator.SetBool("Opened", false);
		} else
		{
			animator.SetBool("Opened", true);
		}
	}
	public void OnArenaFinished()
	{
		StartCoroutine(OpenAfterDelay_C());
	}

	public void OnArenaEnter()
	{
		if (closeOnArenaStart)
		{
			animator.SetBool("Opened", false);
			FeedbackManager.SendFeedback("event.ArenaDoorClosing", this);
		}
	}

	public void OnWaveStart()
	{
		if (currentCounter != null)
		{
			currentCounter.SetCounterToWaiting();
		}
	}

	public void OnWaveFinished ()
	{
		if (currentCounter != null)
		{
			currentCounter.IncreaseCounter(1);
		}
	}

	IEnumerator OpenAfterDelay_C()
	{
		Debug.Log("Opening after delay");
		yield return new WaitForSeconds(delayBeforeOpening);
		animator.SetBool("Opened", true);
		FeedbackManager.SendFeedback("event.ArenaDoorOpening", this);
	}
}
