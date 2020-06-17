using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailerCameraMovingForward : MonoBehaviour
{
    public float speed;
    bool goingForward;
    public AnimationCurve shakeCurve;
    public float shakeIntensity;
    public float shakeFrequency;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            goingForward = true;
        }
        if (goingForward)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            CameraShaker.ShakeCamera(1, 3, 1, shakeCurve);
        }
    }
}
