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
    public Light myLight;


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
                ActivateLinkedObjects();
                isActivated = false;
                myLight.color = puzzleData.RepeaterDesactivate;
            }
            else
            {
                DesactiveLinkedObjects();
                isActivated = true;
                myLight.color = puzzleData.RepeaterActivate;
            }
        }
    }


}
