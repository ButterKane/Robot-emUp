using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarrativeInteractiveElements : MonoBehaviour, IHitable
{
    [HideInInspector] public Transform player1Transform;
    [HideInInspector] public Transform player2Transform;
    [HideInInspector] public bool broken;
    [HideInInspector] public bool angry;
    [HideInInspector] public bool possessed;
    [SerializeField] protected bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
    [SerializeField] protected float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }
    
    //Possession variables
    public float possessionAnimationMaxTime;
    public AnimationCurve possessionAnimationCurve;
    [HideInInspector] public float possessionAnimationTimer;
    public Light possessionLight;
    public float maxLightIntensity;

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
    }

    public virtual void SetAIPossession(bool _isPossessed)
    {
        if (_isPossessed && !broken)
        {
            possessed = true;
            possessionAnimationTimer = 0;
            StartCoroutine(SetPossession(true));
        }
        else if (!broken)
        {
            possessed = false;
            possessionAnimationTimer = 0;
            StartCoroutine(SetPossession(false));
        }
    }

    public virtual IEnumerator SetPossession(bool _isPossessed)
    {
        if (_isPossessed) //IS POSSESSED
        {
            possessionAnimationTimer += Time.deltaTime;
            possessionLight.intensity = Mathf.Lerp(0, maxLightIntensity, possessionAnimationCurve.Evaluate(possessionAnimationTimer / possessionAnimationMaxTime));
            yield return new WaitForEndOfFrame();
            if (possessionAnimationTimer < possessionAnimationMaxTime)
            {
                StartCoroutine(SetPossession(true));
            }
            else
            {
                EndPossessionAnimationEvents();
            }
        }
        else // NOT POSSESSED
        {
            possessionAnimationTimer += Time.deltaTime;
            possessionLight.intensity = Mathf.Lerp(0, maxLightIntensity, possessionAnimationCurve.Evaluate(possessionAnimationTimer / possessionAnimationMaxTime));
            yield return new WaitForEndOfFrame();
            if (possessionAnimationTimer < possessionAnimationMaxTime)
            {
                StartCoroutine(SetPossession(false));
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

        }
        else
        {

        }
    }
}
