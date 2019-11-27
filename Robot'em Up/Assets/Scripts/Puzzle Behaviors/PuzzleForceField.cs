using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PuzzleForceField : PuzzleActivable, IHitable
{
	[SerializeField] private bool _lockable; public bool lockable { get { return _lockable; } set { _lockable = value; } }
	[SerializeField] private float _lockHitboxSize; public float lockHitboxSize { get { return _lockHitboxSize; } set { _lockHitboxSize = value; } }
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

    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source)
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
        isActivated = false;
        ChangeState();
    }
    
    public override void WhenActivate()
    {
        isActivated = true;
        ChangeState();
    }



}
