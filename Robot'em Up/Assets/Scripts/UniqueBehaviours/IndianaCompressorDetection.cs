using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndianaCompressorDetection : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        gameObject.GetComponentInParent<IndianaCompressorRepeater>().DetectedTouch(other);
    }
}
