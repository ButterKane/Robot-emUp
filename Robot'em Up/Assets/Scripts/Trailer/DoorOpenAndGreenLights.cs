using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpenAndGreenLights : MonoBehaviour
{
    public ArenaDoor arenaDoor;
    public Renderer[] ledRends;
    public float delayBetweenLedsAndOpen;
    public float delayBetweenLeds;
    public float greenLightIntensity;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            StartCoroutine(OpenDoor());
        }
    }

    IEnumerator OpenDoor()
    {
        for (int i = 0; i < ledRends.Length; i++)
        {
            ledRends[i].material.SetColor("_EmissionColor", Color.green * greenLightIntensity);
            yield return new WaitForSeconds(delayBetweenLeds);
        }
        yield return new WaitForSeconds(delayBetweenLedsAndOpen-delayBetweenLeds);
        arenaDoor.OpenDoor();
    }
}
