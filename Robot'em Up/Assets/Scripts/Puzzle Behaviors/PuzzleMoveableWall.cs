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

    void Awake()
    {
        startTime = Time.time;
		RecalculatePositions();
		journeyLength = Vector3.Distance(pos1, pos2);

    }

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



    override public void Activate()
    {
        isActivated = true;
        if (state == MoveableWallState.Pos1)
        {
            state = MoveableWallState.OneToTwo;
            startTime = Time.time;
        }
        UpdateLights();
    }

    override public void Desactivate()
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

    [ButtonMethod]
    private void RecalculatePositions()
    {
        pos1 = transform.position;
        pos2 = transform.position + positionModifier;
    }
}

