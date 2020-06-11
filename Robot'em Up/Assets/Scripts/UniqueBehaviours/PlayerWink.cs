using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWink : MonoBehaviour
{
    public Renderer myRend;
    public float minTimeBetweenWinks;
    public float maxTimeBetweenWinks;
    public float timeToWink;
    public AnimationCurve winkCurve;

    // Update is called once per frame
    void Start()
    {
        StartCoroutine(WinkCoroutine());
    }

    IEnumerator WinkCoroutine()
    {
        float i_winkTimer = 0;
        while (i_winkTimer < 1)
        {
            i_winkTimer += Time.deltaTime / timeToWink;
            myRend.material.SetFloat("_Clignement", winkCurve.Evaluate(Mathf.Clamp01(i_winkTimer)));
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(Random.Range(minTimeBetweenWinks, maxTimeBetweenWinks));
        StartCoroutine(WinkCoroutine());
    }
}
