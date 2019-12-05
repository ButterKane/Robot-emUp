using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootStepSound : MonoBehaviour
{
	public void PlayFootstepSound(AnimationEvent evt)
	{
		if (evt.animatorClipInfo.weight > 0.5)
		{
			SoundManager.PlaySound("WalkingOnGround", transform.position);
		}
	}
}
