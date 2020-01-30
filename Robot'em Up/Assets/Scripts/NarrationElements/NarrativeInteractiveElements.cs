﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class NarrativeInteractiveElements : MonoBehaviour, IHitable
{
    [HideInInspector] public Transform player1Transform;
    [HideInInspector] public Transform player2Transform;
    [HideInInspector] public bool broken;
    [HideInInspector] public bool angry;
    [HideInInspector] public bool possessed;
    public AudioSource myAudioSource;
    [SerializeField] protected bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
    [SerializeField] protected float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }
    
    //Possession variables
    public float possessionAnimationMaxTime;
    public AnimationCurve possessionAnimationCurve;
    [HideInInspector] public float possessionAnimationTimer;
    public Light[] possessionLights;
    public float maxLightIntensity;
    public float normalLightIntensity;

    public virtual void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source, Vector3 _bumpModificators = default)
    {
        if (!broken)
        {
            Break();
        }
    }

    public virtual void Start()
    {
        player1Transform = GameManager.playerOne.transform;
        player2Transform = GameManager.playerTwo.transform;
    }

    public virtual void Break()
    {
        broken = true;
        if(possessed)
            SetAIPossession(false);
    }

    public virtual void SetAIPossession(bool _isPossessed)
    {
        if (_isPossessed)
        {
            possessed = true;
            possessionAnimationTimer = 0;
            StartCoroutine(PossessionCoroutine(true));
        }
        else
        {
            possessed = false;
            possessionAnimationTimer = 0;
            StartCoroutine(PossessionCoroutine(false));
        }
    }

    public virtual IEnumerator PossessionCoroutine(bool _isPossessed)
    {
        if (_isPossessed) //IS POSSESSED
        {
            possessionAnimationTimer += Time.deltaTime;
            for (int i = 0; i < possessionLights.Length; i++)
            {
                possessionLights[i].intensity = Mathf.Lerp(0, maxLightIntensity, possessionAnimationCurve.Evaluate(possessionAnimationTimer / possessionAnimationMaxTime));
            }
            yield return new WaitForEndOfFrame();
            if (possessionAnimationTimer < possessionAnimationMaxTime)
            {
                StartCoroutine(PossessionCoroutine(true));
            }
            else
            {
                EndPossessionAnimationEvents();
            }
        }
        else // NOT POSSESSED
        {
            possessionAnimationTimer += Time.deltaTime;
            for (int i = 0; i < possessionLights.Length; i++)
            {
                possessionLights[i].intensity = Mathf.Lerp(0, maxLightIntensity, possessionAnimationCurve.Evaluate(possessionAnimationTimer / possessionAnimationMaxTime));
            }
            yield return new WaitForEndOfFrame();
            if (possessionAnimationTimer < possessionAnimationMaxTime)
            {
                StartCoroutine(PossessionCoroutine(false));
            }
            else
            {
                EndPossessionAnimationEvents();
            }
        }
    }

    public virtual void EndPossessionAnimationEvents()
    {
        if (possessed)
        {
            for (int i = 0; i < possessionLights.Length; i++)
            {
                possessionLights[i].intensity = normalLightIntensity;
            }
        }
        else
        {
            for (int i = 0; i < possessionLights.Length; i++)
            {
                possessionLights[i].intensity = 0;
            }
        }
    }

    public virtual Transform GetCloserPlayer()
    {
        if (Vector3.Distance(transform.position, player1Transform.position) > Vector3.Distance(transform.position, player2Transform.position))
        {
            return player2Transform;
        }
        else
        {
            return player1Transform;
        }
    }
}
