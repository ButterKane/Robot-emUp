using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using TMPro;


public class PuzzleRepeater : PuzzleActivator
{
    [Range(0, 10)]
    public float speedChange;
    [ReadOnly]
    public float timeSpeedChange;
    public float startSpeed = 0;
    public TextMeshPro textMesh;


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

        SoundManager.PlaySound("RepeaterActivate", transform.position, transform);
    }

    public virtual void DeactivatedAction()
    {
        ActivateLinkedObjects();
        isActivated = true;
        UpdateLight();
        SoundManager.PlaySound("RepeaterDesactivate", transform.position, transform);
    }


}
