using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableWall : MonoBehaviour, IHitable
{
    public bool onlyDestroyableByDunk = true;
    public GameObject[] destroyedVisuals;
    public GameObject visualsToHideWhenDestroyed;
    private bool destroyed;
    [SerializeField] private bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
    [SerializeField] private float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }
    [SerializeField] private Vector3 lockSize3DModifier = Vector3.one; public Vector3 lockSize3DModifier_access { get { return lockSize3DModifier; } set { lockSize3DModifier = value; } }

    public void OnHit ( BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, float _damages, DamageSource _source, Vector3 _bumpModificators = default )
    {
        if (!destroyed && _ball !=null)
        {
            if (!_ball.isGhostBall)
            {
                FeedbackManager.SendFeedback("event.DestrObjectHit", this, transform.position, _impactVector, _impactVector);
                DestroyTheObject();
            }
        }
    }

    public void DestroyTheObject ()
    {
        destroyed = true;
        FeedbackManager.SendFeedback("event.DestrObjectDeath", this, transform.position, transform.up, transform.up);
        visualsToHideWhenDestroyed.SetActive(false);
        for (int i = 0; i < destroyedVisuals.Length; i++)
        {
            destroyedVisuals[i].SetActive(true);
        }
    }
}
