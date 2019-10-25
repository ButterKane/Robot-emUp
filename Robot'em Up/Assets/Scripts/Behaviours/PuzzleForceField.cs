using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PuzzleForceField : PuzzleActivable, IHitable
{
    private int _hitCount;
    public bool active = true;
    public bool alsoBlockPlayer = false;
    private MeshRenderer meshRenderer;
    private BoxCollider boxCollider;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        boxCollider = GetComponent<BoxCollider>();
        ChangeState(active, alsoBlockPlayer);
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

    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PlayerController _thrower, int _damages, DamageSource _source)
    {
        if (active)
        {
            _ball.ChangeSpeed(0);
        }
    }

    public void ChangeState (bool _Active = true, bool _alsoBlockPlayer = false)
    {
        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider>();
        }
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        active = _Active;
        alsoBlockPlayer = _alsoBlockPlayer;
        if (alsoBlockPlayer)
        {
            if (active)
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
            if (active)
            {
                meshRenderer.material = puzzleData.M_Forcefield_Active;
            }
            else
            {
                meshRenderer.material = puzzleData.M_Forcefield_Desactivated;

            }
        }
    }


    override public void WhenDesactivate()
    {
        ChangeState(false, alsoBlockPlayer);
    }
    
    override public void WhenActivate()
    {
        ChangeState(true, alsoBlockPlayer);
    }



}
