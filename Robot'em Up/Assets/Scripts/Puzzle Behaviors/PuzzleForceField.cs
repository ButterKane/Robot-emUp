﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PuzzleForceField : PuzzleActivable, IHitable
{
	[SerializeField] private bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
	[SerializeField] private float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }
    public typeForceField type = typeForceField.blockTheBall;
    public enum typeForceField { blockTheBall, blockBallAndPlayer, Flipper }
    private MeshRenderer meshRenderer;
    private BoxCollider boxCollider;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        boxCollider = GetComponent<BoxCollider>();
        ChangeState();
    }

    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source, Vector3 _bumpModificators = default(Vector3))
    {
        if (isActivated && ( type == typeForceField.blockTheBall | type == typeForceField.blockBallAndPlayer ))
        {
            _ball.ChangeSpeed(0);
        }

        if (isActivated && type == typeForceField.Flipper )
        {
            _ball.ResetBounds();
            _ball.MultiplySpeed(2f);
        }
    }

    public void ChangeState()
    {
        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider>();
        }
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        switch (type)
        {
            case typeForceField.blockTheBall:
                boxCollider.isTrigger = true;
                if (isActivated)
                {
                    meshRenderer.material = puzzleData.M_Forcefield_Active;
                }
                else
                {
                    meshRenderer.material = puzzleData.M_Forcefield_Desactivated;
                }

                    break;
            case typeForceField.blockBallAndPlayer:
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
                break;
            case typeForceField.Flipper:
                boxCollider.isTrigger = true;
                if (isActivated)
                {
                    meshRenderer.material = puzzleData.M_Forcefield_Flipper_Active;
                }
                else
                {
                    meshRenderer.material = puzzleData.M_Forcefield_Flipper_Desactivated;

                }
                break;
            default:
                break;
        }
        UpdateLights();
        
    }


    public override void WhenDesactivate()
    {
        SoundManager.PlaySound("ForceFieldDesactivate", transform.position, transform);
        isActivated = false;
        ChangeState();
    }
    
    public override void WhenActivate()
    {
        SoundManager.PlaySound("ForceFieldActivate", transform.position, transform);
        isActivated = true;
        ChangeState();
    }
}
