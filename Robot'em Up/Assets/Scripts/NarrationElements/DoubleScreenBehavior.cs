using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleScreenBehavior : NarrativeInteractiveElements
{
    public Transform[] selves;
    public Transform[] leftEyeTransform;
    public Transform[] rightEyeTransform;
    public Transform[] topEyeTransform;
    public float maxEyeOffset;
    Vector3 eyesWantedPosition;

    public Renderer[] myRend;
    public Material possessedMat;
    public Material brokenScreenMat;
    public Material notPossessedMat;
    public float eyeLerpIntensity;



    // Update is called once per frame
    void Update()
    {
        if (!broken)
        {
            UpdateEyeLocation();

            if (angry)
            {
                AngryMatModification();
            }
        }
    }

    public override void EndPossessionAnimationEvents()
    {
        base.EndPossessionAnimationEvents();
        if (possessed && !broken)
        {
            myRend[0].material = possessedMat;
            myRend[1].material = possessedMat;

            topEyeTransform[0].gameObject.SetActive(true);
            topEyeTransform[1].gameObject.SetActive(true);
            
            leftEyeTransform[0].gameObject.SetActive(true);
            leftEyeTransform[1].gameObject.SetActive(true);
            
            rightEyeTransform[0].gameObject.SetActive(true);
            rightEyeTransform[1].gameObject.SetActive(true);
        }
        else if (!broken)
        {
            myRend[0].material = notPossessedMat;
            myRend[1].material = notPossessedMat;

            topEyeTransform[0].gameObject.SetActive(false);
            topEyeTransform[1].gameObject.SetActive(false);
            
            leftEyeTransform[0].gameObject.SetActive(false);
            leftEyeTransform[1].gameObject.SetActive(false);
            
            rightEyeTransform[0].gameObject.SetActive(false);
            rightEyeTransform[1].gameObject.SetActive(false);
        }
    }

    void AngryMatModification()
    {
        myRend[0].material.SetFloat("_SinFrequency", 40);
        myRend[0].material.SetFloat("_AdditiveEmissive", 2.5f);
        myRend[0].material.SetColor("_Noise1PannerSpeed", new Color(4, 8, 0, 0));
        myRend[0].material.SetColor("_Noise2PannerSpeed", new Color(-10, 7, 0, 0));
        myRend[0].material.SetFloat("_RedEmissiveIntensity", Random.Range(1f, 1.2f));

        myRend[1].material.SetFloat("_SinFrequency", 40);
        myRend[1].material.SetFloat("_AdditiveEmissive", 2.5f);
        myRend[1].material.SetColor("_Noise1PannerSpeed", new Color(4, 8, 0, 0));
        myRend[1].material.SetColor("_Noise2PannerSpeed", new Color(-10, 7, 0, 0));
        myRend[1].material.SetFloat("_RedEmissiveIntensity", Random.Range(1f, 1.2f));


    }

    public void SetAngryState(bool _isAngry)
    {
        if (_isAngry)
        {
            angry = true;
        }
        else //reset default values
        {
            angry = false;

            myRend[0].material.SetFloat("_SinFrequency", 5);
            myRend[0].material.SetFloat("_AdditiveEmissive", 0.77f);
            myRend[0].material.SetColor("_Noise1PannerSpeed", new Color(.1f, .1f, 0, 0));
            myRend[0].material.SetColor("_Noise2PannerSpeed", new Color(.15f, .15f, 0, 0));
            myRend[0].material.SetFloat("_RedEmissiveIntensity", 0);

            myRend[1].material.SetFloat("_SinFrequency", 5);
            myRend[1].material.SetFloat("_AdditiveEmissive", 0.77f);
            myRend[1].material.SetColor("_Noise1PannerSpeed", new Color(.1f, .1f, 0, 0));
            myRend[1].material.SetColor("_Noise2PannerSpeed", new Color(.15f, .15f, 0, 0));
            myRend[1].material.SetFloat("_RedEmissiveIntensity", 0);
        }
    }

    float RemapValue(float value, float min1, float max1, float min2, float max2)
    {
        return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
    }

    void UpdateEyeLocation()
    {
        Vector3 vectorToPlayer = GetCloserPlayer().position - selves[0].position;
        vectorToPlayer.y = 0;

        float angleLeft = Vector3.Angle(selves[0].right, vectorToPlayer);
        float angleForward = Vector3.Angle(selves[0].forward, vectorToPlayer);
        float angleRight = Vector3.Angle(-selves[0].right, vectorToPlayer);

        if (angleForward < 100)
        {
            eyesWantedPosition = new Vector3(0, 0, 0);
        }
        else
        {
            float valueToRemap = (angleRight + angleForward) / angleForward;
            eyesWantedPosition = Vector3.Lerp(new Vector3(-maxEyeOffset, 0, 0), new Vector3(maxEyeOffset, 0, 0), RemapValue(valueToRemap, 1.10f, 2f, 0, 1));
        }

        leftEyeTransform[0].localPosition = Vector3.Lerp(leftEyeTransform[0].localPosition, eyesWantedPosition, eyeLerpIntensity);
        rightEyeTransform[0].localPosition = Vector3.Lerp(rightEyeTransform[0].localPosition, eyesWantedPosition, eyeLerpIntensity);
        topEyeTransform[0].localPosition = Vector3.Lerp(topEyeTransform[0].localPosition, eyesWantedPosition, eyeLerpIntensity);



        vectorToPlayer = GetCloserPlayer().position - selves[1].position;
        vectorToPlayer.y = 0;

        angleLeft = Vector3.Angle(selves[1].right, vectorToPlayer);
        angleForward = Vector3.Angle(selves[1].forward, vectorToPlayer);
        angleRight = Vector3.Angle(-selves[1].right, vectorToPlayer);

        if (angleForward < 100)
        {
            eyesWantedPosition = new Vector3(0, 0, 0);
        }
        else
        {
            float valueToRemap = (angleRight + angleForward) / angleForward;
            eyesWantedPosition = Vector3.Lerp(new Vector3(-maxEyeOffset, 0, 0), new Vector3(maxEyeOffset, 0, 0), RemapValue(valueToRemap, 1.10f, 2f, 0, 1));
        }

        leftEyeTransform[1].localPosition = Vector3.Lerp(leftEyeTransform[1].localPosition, eyesWantedPosition, eyeLerpIntensity);
        rightEyeTransform[1].localPosition = Vector3.Lerp(rightEyeTransform[1].localPosition, eyesWantedPosition, eyeLerpIntensity);
        topEyeTransform[1].localPosition = Vector3.Lerp(topEyeTransform[1].localPosition, eyesWantedPosition, eyeLerpIntensity);
    }

    public override void Break()
    {
        base.Break();

        myRend[0].material = brokenScreenMat;
        leftEyeTransform[0].gameObject.SetActive(false);
        rightEyeTransform[0].gameObject.SetActive(false);
        leftEyeTransform[0].gameObject.SetActive(false);
        topEyeTransform[0].gameObject.SetActive(false);

        myRend[1].material = brokenScreenMat;
        leftEyeTransform[1].gameObject.SetActive(false);
        rightEyeTransform[1].gameObject.SetActive(false);
        leftEyeTransform[1].gameObject.SetActive(false);
        topEyeTransform[1].gameObject.SetActive(false);
    }
}
