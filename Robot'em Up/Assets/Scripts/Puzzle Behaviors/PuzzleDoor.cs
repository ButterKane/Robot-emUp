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
        open = true;

        FeedbackManager.SendFeedback("event.doorOpen", this);
        SoundManager.PlaySound("DoorOpen", transform.position, transform);

        foreach (var item in indictatorLightsList)
        {
            item.gameObject.SetActive(false);
        }
        if (destroyWhenOpened != null)
        {
            //Debug.Log("Destroy a door");
            Destroy(destroyWhenOpened);
            FXManager.InstantiateFX(puzzleData.Linked, Vector3.up * 2, true, Vector3.zero, Vector3.one);
        }
    }



}
