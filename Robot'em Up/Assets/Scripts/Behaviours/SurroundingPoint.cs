using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurroundingPoint : MonoBehaviour
{
    public bool isOccupied;
    //{
    //    get { return isOccupied; }
    //    set
    //    {
    //        if (value == true)
    //        {
    //            isOccupied = value;
    //            StartCoroutine(TimerBeforeReset());
    //        }
    //        else
    //        {
    //            isOccupied = value;
    //        }
    //    }
    //}

    void Start()
    {
        isOccupied = false;
    }

    private IEnumerator TimerBeforeReset()
    {
        yield return new WaitForSeconds(1);
        isOccupied = false;
    }
}
