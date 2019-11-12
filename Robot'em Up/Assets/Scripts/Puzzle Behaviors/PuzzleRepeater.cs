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

    public TextMesh textMesh;


    void Awake()
    {
        timeSpeedChange = speedChange;
    }

    // Update is called once per frame
    void Update()
    {
        timeSpeedChange -= Time.deltaTime;
        textMesh.text = System.Math.Round(timeSpeedChange, 1).ToString();
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
