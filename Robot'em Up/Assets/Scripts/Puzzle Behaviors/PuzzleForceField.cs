using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PuzzleForceField : PuzzleActivable, IHitable
{
	[SerializeField] private bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
	[SerializeField] private float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }
    [SerializeField] private Vector3 lockSize3DModifier = Vector3.one; public Vector3 lockSize3DModifier_access { get { return lockSize3DModifier; } set { lockSize3DModifier = value; } }
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

    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, float _damages, DamageSource _source, Vector3 _bumpModificators = default(Vector3))
    {
        if (isActivated && ( type == typeForceField.blockTheBall | type == typeForceField.blockBallAndPlayer ))
        {
            _ball.ChangeSpeed(0);
        }

        if (isActivated && type == typeForceField.Flipper )
        {
            _ball.ResetBounceCount();
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
                    meshRenderer.material = puzzleData.m_forcefield_Active;
                }
                else
                {
                    meshRenderer.material = puzzleData.m_forcefield_Desactivated;
                }

                    break;
            case typeForceField.blockBallAndPlayer:
                if (isActivated)
                {
                    boxCollider.isTrigger = false;
                    meshRenderer.material = puzzleData.m_forcefieldPlayers_Active;
                }
                else
                {
                    boxCollider.isTrigger = true;
                    meshRenderer.material = puzzleData.m_forcefieldPlayers_Desactivated;

                }
                break;
            case typeForceField.Flipper:
                boxCollider.isTrigger = true;
                if (isActivated)
                {
                    meshRenderer.material = puzzleData.m_forcefield_Flipper_Active;
                }
                else
                {
                    meshRenderer.material = puzzleData.m_forcefield_Flipper_Desactivated;

                }
                break;
            default:
                break;
        }
        UpdateLights();
        
    }


    public override void WhenDesactivate()
    {
		FeedbackManager.SendFeedback("event.PuzzleForceFieldDesactivation", this);
        isActivated = false;
        ChangeState();
    }
    
    public override void WhenActivate()
    {
		FeedbackManager.SendFeedback("event.PuzzleForceFieldActivation", this);
        isActivated = true;
        ChangeState();
    }
}
