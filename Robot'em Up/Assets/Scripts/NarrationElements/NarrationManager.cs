﻿using MyBox;
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
    [ReadOnly] public float dialogueVolumeSettingsMod;
    public AudioMixerGroup defaultAudioMixer;
    public int textSize = 20;
    public Color textColor = Color.white;
    public TMP_FontAsset robotoTextFont;
    public TMP_FontAsset stylizedTextFont;
    [ReadOnly] public TMP_FontAsset selectedFont;


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
        selectedFont = robotoTextFont;
        narrationManager = this;
        CreateDialogueBox();
        textWritingCoroutine = null;
        dialogueBoxInstance = null;

        player1Transform = GameManager.playerOne.transform;
        player2Transform = GameManager.playerTwo.transform;
        ChangeTextColor(PlayerPrefs.GetInt("REU_In-Game Text Color", 0)); // initiation, either with saved value or with base value
        ChangeTextSize(PlayerPrefs.GetInt("REU_In-Game Text Size", 1));
        ChangeSelectedFont(SwissArmyKnife.ConvertPlayerPrefStringAsBool(PlayerPrefs.GetString("REU_Stylized In-Game Text Font")));
        StartCoroutine(CheckElementToActivate());
    }

    // Update is called once per frame
    void Update()
    {
        middlePlayerPosition = (player1Transform.position + player2Transform.position) / 2;
    }

    #region Public methods
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

    public void LaunchDialogue(DialogueData _dialogueData)
    {
        if (dialogueBoxInstance != null)
        {
            float i_normalAudioVolume = myAudioSource.volume;
            dialogueBoxInstance.SetActive(true);
            textField = dialogueBoxInstance.GetComponentInChildren<TextMeshProUGUI>();
            textField.color = textColor;
            textField.fontSize = textSize;
            textField.font = selectedFont;

            myAudioSource.volume = i_normalAudioVolume * PlayerPrefs.GetFloat("REU_Dialogue Volume", dialogueVolumeSettingsMod); 
            myAudioSource.clip = _dialogueData.dialogueClip;
            myAudioSource.PlayOneShot(_dialogueData.dialogueClip);

            if (textWritingCoroutine == null)
            {
                textWritingCoroutine = NewWriteText_C(_dialogueData, typingSpeed);
                StartCoroutine(textWritingCoroutine);
            }
        }
    }

    public void ChangeActivatedNarrationElement(NarrativeInteractiveElements _newNarrativeElement)
    {
        currentNarrationElementActivated = _newNarrativeElement;
        if (currentNarrationElementActivated != null)
        {
            myAudioSource.outputAudioMixerGroup = currentNarrationElementActivated.myAudioMixer;
        }
        else
        {
            myAudioSource.outputAudioMixerGroup = defaultAudioMixer;
        }
    }

    public void ChangeTextColor(int _newColorIndex)
    {
        switch (_newColorIndex)
        {
            case 0://white
                narrationManager.textColor = Color.white;
                break;
            case 1://yellow
                narrationManager.textColor = Color.yellow;
                break;
            case 2://blue
                narrationManager.textColor = Color.blue;
                break;
            case 3://green
                narrationManager.textColor = Color.green;
                break;
            case 4://red
                narrationManager.textColor = Color.red;
                break;
        }
    }

    public void ChangeTextSize(int _newTextSizeIndex)
    {
        switch (_newTextSizeIndex)
        {
            case 0://small
                narrationManager.textSize = 14;
                break;
            case 1://regular
                narrationManager.textSize = 20;
                break;
            case 2://big
                narrationManager.textSize = 30;
                break;
            case 3://very big
                narrationManager.textSize = 40;
                break;
        }
    }

    public void ChangeSelectedFont(bool _isFontStylized)
    {
        if (_isFontStylized)
            {
                narrationManager.selectedFont = narrationManager.stylizedTextFont;
            }
            else
            {
                narrationManager.selectedFont = narrationManager.robotoTextFont;
            }
        // Yes = Pas Roboto
        // No = Roboto
    }
    #endregion

    #region Private methods
    private void CreateDialogueBox()
    {
        dialogueBoxInstance = Instantiate(dialogueBoxPrefab, GameManager.mainCanvas.transform);
        dialogueBoxInstance.SetActive(false);
    }
    #endregion

    #region Coroutines
    public IEnumerator CheckElementToActivate()
    {
        if (narrativeElementsInRange.Count > 0)
        {
            float i_minimalDistance = 100;
            int i_narrativeElementToActivate = -1;
            for (int i = 0; i < narrativeElementsInRange.Count; i++)
            {
                float i_distanceFromElement = Vector3.Distance(narrativeElementsInRange[i].transform.position, middlePlayerPosition);
                if (i_distanceFromElement < i_minimalDistance)
                {
                    i_minimalDistance = i_distanceFromElement;
                    i_narrativeElementToActivate = i;
                }
            }

            if (narrativeElementsInRange[i_narrativeElementToActivate] != activatedNarrativeElement)
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

    private IEnumerator NewWriteText_C(DialogueData _dialogueData, float _typingSpeed)
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

        yield return new WaitForSeconds(_dialogueData.timeBeforeDestruction);
        dialogueBoxInstance.SetActive(false);
    }
    #endregion
}
