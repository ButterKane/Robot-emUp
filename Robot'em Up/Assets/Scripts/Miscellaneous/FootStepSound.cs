using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootStepSound : MonoBehaviour
{
	PlayerController linkedPlayer;

	private void Awake ()
	{
		linkedPlayer = GetComponentInParent<PlayerController>();
	}
	public void PlayFootstepSound(AnimationEvent _evt)
	{
		if (_evt.animatorClipInfo.weight > 0.5)
		{
			FeedbackManager.SendFeedback("event.PlayerWalkingOnGround", linkedPlayer);
		}
	}
}
