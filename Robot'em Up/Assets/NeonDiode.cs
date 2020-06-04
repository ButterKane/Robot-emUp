using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeonDiode : MonoBehaviour
{
    public int diodeID;
    public bool startOnAwake = true;
    public float activationSpeed = 1f;
    public float startDelay = 0f;
    public float emissiveBoost = 1f;

    private Vector3 endPosition;
    private LineRenderer lr;
    private bool activated = false;
    private void Awake ()
    {
        lr = GetComponent<LineRenderer>();
        if (emissiveBoost != 1f)
        {
            lr.sharedMaterial = new Material(lr.sharedMaterial);
            lr.sharedMaterial.SetColor("_EmissionColor", lr.sharedMaterial.GetColor("_EmissionColor") * emissiveBoost);
        }
        DiodeManager.RegisterNeonController(this, diodeID);
        endPosition = lr.GetPosition(1);
        lr.SetPosition(1, Vector3.zero);
        if (startOnAwake)
        {
            Activate();
        }
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
        yield return new WaitForSeconds(startDelay);
        float length = endPosition.z;
        for (float i = 0; i < length; i+= Time.deltaTime * activationSpeed * 15f)
        {
            Vector3 secondPosition = Vector3.Lerp(Vector3.zero, endPosition, i / length);
            lr.SetPosition(1, secondPosition);
            yield return null;
        }
        lr.SetPosition(1, endPosition);
    }
}
