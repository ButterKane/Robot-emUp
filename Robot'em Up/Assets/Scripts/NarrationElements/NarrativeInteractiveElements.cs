using System.Collections;
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
    [SerializeField] protected bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
    [SerializeField] protected float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }
    [SerializeField] private Vector3 lockSize3DModifier = Vector3.one; public Vector3 lockSize3DModifier_access { get { return lockSize3DModifier; } set { lockSize3DModifier = value; } }
    public string breakEventName;
    
    //Possession variables
    public float possessionAnimationMaxTime;
    public Color possessionColor;
    public AnimationCurve possessionAnimationCurve;
    [HideInInspector] public float possessionAnimationTimer;
    public Renderer[] possessionEmissives;
    public float maxEmissiveIntensity;
    public float normalEmissiveIntensity;

    public AudioMixerGroup myAudioMixer;

    public virtual void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, float _damages, DamageSource _source, Vector3 _bumpModificators = default)
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
        lockable = false;
        if(possessed)
            SetAIPossession(false);
        FeedbackManager.SendFeedback(breakEventName, this);
        if (NarrationManager.narrationManager.currentNarrationElementActivated == this)
        {
            NarrationManager.narrationManager.ChangeActivatedNarrationElement(null);
        }
    }

    public virtual void SetAIPossession(bool _isPossessed)
    {
        if (_isPossessed && !broken)
        {
            possessed = true;
            possessionAnimationTimer = 0;
            StartCoroutine(PossessionCoroutine(true));
        }
        else
        {
            possessed = false;
            if (!broken) {
                possessionAnimationTimer = 0;
                StartCoroutine(PossessionCoroutine(false));
            }
            else //sauter l'étape du sinus d'emissive
            {
                EndPossessionAnimationEvents();
            }
        }
    }

    public virtual IEnumerator PossessionCoroutine(bool _isPossessed)
    {
        if (_isPossessed) //IS POSSESSED
        {
            possessionAnimationTimer += Time.deltaTime;
            for (int i = 0; i < possessionEmissives.Length; i++)
            {
                possessionEmissives[i].material.SetColor("_EmissionColor", possessionColor * Mathf.Lerp(0, maxEmissiveIntensity, possessionAnimationCurve.Evaluate(possessionAnimationTimer / possessionAnimationMaxTime)));
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
            for (int i = 0; i < possessionEmissives.Length; i++)
            {
                possessionEmissives[i].material.SetColor("_EmissionColor", possessionColor * Mathf.Lerp(normalEmissiveIntensity, maxEmissiveIntensity, possessionAnimationCurve.Evaluate(possessionAnimationTimer / possessionAnimationMaxTime)));
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
        if (possessed && !broken)
        {
            for (int i = 0; i < possessionEmissives.Length; i++)
            {
                possessionEmissives[i].material.SetColor("_EmissionColor", possessionColor * normalEmissiveIntensity);
            }
            NarrationManager.narrationManager.ChangeActivatedNarrationElement(this);
        }
        else
        {
            for (int i = 0; i < possessionEmissives.Length; i++)
            {
                possessionEmissives[i].material.SetColor("_EmissionColor", Color.black);
            }

            if (NarrationManager.narrationManager.currentNarrationElementActivated == this)
            {
                NarrationManager.narrationManager.ChangeActivatedNarrationElement(null);
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
