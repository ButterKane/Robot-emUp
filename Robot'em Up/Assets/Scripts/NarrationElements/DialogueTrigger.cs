using MyBox;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueData dialogueData;
    public GameObject dialogueBoxPrefab;
    public float typingSpeed = 5;
    private GameObject dialogueBoxInstance;
    private TextMeshProUGUI textField;
    private Image subImage;
    private IEnumerator textWritingCoroutine;

    private void Start()
    {
        textWritingCoroutine = null;
        dialogueBoxInstance = null;
    }


    public void LaunchDialogue()
    {
        if (textWritingCoroutine == null)
        {
            textWritingCoroutine = NewWriteText_C();
            StartCoroutine(textWritingCoroutine);
        }
    }

    public IEnumerator NewWriteText_C()
    {
        Color i_appearingColor = new Color(textField.color.r, textField.color.g, textField.color.b, 1);
        TMP_TextInfo i_textInfo = textField.textInfo;
        Color32[] i_newVertexColors;

        for (int i = 0; i < dialogueData.texts.Length; i++) // Repeat for every text in dialogue data
        {
            textField.text = " " + dialogueData.texts[i].text;   // The added " " is because the following method to write ignores the first character(problem with the vertices)

            for (int j = 0; j < dialogueData.texts[i].text.Length + 1; j++) //The +1 is to fit the added " "
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

                yield return new WaitForSeconds(1 / typingSpeed);
            }

            yield return new WaitForSeconds(dialogueData.texts[i].pauseAfterText);
            textWritingCoroutine = null;
        }
        Destroy(dialogueBoxInstance, dialogueData.timeBeforeDestruction);
        Destroy(gameObject, dialogueData.timeBeforeDestruction);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && dialogueBoxInstance == null)
        {
            dialogueBoxInstance = Instantiate(dialogueBoxPrefab, GameManager.mainCanvas.transform);
            textField = dialogueBoxInstance.transform.GetComponentInChildren<TextMeshProUGUI>();
            subImage = dialogueBoxInstance.transform.GetChild(0).GetComponent<Image>();
            StartCoroutine(BlinkSubImage());
            LaunchDialogue();
        }
    }

    public IEnumerator BlinkSubImage()
    {
        while (true)
        {
            subImage.enabled = !subImage.enabled;
            yield return new WaitForSeconds(0.5f);
        }
    }


    //Not called, just in case
    public void PlaySound(AudioClip _dialogueClip, Vector3 _position)
    {
        GameObject i_newSoundPlayer = new GameObject();
        i_newSoundPlayer.name = "SoundPlayer";
        AudioSource i_newAudioSource = i_newSoundPlayer.AddComponent<AudioSource>();
        i_newAudioSource.spatialBlend = 0.65f;
        i_newAudioSource.maxDistance = 100;
        i_newAudioSource.volume = 1;

        i_newAudioSource.gameObject.AddComponent<SoundAutoDestroyer>();
        i_newAudioSource.transform.position = _position;
        i_newAudioSource.clip = _dialogueClip;
        i_newAudioSource.Play();
    }

    public IEnumerator WriteText_C()
    {
        for (int j = 0; j < dialogueData.texts.Length; j++)
        {
            for (int i = 0; i < dialogueData.texts[j].text.Length + 1; i++)
            {
                textField.text = dialogueData.texts[j].text.Substring(0, i);
                yield return new WaitForSeconds(1 / typingSpeed);
            }

            yield return new WaitForSeconds(dialogueData.texts[j].pauseAfterText);
            textWritingCoroutine = null;
        }
        Destroy(dialogueBoxInstance, dialogueData.timeBeforeDestruction);
        Destroy(gameObject, dialogueData.timeBeforeDestruction);
    }
}
