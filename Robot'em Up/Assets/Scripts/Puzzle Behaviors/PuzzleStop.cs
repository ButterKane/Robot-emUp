using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class PuzzleStop : MonoBehaviour, IHitable
{
	[SerializeField] private bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
	[SerializeField] private float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }

    [SerializeField] private Vector3 lockSize3DModifier = Vector3.one; public Vector3 lockSize3DModifier_access { get { return lockSize3DModifier; } set { lockSize3DModifier = value; } }

    public PuzzleDatas puzzleData;
    private GameObject fX_StopBall;

    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, float _damages, DamageSource _source, Vector3 _bumpModificators = default(Vector3))
    {
        //Stop the ball
        if (_ball != null)
        {
			Analytics.CustomEvent("BallStopped", new Dictionary<string, object> { { "Zone", GameManager.GetCurrentZoneName() }, });
			_ball.ChangeSpeed(0);
            fX_StopBall = FeedbackManager.SendFeedback("event.PuzzleBlockBall", this, _ball.transform.position, -_impactVector, _impactVector).GetVFX();
        }

        //Show an FX
        if (fX_StopBall != null)
        {
            Destroy(fX_StopBall);
        }

        //Will desactivate all puzzle links
        PuzzleLink[] i_links = FindObjectsOfType<PuzzleLink>();
        foreach (var item in i_links)
        {
            item.chargingTime = -1f;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
