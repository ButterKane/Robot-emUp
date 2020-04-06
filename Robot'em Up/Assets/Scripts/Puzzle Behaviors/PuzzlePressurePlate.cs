using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class PuzzlePressurePlate : PuzzleActivator
{
    [ReadOnly] public bool pawnHere;
    private int totalPawnsHere;

    void Awake()
    {
        totalPawnsHere = 0;
    }


    private void OnTriggerEnter(Collider _other)
    {
        PawnController foundPawn = _other.gameObject.GetComponent<PawnController>();
        if (foundPawn && !shutDown)
        {
            pawnHere = true;
            transform.localScale = new Vector3(transform.localScale.x, 0.3f, transform.localScale.z);
            totalPawnsHere++;
            if (!isActivated)
			{
				FeedbackManager.SendFeedback("event.PuzzlePressurePlateActivation", this);
			}
            isActivated = true;
            ActivateLinkedObjects();
        }

        UpdateLight();
    }


    private void OnTriggerExit(Collider _other)
    {
        PawnController foundPawn = _other.gameObject.GetComponent<PawnController>();
        if (foundPawn)
        {
            totalPawnsHere--;
            if (totalPawnsHere < 1)
            {
				if (isActivated)
				{
					FeedbackManager.SendFeedback("event.PuzzlePressurePlateDesactivation", this);
				}
				isActivated = false;
                DesactiveLinkedObjects();
                pawnHere = false;
                transform.localScale = new Vector3(transform.localScale.x, 1f, transform.localScale.z);
            }
        }

        UpdateLight();

    }


    override public void CustomShutDown()
    {
        transform.localScale = new Vector3(transform.localScale.x, 0.3f, transform.localScale.z);
        isActivated = false;
    }
}
