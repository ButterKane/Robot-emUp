using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndianaCompressorDetection : MonoBehaviour
{
    private void OnTriggerEnter(Collider _other)
    {
        gameObject.GetComponentInParent<IndianaCompressorRepeater>().DetectedTouch(_other);
    }
}
