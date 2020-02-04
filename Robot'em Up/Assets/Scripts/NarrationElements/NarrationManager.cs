using MyBox;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NarrationManager : MonoBehaviour
{
    public static NarrationManager narrationManager;

    public AudioSource myAudioSource;

    [Header("Read-Only")]
    public NarrativeInteractiveElements currentNarrationElementActivated;

    // Dialogue box and data
    public GameObject dialogueBoxPrefab;
    public float typingSpeed = 5;
    private GameObject dialogueBoxInstance;
    private TextMeshProUGUI textField;
    private Image subImage;
    private IEnumerator textWritingCoroutine;

    // Start is called before the first frame update
    void Awake()
    {
        narrationManager = this;

        textWritingCoroutine = null;
        dialogueBoxInstance = null;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static void LaunchDialogue(DialogueData _dialogueData)
    {
        narrationManager.myAudioSource.clip = _dialogueData.dialogueClip;
        narrationManager.myAudioSource.PlayOneShot(_dialogueData.dialogueClip);

        if (narrationManager.textWritingCoroutine == null)
        {
            narrationManager.textWritingCoroutine = narrationManager.WriteText_C(_dialogueData);
            narrationManager.StartCoroutine(narrationManager.textWritingCoroutine);
        }
    }

    public void ChangeActivatedNarrationElement(NarrativeInteractiveElements _newNarrativeElement)
    {
        currentNarrationElementActivated = _newNarrativeElement;
        if(currentNarrationElementActivated != null)
        {
            myAudioSource.volume = 1f;
            myAudioSource.outputAudioMixerGroup = currentNarrationElementActivated.myAudioMixer;
        }
        else
        {
            myAudioSource.volume = 0f;
        }
    }

    public IEnumerator WriteText_C(DialogueData _dialogueData)
    {
        if (currentNarrationElementActivated != null)   // Check if IA's Screen is still active
        {
            for (int j = 0; j < _dialogueData.texts.Length; j++)
            {
                for (int i = 0; i < _dialogueData.texts[j].text.Length + 1; i++)
                {
                    textField.text = _dialogueData.texts[j].text.Substring(0, i);
                    yield return new WaitForSeconds(1 / typingSpeed);
                }

                yield return new WaitForSeconds(_dialogueData.texts[j].pauseAfterText);
            }
        }
        else
        {
            yield return new WaitForSeconds(_dialogueData.timeBeforeDestruction);
            textWritingCoroutine = null;
        }
        Destroy(dialogueBoxInstance);
        Destroy(gameObject);
    }

    public IEnumerator BlinkSubImage()
    {
        while (true)
        {
            subImage.enabled = !subImage.enabled;
            yield return new WaitForSeconds(0.5f);
        }
    }

}
