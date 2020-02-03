using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarrationManager : MonoBehaviour
{
    public static NarrationManager narrationManager;
    
    public AudioSource myAudioSource;

    [Header("Read-Only")]
    public NarrativeInteractiveElements currentNarrationElementActivated;

    // Start is called before the first frame update
    void Start()
    {
        narrationManager = this;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void LaunchDialogue(AudioClip _clip)
    {
        myAudioSource.clip = _clip;
        myAudioSource.PlayOneShot(_clip);
    }

    public void ChangeActivatedNarrationElement(NarrativeInteractiveElements _newNarrativeElement)
    {
        currentNarrationElementActivated = _newNarrativeElement;
        if(currentNarrationElementActivated != null)
        {
            myAudioSource.volume = 1f;
            myAudioSource.outputAudioMixerGroup = currentNarrationElementActivated.myAudioMixer;
        }
        else
        {
            myAudioSource.volume = 0f;
        }
    }
}
