using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurroundingPoint : MonoBehaviour
{
    public bool isOccupied_access
    {
        get { return isOccupied; }
        set
        {
            isOccupied = value;
            if (value == true)
            {
                StartCoroutine(TimerBeforeReset());
            }
        }
    }
    private bool isOccupied;

    public EnemyBehaviour closestEnemy;


    void Start()
    {
        isOccupied_access = false;
    }

    public IEnumerator TimerBeforeReset()
    {
        yield return new WaitForSeconds(1);
        isOccupied_access = false;
    }
}
