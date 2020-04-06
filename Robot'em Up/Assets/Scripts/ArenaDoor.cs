using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ArenaDoor : MonoBehaviour
{
	public bool closeOnArenaStart;
	public Counter currentCounter;
	public float delayBeforeOpening = 1f;
	private List<PlayerController> playerGoneThroughDoor;
	private Collider linkedCollider;

	private Animator animator;
	private void Awake ()
	{
		linkedCollider = GetComponent<Collider>();
		playerGoneThroughDoor = new List<PlayerController>();
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
		yield return new WaitForSeconds(delayBeforeOpening);
		animator.SetBool("Opened", true);
		linkedCollider.isTrigger = true;
		FeedbackManager.SendFeedback("event.ArenaDoorOpening", this);
	}

	public void OpenDoor()
	{
		Debug.Log("DOOR OPENING");
		linkedCollider.isTrigger = true;
		animator.SetBool("Opened", true);
		FeedbackManager.SendFeedback("event.ArenaDoorOpening", this);
	}
	public void CloseDoor()
	{
		linkedCollider.isTrigger = false;
		animator.SetBool("Opened", false);
		FeedbackManager.SendFeedback("event.ArenaDoorClosing", this);
	}
}
