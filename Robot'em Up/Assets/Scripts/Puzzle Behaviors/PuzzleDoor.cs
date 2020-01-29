using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleDoor : PuzzleActivable
{
    public bool open;
    public GameObject destroyWhenOpened;

    override public void WhenActivate()
    {
        isActivated = true;
        UpdateLights();
        //Debug.Log("Activate a door");
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
