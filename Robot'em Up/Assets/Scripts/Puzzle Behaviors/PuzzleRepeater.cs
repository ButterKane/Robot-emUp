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
    public float startSpeed = 0;

    public TextMesh textMesh;


    void Awake()
    {
        timeSpeedChange = startSpeed;
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
                ActivatedAction();
            }
            else
            {
                DeactivatedAction();
            }
        }
    }

    public virtual void ActivatedAction()
    {
        DesactiveLinkedObjects();
        isActivated = false;
        UpdateLight();
    }

    public virtual void DeactivatedAction()
    {
        ActivateLinkedObjects();
        isActivated = true;
        UpdateLight();
    }


}
