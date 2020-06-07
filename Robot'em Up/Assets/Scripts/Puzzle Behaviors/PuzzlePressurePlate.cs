using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class PuzzlePressurePlate : PuzzleActivator
{
    [ReadOnly] public bool pawnHere;
    private List<PawnController> pawnHereList = new List<PawnController>();
    public Animator animator;


    private void OnTriggerEnter(Collider _other)
    {
        PawnController foundPawn = _other.gameObject.GetComponent<PawnController>();
        if (foundPawn && !shutDown)
        {
            if (!pawnHereList.Contains(foundPawn))
            {
                pawnHereList.Add(foundPawn);
                pawnHere = true;
                transform.localScale = new Vector3(transform.localScale.x, 0.3f, transform.localScale.z);
                if (!isActivated)
                {
                    FeedbackManager.SendFeedback("event.PuzzlePressurePlateActivation", this);
                }
                isActivated = true;
                animator.SetBool("Activated", true);
                ActivateLinkedObjects();
            }
        }

        //UpdateLight();
    }


    private void OnTriggerExit(Collider _other)
    {
        PawnController foundPawn = _other.gameObject.GetComponent<PawnController>();
        if (foundPawn)
        {
            if (pawnHereList.Contains(foundPawn))
            {
                pawnHereList.Remove(foundPawn);
                if (pawnHereList.Count < 1)
                {
                    if (isActivated)
                    {
                        FeedbackManager.SendFeedback("event.PuzzlePressurePlateDesactivation", this);
                    }
                    isActivated = false;
                    animator.SetBool("Activated", false);

                    DesactiveLinkedObjects();
                    pawnHere = false;
                    transform.localScale = new Vector3(transform.localScale.x, 1f, transform.localScale.z);
                }
            }
        }

        //UpdateLight();

    }


    override public void CustomShutDown()
    {
        transform.localScale = new Vector3(transform.localScale.x, 0.3f, transform.localScale.z);
        isActivated = false;
        animator.SetBool("Activated", false);
    }
}
