using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueText
{
    [TextArea(5, 15)]
    public string text;
    public float pauseAfterText;
}

[CreateAssetMenu(fileName = "DialogueData", menuName = "GlobalDatas/DialogueDatas", order = 1)]
public class DialogueData : ScriptableObject
{
    [Separator("Main audio")]
    public AudioClip dialogueClip;

    [Separator("Written text managing")]
    public DialogueText[] texts;

    [Separator("Final Time")]
    public float timeBeforeDestruction = 2;
}
