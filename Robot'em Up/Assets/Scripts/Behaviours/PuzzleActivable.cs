using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleActivable : MonoBehaviour
{
    public PuzzleDatas puzzleData;
    [SerializeField] public List<PuzzleActivator> puzzleActivators;
    [SerializeField] public List<PuzzleActivator> puzzleDesactivator;
    [SerializeField] public bool needAllConditions = false;
    public bool isActivated = true;
    public virtual void WhenActivate()
    {
        isActivated = true;
    }


    public virtual void WhenDesactivate()
    {
        isActivated = false;
    }

}
