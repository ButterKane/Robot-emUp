using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleActivable : MonoBehaviour
{
    [SerializeField] public List<PuzzleActivator> puzzleActivators;
    [SerializeField] public List<PuzzleActivator> puzzleDesactivator;
    public virtual void WhenActivate()
    { }


    public virtual void WhenDesactivate()
    { }

}
