using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomElevator : MonoBehaviour
{

    public float moveSpeed = 1;
    public float height = 10;

    private bool activated = false;

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
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + (Vector3.up * height);

        for (float i = 0; i < height; i+= Time.deltaTime * moveSpeed)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, i / height);
            yield return null;
        }
        transform.position = endPosition;
    }
}
