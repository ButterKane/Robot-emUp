using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class PuzzleRepeater : PuzzleActivator
{
    [Range(0, 10)]
    public float speedChange;
    [ReadOnly]
    public float timeSpeedChange;


    void Awake()
    {
        timeSpeedChange = speedChange;
    }

    // Update is called once per frame
    void Update()
    {
        timeSpeedChange -= Time.deltaTime;
        if (timeSpeedChange < 0 )
        {
            timeSpeedChange = speedChange;
            if (isActivated)
            {
                DesactiveLinkedObjects();
                isActivated = false;
                UpdateLight();
            }
            else
            {
                ActivateLinkedObjects();
                isActivated = true;
                UpdateLight();
            }
        }
    }


}
