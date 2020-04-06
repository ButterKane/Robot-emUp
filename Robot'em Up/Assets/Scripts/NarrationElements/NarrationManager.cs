using MyBox;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class NarrationManager : MonoBehaviour
{
    public static NarrationManager narrationManager;

    public AudioSource myAudioSource;
    public AudioMixerGroup defaultAudioMixer;

    [Header("Read-Only")]
    public NarrativeInteractiveElements currentNarrationElementActivated;

    // Dialogue box and data
    public GameObject dialogueBoxPrefab;
    public float typingSpeed = 5;
    private GameObject dialogueBoxInstance;
    private TextMeshProUGUI textField;
    public Image subImage;
    private IEnumerator textWritingCoroutine;

    List<NarrativeInteractiveElements> narrativeElementsInRange = new List<NarrativeInteractiveElements>();
    public float timeBetweenRangeCheck;
    [HideInInspector] public Transform player1Transform;
    [HideInInspector] public Transform player2Transform;
    Vector3 middlePlayerPosition;
    NarrativeInteractiveElements activatedNarrativeElement;

    // Start is called before the first frame update
    void Awake()
    {
        narrationManager = this;

        textWritingCoroutine = null;
        dialogueBoxInstance = null;

        player1Transform = GameManager.playerOne.transform;
        player2Transform = GameManager.playerTwo.transform;

        StartCoroutine(CheckElementToActivate());
    }

    // Update is called once per frame
    void Update()
    {
        middlePlayerPosition = (player1Transform.position + player2Transform.position) / 2;
    }

    public void SetNarrativeElementsInRange(bool _add, NarrativeInteractiveElements _narrativeElement)
    {
        if (_add)
        {
            narrativeElementsInRange.Add(_narrativeElement);
        }
        else
        {
            narrativeElementsInRange.Remove(_narrativeElement);
        }
    }

    public IEnumerator CheckElementToActivate()
    {
        if (narrativeElementsInRange.Count > 0)
        {
            float i_distance = 100;
            int i_narrativeElementToActivate = -1;
            for (int i = 0; i < narrativeElementsInRange.Count; i++)
            {
                if (Vector3.Distance(narrativeElementsInRange[i].transform.position, middlePlayerPosition) < i_distance)
                {
                    i_distance = Vector3.Distance(narrativeElementsInRange[i].transform.position, middlePlayerPosition);
                    i_narrativeElementToActivate = i;
                }
            }

            if(narrativeElementsInRange[i_narrativeElementToActivate] != activatedNarrativeElement)
            {
                if (activatedNarrativeElement != null)
                {
                    activatedNarrativeElement.SetAIPossession(false);
                }
                activatedNarrativeElement = narrativeElementsInRange[i_narrativeElementToActivate];
                activatedNarrativeElement.SetAIPossession(true);
            }
        }
        else if (activatedNarrativeElement != null)
        {
            activatedNarrativeElement.SetAIPossession(false);
            activatedNarrativeElement = null;
        }
        yield return new WaitForSeconds(timeBetweenRangeCheck);
        StartCoroutine(CheckElementToActivate());
    }

    public void LaunchDialogue(DialogueData _dialogueData)
    {
        dialogueBoxInstance = Instantiate(dialogueBoxPrefab, GameManager.mainCanvas.transform);
        textField = dialogueBoxInstance.GetComponentInChildren<TextMeshProUGUI>();
        myAudioSource.clip = _dialogueData.dialogueClip;
        myAudioSource.PlayOneShot(_dialogueData.dialogueClip);

        if (textWritingCoroutine == null)
        {
            textWritingCoroutine = NewWriteText_C(_dialogueData, typingSpeed);
            StartCoroutine(textWritingCoroutine);
        }
    }

    public void ChangeActivatedNarrationElement(NarrativeInteractiveElements _newNarrativeElement)
    {
        currentNarrationElementActivated = _newNarrativeElement;
        if(currentNarrationElementActivated != null)
        {
            myAudioSource.outputAudioMixerGroup = currentNarrationElementActivated.myAudioMixer;
        }
        else
        {
            myAudioSource.outputAudioMixerGroup = defaultAudioMixer;
        }
    }

    public IEnumerator NewWriteText_C(DialogueData _dialogueData, float _typingSpeed)
    {
        Color i_appearingColor = new Color(textField.color.r, textField.color.g, textField.color.b, 1);
        TMP_TextInfo i_textInfo = textField.textInfo;
        Color32[] i_newVertexColors;

        for (int i = 0; i < _dialogueData.texts.Length; i++) // Repeat for every text in dialogue data
        {
            textField.text = " " + _dialogueData.texts[i].text;   // The added " " is because the following method to write ignores the first character(problem with the vertices)

            for (int j = 0; j < _dialogueData.texts[i].text.Length + 1; j++) //The +1 is to fit the added " "
            {
                // Get the index of the material used by the current character.
                int materialIndex = i_textInfo.characterInfo[j].materialReferenceIndex;

                // Get the vertex colors of the mesh used by this text element (character or sprite).
                i_newVertexColors = i_textInfo.meshInfo[materialIndex].colors32;

                // Get the index of the first vertex used by this text element.
                int vertexIndex = i_textInfo.characterInfo[j].vertexIndex;

                // Only change the vertex color if the text element is visible.
                if (i_textInfo.characterInfo[j].isVisible)
                {
                    i_newVertexColors[vertexIndex + 0] = i_appearingColor;
                    i_newVertexColors[vertexIndex + 1] = i_appearingColor;
                    i_newVertexColors[vertexIndex + 2] = i_appearingColor;
                    i_newVertexColors[vertexIndex + 3] = i_appearingColor;

                    // New function which pushes (all) updated vertex data to the appropriate meshes when using either the Mesh Renderer or CanvasRenderer.
                    textField.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                }

                yield return new WaitForSeconds(1 / _typingSpeed);
            }

            yield return new WaitForSeconds(_dialogueData.texts[i].pauseAfterText);
            textWritingCoroutine = null;
        }
        Destroy(dialogueBoxInstance, _dialogueData.timeBeforeDestruction);
    }

    public IEnumerator WriteText_C(DialogueData _dialogueData, float _typingSpeed)
    {
        if (currentNarrationElementActivated != null)   // Check if IA's Screen is still active
        {
            for (int j = 0; j < _dialogueData.texts.Length; j++)
            {
                for (int i = 0; i < _dialogueData.texts[j].text.Length + 1; i++)
                {
                    textField.text = _dialogueData.texts[j].text.Substring(0, i);
                    yield return new WaitForSeconds(1 / _typingSpeed);
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
