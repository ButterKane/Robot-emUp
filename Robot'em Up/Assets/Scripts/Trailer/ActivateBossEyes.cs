using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateBossEyes : MonoBehaviour
{
    public float timeToGetToMaxIntensity;
    public AnimationCurve intensityCurve;
    public Renderer[] eyesRend;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            StartCoroutine(BossEyesOpen());
        }
    }

    IEnumerator BossEyesOpen()
    {
        float timer = 0;
        while (timer < 1)
        {
            timer += Time.deltaTime / timeToGetToMaxIntensity;
            for (int i = 0; i < eyesRend.Length; i++)
            {
                eyesRend[i].material.SetFloat("_Dissolve", intensityCurve.Evaluate(Mathf.Clamp01(timer)));
                eyesRend[i].material.SetFloat("_CircleSize", intensityCurve.Evaluate(Mathf.Clamp01(timer)));
            }
            yield return new WaitForEndOfFrame();
        }
    }
}
