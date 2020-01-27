using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleStop : MonoBehaviour, IHitable
{
	[SerializeField] private bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
	[SerializeField] private float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }

	public PuzzleDatas puzzleData;
    private GameObject fX_StopBall;

    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source, Vector3 _bumpModificators = default(Vector3))
    {
        //Stop the ball
        _ball.ChangeSpeed(0);

        //Show an FX
        if (fX_StopBall != null)
        {
            Destroy(fX_StopBall);
        }
		fX_StopBall = FeedbackManager.SendFeedback("event.PuzzleBlockBall", this, _ball.transform.position, -_impactVector, _impactVector).GetVFX();

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
