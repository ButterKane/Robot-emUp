using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SpeakerBehavior : NarrativeInteractiveElements
{
    public Rigidbody speakerRB;
    public float forwardForceThrow;
    public float downForceThrow;

    public override void Break()
    {
        base.Break();
        speakerRB.transform.parent = null;
        speakerRB.isKinematic = false;
        speakerRB.AddForce(transform.forward * forwardForceThrow + -transform.up * downForceThrow);
    }

    public override void EndPossessionAnimationEvents()
    {
        base.EndPossessionAnimationEvents();
        if (possessed)
        {
            myAudioSource.enabled = true;
        }
        else
        {
            myAudioSource.enabled = false;
        }
    }
}
