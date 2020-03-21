﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleLink : PuzzleActivator, IHitable
{


    [Header("Puzzle Link")]
    [Range(0.3f, 20)] public float nbSecondsLinkMaintained = 8f;
    public MeshRenderer CompletionShader;
    public Animator myAnim;

    private GameObject fX_Activation;
    private GameObject fX_Linked;
    private GameObject fX_LinkEnd;
    [SerializeField] private bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
    [SerializeField] private float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }
    [SerializeField] private Vector3 lockSize3DModifier = Vector3.one; public Vector3 lockSize3DModifier_access { get { return lockSize3DModifier; } set { lockSize3DModifier = value; } }


    public float chargingTime;


    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, float _damages, DamageSource _source, Vector3 _bumpModificators = default(Vector3))
    {
        if ((_source == DamageSource.Ball | _source == DamageSource.Dunk) & !shutDown)
        {
            if (MomentumManager.GetMomentum() >= puzzleData.nbMomentumNeededToLink)
            {

                if (fX_Linked != null)
                {
                    Destroy(fX_Linked);
                }

                if (fX_LinkEnd != null)
                {
                    Destroy(fX_LinkEnd);
                }

                fX_Linked = FeedbackManager.SendFeedback("event.PuzzleLinkActivated", this, transform.position, _impactVector, _impactVector).GetVFX();

                if (fX_Activation == null)
                {
                    fX_Activation = FeedbackManager.SendFeedback("event.PuzzleLinkActivation", this, transform.position, _impactVector, _impactVector).GetVFX();
                }
                MomentumManager.DecreaseMomentum(puzzleData.nbMomentumLooseWhenLink);
                chargingTime = nbSecondsLinkMaintained;
                isActivated = true;
                myAnim.SetTrigger("ActivatingTrigger");

                ActivateLinkedObjects();
            }
        }
    }

    void Awake()
    {
        chargingTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (chargingTime > 0 && isActivated)
        {
            CompletionShader.material.SetFloat("_AddToCompleteCircle", chargingTime / nbSecondsLinkMaintained);
            chargingTime -= Time.deltaTime;
        }

        if (chargingTime <= 0 && isActivated)
        {
            isActivated = false;
            fX_LinkEnd = FeedbackManager.SendFeedback("event.PuzzleLinkDesactivation", this).GetVFX();
            if (fX_Activation != null)
            {
                Destroy(fX_Activation);
            }
            if (fX_Linked != null)
            {
                Destroy(fX_Linked);
            }
            DesactiveLinkedObjects();
        }
    }


    override public void customShutDown()
    {
        //transform.position = transform.position + Vector3.up * -0.5f;
        myAnim.SetTrigger("ClosingTrigger");
        CompletionShader.material.SetFloat("_AddToCompleteCircle", 0);
        isActivated = false;


        fX_LinkEnd = FeedbackManager.SendFeedback("event.PuzzleLinkDesactivation", this).GetVFX();
        if (fX_Activation != null)
        {
            Destroy(fX_Activation);
        }
        if (fX_Linked != null)
        {
            Destroy(fX_Linked);
        }
    }
}
