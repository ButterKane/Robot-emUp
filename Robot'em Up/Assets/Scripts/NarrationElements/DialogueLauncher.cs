using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueLauncher : MonoBehaviour
{
    public void LaunchDialogueEvent(DialogueData _dialogueData)
    {
        NarrationManager.narrationManager.LaunchDialogue(_dialogueData);
    }
}
