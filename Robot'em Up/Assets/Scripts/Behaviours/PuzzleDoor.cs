using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleDoor : MonoBehaviour
{
    public PuzzleDatas puzzleData;
    public List<PuzzleLink> linksNeededToOpen;
    public bool open;
    public GameObject destroyWhenOpened;

   
    public void checkIfValid()
    {
        bool tempValid = true;
        foreach (PuzzleLink link in linksNeededToOpen)
        {
            if (!link.isActivated)
            {
                tempValid = false;
            }
        }
        if (tempValid)
        {
            openTheDoor();
        }
        
    }


    public void openTheDoor()
    {
        open = true;
        FXManager.InstantiateFX(puzzleData.Linked, Vector3.up * 1, true, transform);
        if (destroyWhenOpened != null)
        {
            Destroy(destroyWhenOpened);
        }
    }



}
