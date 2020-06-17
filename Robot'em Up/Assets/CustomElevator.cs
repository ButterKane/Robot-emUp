using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomElevator : MonoBehaviour
{

    public float moveSpeed = 1;
    public float height = 10;

    private bool activated = false;
    public Transform visuals;
    public Transform emissiveVisuals;
    public AnimationCurve acceleration;
    public bool startOnAwake = true;

    private void Awake ()
    {
        emissiveVisuals.gameObject.SetActive(false);
        if (startOnAwake)
        {
            emissiveVisuals.gameObject.SetActive(true);
            Activate();
        }
    }
    public void EnableEmissive()
    {
        emissiveVisuals.gameObject.SetActive(true);
    }
    public void Activate()
    {
        if (!activated)
        {
            activated = true;
            StartCoroutine(Activate_C());
        }
    }

    IEnumerator Activate_C()
    {
        Vector3 startPosition = visuals.transform.position;
        Vector3 endPosition = startPosition + (Vector3.up * height);

        for (float i = 0; i < height; i+= Time.deltaTime * moveSpeed)
        {
            visuals.transform.position = Vector3.Lerp(startPosition, endPosition, acceleration.Evaluate(i / height));
            yield return null;
        }
        visuals.transform.position = endPosition;
    }
}
