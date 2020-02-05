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
    [Range(0, 1)] public float boxOpacity = 0.3f;
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
            PlaySound(dialogueData.dialogueClip, transform.position);
            textWritingCoroutine = WriteText_C();
            StartCoroutine(textWritingCoroutine);
        }
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && dialogueBoxInstance == null)
        {
            dialogueBoxInstance = Instantiate(dialogueBoxPrefab, GameManager.mainCanvas.transform);
            dialogueBoxInstance.GetComponent<Image>().color = new Color(0, 0, 0, boxOpacity);
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
}
