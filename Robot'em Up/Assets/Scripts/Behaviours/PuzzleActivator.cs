using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleActivator : MonoBehaviour
{
    public float signalPower;
    public bool unlimitedTime;
    // Update is called once per frame
    public virtual void ActivateLinkedObjects()
    {
        PuzzleActivable[] activables = FindObjectsOfType<PuzzleActivable>();
        foreach (var item in activables)
        {
            if (item.puzzleActivators.Contains(this))
            {
                item.WhenActivate();
            }
        }

        foreach (var item in activables)
        {
            if (item.puzzleDesactivator.Contains(this))
            {
                item.WhenDesactivate();
            }
        }
    }


    public virtual void DesactiveLinkedObjects()
    {
        PuzzleActivable[] activables = FindObjectsOfType<PuzzleActivable>();
        foreach (var item in activables)
        {
            if (item.puzzleActivators.Contains(this))
            {
                item.WhenDesactivate();
            }
        }

        foreach (var item in activables)
        {
            if (item.puzzleDesactivator.Contains(this))
            {
                item.WhenActivate();
            }
        }
    }
}
