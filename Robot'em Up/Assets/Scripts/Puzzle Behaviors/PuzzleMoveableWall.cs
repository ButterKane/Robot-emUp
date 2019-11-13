using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class PuzzleMoveableWall : PuzzleActivable
{

    public Vector3 Pos1;
    public Vector3 Pos2;
    public Vector3 PositionModifier;
    [Range(0, 10)]
    public float speed;
    public enum MoveableWallState { Pos1, Pos2, OneToTwo, TwoToOne }
    public MoveableWallState state;


    private float journeyLength;
    private float startTime;

    // Start is called before the first frame update
    void Awake()
    {
        startTime = Time.time;
        journeyLength = Vector3.Distance(Pos1, Pos2);

    }

    // Update is called once per frame
    void Update()
    {
        if (state == MoveableWallState.OneToTwo)
        {
            float distCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distCovered / journeyLength;
            transform.position = Vector3.Lerp(Pos1, Pos2, fractionOfJourney);
            if (fractionOfJourney > 0.99f)
            {
                state = MoveableWallState.Pos2;
                if (!isActivated)
                {
                    state = MoveableWallState.TwoToOne;
                    startTime = Time.time;
                }
            }
        }


        if (state == MoveableWallState.TwoToOne)
        {
            float distCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distCovered / journeyLength;
            transform.position = Vector3.Lerp(Pos2, Pos1, fractionOfJourney);
            if (fractionOfJourney > 0.99f)
            {
                state = MoveableWallState.Pos1;
                if (isActivated)
                {
                    state = MoveableWallState.OneToTwo;
                    startTime = Time.time;
                }
            }
        }

    }



    override public void WhenActivate()
    {
        isActivated = true;
        if (state == MoveableWallState.Pos1)
        {
            state = MoveableWallState.OneToTwo;
            startTime = Time.time;

        }
        UpdateLights();
    }

    override public void WhenDesactivate()
    {
        isActivated = false;
        if (state == MoveableWallState.Pos2)
        {
            state = MoveableWallState.TwoToOne;
            startTime = Time.time;
        }

        UpdateLights();
    }

#if UNITY_EDITOR // conditional compilation is not mandatory
    [ButtonMethod]
    private void RecalculatePositions()
    {
        Pos1 = transform.position;
        Pos2 = transform.position + PositionModifier;
    }
#endif
}

