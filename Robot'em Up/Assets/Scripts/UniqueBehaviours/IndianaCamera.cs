using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndianaCamera : MonoBehaviour
{
    public PawnController firstPawn;
    public PawnController secondPawn;
    public float shakeAmount;

    public void Update()
    {
        Vector3 wantedPosition = new Vector3((firstPawn.transform.position.x + secondPawn.transform.position.x) / 2, 0, (firstPawn.transform.position.z + secondPawn.transform.position.z) / 2);
        transform.position = Vector3.Lerp(transform.position, wantedPosition, Time.deltaTime);
        if (shakeAmount > 0)
        {
            transform.position = transform.position + Random.insideUnitSphere * 0.75f;
            shakeAmount -= Time.deltaTime;
        }
        else
        {
            shakeAmount = 0.0f;
        }
    }
}