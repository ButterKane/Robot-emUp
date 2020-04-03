using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarrationElementsRangeTrigger : MonoBehaviour
{
    public NarrativeInteractiveElements myElement;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MiddlePoint"))
        {
            myElement.SetInRange(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MiddlePoint"))
        {
            myElement.SetInRange(false);
        }
    }
}
