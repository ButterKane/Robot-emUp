using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class PuzzleElevator : PuzzleActivable
{

    public Vector3 downPosition;
    public Vector3 upPosition;
    [Range(0, 10)]
    public float speed;
    public enum ElevatorState { Down, Up, MovingDown, MovingUp }
    public ElevatorState state;

    private float journeyLength;
    private float startTime;

    // Start is called before the first frame update
    void Awake()
    {
        startTime = Time.time;
        journeyLength = Vector3.Distance(downPosition, upPosition);
    }

    // Update is called once per frame
    void Update()
    {
        if (state == ElevatorState.MovingUp)
        {
            float i_distCovered = (Time.time - startTime) * speed;
            float i_fractionOfJourney = i_distCovered / journeyLength;
            transform.position = Vector3.Lerp(downPosition, upPosition, i_fractionOfJourney);
            if (i_fractionOfJourney > 0.99f)
            {
                state = ElevatorState.Up;
                if (!isActivated)
                {
                    state = ElevatorState.MovingDown;
                    startTime = Time.time;
                }
            }
        }


        if (state == ElevatorState.MovingDown)
        {
            float i_distCovered = (Time.time - startTime) * speed;
            float i_fractionOfJourney = i_distCovered / journeyLength;
            transform.position = Vector3.Lerp(upPosition, downPosition, i_fractionOfJourney);
            if (i_fractionOfJourney > 0.99f)
            {
                state = ElevatorState.Down;
                if (isActivated)
                {
                    state = ElevatorState.MovingUp;
                    startTime = Time.time;
                }
            }
        }

    }

    override public void WhenActivate()
    {
        isActivated = true;
        if (state == ElevatorState.Down)
        {
            state = ElevatorState.MovingUp;
            startTime = Time.time;

        }
    }

    override public void WhenDesactivate()
    {
        isActivated = false;
        if (state == ElevatorState.Up)
        {
            state = ElevatorState.MovingDown;
            startTime = Time.time;
        }

    }

#if UNITY_EDITOR // conditional compilation is not mandatory
    [ButtonMethod]
    private void SetPositionsToCurrent()
    {
        downPosition = transform.position;
        upPosition = transform.position;
        upPosition = new Vector3(upPosition.x, upPosition.y + 2, upPosition.z);
    }
#endif
}
