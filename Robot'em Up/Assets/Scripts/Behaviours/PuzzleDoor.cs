using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleDoor : PuzzleActivable
{
    public bool open;
    public GameObject destroyWhenOpened;


    override public void WhenActivate()
    {
        DestroyTheDoor();
    }


    public void DestroyTheDoor()
    {
        open = true;
        FXManager.InstantiateFX(puzzleData.Linked, Vector3.up * 2, true, Vector3.zero, Vector3.one, transform);
        if (destroyWhenOpened != null)
        {
            Destroy(destroyWhenOpened);
        }
    }



}
