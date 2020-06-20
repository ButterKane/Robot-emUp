﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class ScreenBehavior : NarrativeInteractiveElements
{
    public enum AIScreenState { Activated, Desactivated, Broken }
    public Transform leftEyeTransform;
    public Transform rightEyeTransform;
    public Transform topEyeTransform;
    public float maxEyeOffset;
    private AIScreenState currentState = AIScreenState.Desactivated;
    Vector3 eyesWantedPosition;

    public Renderer myRend;
    public Material possessedMat;
    public Material brokenScreenMat;
    public Material notPossessedMat;
    public float eyeLerpIntensity;


    private void Awake ()
    {
        currentState = AIScreenState.Desactivated;
    }
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

    public override void SetAIPossession ( bool _isPossessed )
    {
        base.SetAIPossession(_isPossessed);
        if (_isPossessed)
        {
            ChangeState(AIScreenState.Activated);
        } else
        {
            ChangeState(AIScreenState.Desactivated);
        }
    }

    public void ChangeState(AIScreenState _state)
    {
        if (currentState == _state)
        {
            return;
        }
        switch (_state)
        {
            case AIScreenState.Activated:
                FeedbackManager.SendFeedback("event.ScreenActivation", this);
                break;
            case AIScreenState.Desactivated:
                FeedbackManager.SendFeedback("event.ScreenDesactivation", this);
                break;
            case AIScreenState.Broken:
                break;
        }
        Debug.Log("Changing state: " + _state);
        currentState = _state;
    }

    public override void EndPossessionAnimationEvents()
    {
        base.EndPossessionAnimationEvents();
        if (possessed && !broken)
        {
            myRend.material = possessedMat;
            topEyeTransform.gameObject.SetActive(true);
            leftEyeTransform.gameObject.SetActive(true);
            rightEyeTransform.gameObject.SetActive(true);
        }
        else if (!broken)
        {
            myRend.material = notPossessedMat;
            topEyeTransform.gameObject.SetActive(false);
            leftEyeTransform.gameObject.SetActive(false);
            rightEyeTransform.gameObject.SetActive(false);
        }
    }

    void AngryMatModification()
    {
        myRend.material.SetFloat("_SinFrequency", 40);
        myRend.material.SetFloat("_AdditiveEmissive", 2.5f);
        myRend.material.SetColor("_Noise1PannerSpeed", new Color(4, 8, 0, 0));
        myRend.material.SetColor("_Noise2PannerSpeed", new Color(-10, 7, 0, 0));
        myRend.material.SetFloat("_RedEmissiveIntensity", Random.Range(1f, 1.2f));
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
            myRend.material.SetFloat("_SinFrequency", 5);
            myRend.material.SetFloat("_AdditiveEmissive", 0.77f);
            myRend.material.SetColor("_Noise1PannerSpeed", new Color(.1f, .1f, 0, 0));
            myRend.material.SetColor("_Noise2PannerSpeed", new Color(.15f, .15f, 0, 0));
            myRend.material.SetFloat("_RedEmissiveIntensity", 0);
        }
    }

    float RemapValue(float value, float min1, float max1, float min2, float max2)
    {
        return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
    }

    void UpdateEyeLocation()
    {
        Vector3 vectorToPlayer = GetCloserPlayer().position - transform.position;
        vectorToPlayer.y = 0;

        float angleLeft = Vector3.Angle(transform.right, vectorToPlayer);
        float angleForward = Vector3.Angle(transform.forward, vectorToPlayer);
        float angleRight = Vector3.Angle(-transform.right, vectorToPlayer);

        if (angleForward < 100)
        {
            eyesWantedPosition = new Vector3(0, 0, 0);
        }
        else
        {
            float valueToRemap = (angleRight + angleForward) / angleForward;
            eyesWantedPosition = Vector3.Lerp(new Vector3(-maxEyeOffset, 0, 0), new Vector3(maxEyeOffset, 0, 0), RemapValue(valueToRemap, 1.10f, 2f, 0, 1));
        }

        leftEyeTransform.localPosition = Vector3.Lerp(leftEyeTransform.localPosition, eyesWantedPosition, eyeLerpIntensity);
        rightEyeTransform.localPosition = Vector3.Lerp(rightEyeTransform.localPosition, eyesWantedPosition, eyeLerpIntensity);
        topEyeTransform.localPosition = Vector3.Lerp(topEyeTransform.localPosition, eyesWantedPosition, eyeLerpIntensity);
    }

    public override void Break()
    {
        base.Break();
        myRend.material = brokenScreenMat;
        leftEyeTransform.gameObject.SetActive(false);
        rightEyeTransform.gameObject.SetActive(false);
        leftEyeTransform.gameObject.SetActive(false);
        topEyeTransform.gameObject.SetActive(false);
        ChangeState(AIScreenState.Broken);
    }
}
