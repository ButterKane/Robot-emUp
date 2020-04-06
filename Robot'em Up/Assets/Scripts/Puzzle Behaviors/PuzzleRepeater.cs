using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using TMPro;


public class PuzzleRepeater : PuzzleActivator
{
    [Header("Puzzle Repeater")]
    [Range(0, 10)]
    public float speedChange;
    [ReadOnly]
    public float timeSpeedChange;
    public float startSpeed = 0;
    public MeshRenderer CompletionShader;
    public TextMeshPro textMesh;
    public bool Test1;


    void Awake()
    {
        timeSpeedChange = startSpeed;
    }

    void Update()
    {
        timeSpeedChange -= Time.deltaTime;
        CompletionShader.material.SetFloat("_AddToCompleteCircle", timeSpeedChange / speedChange);
       
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

        FeedbackManager.SendFeedback("event.PuzzleRepeaterActivation", this);
    }

    public virtual void DeactivatedAction()
    {
        ActivateLinkedObjects();
        isActivated = true;
        UpdateLight();
		FeedbackManager.SendFeedback("event.PuzzleRepeaterDesactivation", this);
	}


}
