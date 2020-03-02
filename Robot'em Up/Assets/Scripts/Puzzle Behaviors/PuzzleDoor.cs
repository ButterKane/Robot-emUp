using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleDoor : PuzzleActivable
{
    public bool open;
    public GameObject destroyWhenOpened;
    public List<PuzzleActivator> activatorsToShutDown;

    override public void WhenActivate()
    {
        //Debug.Log("Activate a door");
        isActivated = true;
        UpdateLights();
        foreach (var item in activatorsToShutDown)
        {
            //Debug.Log("Try to shut down");
            item.shutDownPuzzleActivator();
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
            //FXManager.InstantiateFX(puzzleData.linked, Vector3.up * 2, true, Vector3.zero, Vector3.one);
        }
    }



}
