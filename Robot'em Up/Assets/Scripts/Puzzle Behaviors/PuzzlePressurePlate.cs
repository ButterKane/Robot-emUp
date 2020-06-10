using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using UnityEngine.Events;

public class PuzzlePressurePlate : PuzzleActivator
{
    [ReadOnly] public bool pawnHere;
    private List<PawnController> pawnHereList = new List<PawnController>();
    public Animator animator;
    public UnityEvent activatedEvent;
    public UnityEvent deactivatedEvent;

    private void OnTriggerEnter(Collider _other)
    {
        PlayerController foundPawn = _other.gameObject.GetComponent<PlayerController>();
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
                activatedEvent.Invoke();
            }
        }

        //UpdateLight();
    }


    private void OnTriggerExit(Collider _other)
    {
        PlayerController foundPawn = _other.gameObject.GetComponent<PlayerController>();
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
                    deactivatedEvent.Invoke();
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
