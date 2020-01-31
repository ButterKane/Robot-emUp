using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarrativeInteractiveActivator : MonoBehaviour
{
    public NarrativeInteractiveElements myScreen;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && other.name.Contains("Player") && !myScreen.possessed)
        {
            print("possessed");
            myScreen.SetAIPossession(true);
        }
        else if (other.tag == "Player" && other.name.Contains("Player") && myScreen.possessed)
        {
            print("NOT possessed");
            myScreen.SetAIPossession(false);
        }
    }
}
