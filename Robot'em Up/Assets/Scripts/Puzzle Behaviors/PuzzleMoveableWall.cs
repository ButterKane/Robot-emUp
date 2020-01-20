using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class PuzzleMoveableWall : PuzzleActivable
{

    [ReadOnly] public Vector3 pos1;
    [ReadOnly] public Vector3 pos2;
    public Vector3 positionModifier;
    [Range(0, 40)]
    public float speed;
    public enum MoveableWallState { Pos1, Pos2, OneToTwo, TwoToOne }
    public MoveableWallState state;


    public bool stopImmediatly;
    private float journeyLength;
    private float startTime;
    private float fractionOfJourney;
    float distCovered;

    // Start is called before the first frame update
    void Awake()
    {
        startTime = Time.time;
        journeyLength = Vector3.Distance(pos1, pos2);
        RecalculatePositions();

    }

    // Update is called once per frame
    void Update()
    {
        if (state == MoveableWallState.OneToTwo)
        {
            distCovered = (Time.time - startTime) * speed;
            fractionOfJourney = distCovered / journeyLength;
            transform.position = Vector3.Lerp(pos1, pos2, fractionOfJourney);
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
            distCovered = (Time.time - startTime) * speed;
            fractionOfJourney = distCovered / journeyLength;
            transform.position = Vector3.Lerp(pos2, pos1, fractionOfJourney);
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
        if (stopImmediatly)
        {
            state = MoveableWallState.TwoToOne;
            startTime = Time.time - (1 - fractionOfJourney) * journeyLength / speed;
        }

        UpdateLights();
    }

//#if UNITY_EDITOR // conditional compilation is not mandatory
    [ButtonMethod]
    private void RecalculatePositions()
    {
        pos1 = transform.position;
        pos2 = transform.position + positionModifier;
    }
//#endif
}

