using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PuzzleForceField : PuzzleActivable, IHitable
{
    private int _hitCount;
    public bool alsoBlockPlayer = false;
    private MeshRenderer meshRenderer;
    private BoxCollider boxCollider;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        boxCollider = GetComponent<BoxCollider>();
        ChangeState(isActivated, alsoBlockPlayer);
    }

    public int hitCount
    {
        get
        {
            return _hitCount;
        }
        set
        {
            _hitCount = value;
        }
    }

    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source)
    {
        if (isActivated)
        {
            _ball.ChangeSpeed(0);
        }
    }

    public void ChangeState (bool _alsoBlockPlayer = false)
    {
        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider>();
        }
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        alsoBlockPlayer = _alsoBlockPlayer;
        if (alsoBlockPlayer)
        {
            if (isActivated)
            {
                boxCollider.isTrigger = false;
                meshRenderer.material = puzzleData.M_ForcefieldPlayers_Active;
            }
            else
            {
                boxCollider.isTrigger = true;
                meshRenderer.material = puzzleData.M_ForcefieldPlayers_Desactivated;

            }
        }
        else
        {
            boxCollider.isTrigger = true;
            if (isActivated)
            {
                meshRenderer.material = puzzleData.M_Forcefield_Active;
            }
            else
            {
                meshRenderer.material = puzzleData.M_Forcefield_Desactivated;

            }
        }
    }


    public override void WhenDesactivate()
    {
        isActivated = false;
        ChangeState(alsoBlockPlayer);
    }
    
    public override void WhenActivate()
    {
        isActivated = true;
        ChangeState(alsoBlockPlayer);
    }



}
