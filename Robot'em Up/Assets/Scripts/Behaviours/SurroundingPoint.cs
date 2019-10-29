using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurroundingPoint : MonoBehaviour
{
    public bool isOccupied
    {
        get { return _isOccupied; }
        set
        {
            _isOccupied = value;
            if (value == true)
            {
                StartCoroutine(TimerBeforeReset());
            }
        }
    }

    private bool _isOccupied;

    void Start()
    {
        isOccupied = false;
    }

    public IEnumerator TimerBeforeReset()
    {
        yield return new WaitForSeconds(1);
        isOccupied = false;
    }
}
