using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleStop : MonoBehaviour, IHitable
{

    public PuzzleDatas puzzleData;
    private GameObject FX_StopBall;
    private int _hitCount;
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
        //Stop the ball
        _ball.ChangeSpeed(0);

        //Show an FX

        if (FX_StopBall != null)
        {
            Destroy(FX_StopBall);
        }
        FX_StopBall = FXManager.InstantiateFX(puzzleData.LinkStop, Vector3.zero, true, -_impactVector, Vector3.one * 5, transform);

        //Will desactivate all puzzle links
        PuzzleLink[] links = FindObjectsOfType<PuzzleLink>();
        foreach (var item in links)
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
