using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleForceField : MonoBehaviour, IHitable
{
    private int _hitCount;
    public PuzzleDatas puzzleData;
    public PuzzleLink LinkedPuzzleLink;
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
        active = _Active;
        alsoBlockPlayer = _alsoBlockPlayer;
        if (alsoBlockPlayer)
        {
            boxCollider.isTrigger = false;
            if (active)
            {
                meshRenderer.material = puzzleData.M_ForcefieldPlayers_Active;
            }
            else
            {
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
    
}
