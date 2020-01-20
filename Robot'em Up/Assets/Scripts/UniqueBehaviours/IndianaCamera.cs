using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndianaCamera : MonoBehaviour
{
    public bool onRail;
    public Vector3 railPositionWanted;
    public PawnController firstPawn;
    public PawnController secondPawn;
    public float shakeAmount;

    public void Update()
    {
        if (onRail)
        {
            Vector3 i_playerPosition = new Vector3((firstPawn.transform.position.x + secondPawn.transform.position.x) / 2, 0, (firstPawn.transform.position.z + secondPawn.transform.position.z) / 2);
            Vector3 i_wantedPosition = (i_playerPosition + railPositionWanted) / 2;
            transform.position = Vector3.Lerp(transform.position, i_wantedPosition, Time.deltaTime);
        }
        else
        {
            Vector3 i_wantedPosition = new Vector3((firstPawn.transform.position.x + secondPawn.transform.position.x) / 2, 0, (firstPawn.transform.position.z + secondPawn.transform.position.z) / 2);
            transform.position = Vector3.Lerp(transform.position, i_wantedPosition, Time.deltaTime);
        }
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