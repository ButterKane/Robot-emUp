using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleDoor : PuzzleActivable
{
    public bool open;
    public GameObject destroyWhenOpened;
    public List<PuzzleActivator> activatorsToShutDown;
    public List<PuzzleActivable> activableToShutDown;

    override public void Activate()
    {
        isActivated = true;
        UpdateLights();
        foreach (var item in activatorsToShutDown)
        {
            item.ShutDownPuzzleActivator();
        }
        foreach (var item in activableToShutDown)
        {
            item.ShutDownPuzzle();
        }
        DestroyTheDoor();
    }

    public void DestroyTheDoor()
    {
        if (!open)
        {
            FeedbackManager.SendFeedback("event.PuzzleDoorOpen", this, transform.position, Vector3.zero, Vector3.zero);
        }

        open = true;
        
        foreach (var item in indictatorLightsList)
        {
            item.gameObject.SetActive(false);
        }
        if (destroyWhenOpened != null)
        {
            Destroy(destroyWhenOpened);
        }
    }
}
