using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public enum Inputs
{
    AButton,
    BButton,
    XButton,
    YButton,
    PadUp,
    PadDown,
    PadRight,
    PadLeft,
    LeftJoystick,
    RightJoystick,
    LB,
    RB,
    LT,
    RT,
    StartButton,
    BackButton
}

[System.Serializable]
public class BothInputs
{
    public string InputName;
    public Inputs P1Input;
    public Inputs P2Input;
}

public class InputManager : MonoBehaviour
{
    public static InputManager i;

    [Separator("Settings")]
    public bool inputDisabled;
    public float triggerTreshold = 0.1f;
    GamePadState state;
    private Camera cam;
    public PlayerIndex playerIndex;
    [ReadOnly] public Vector3 moveInput;
    public float deadzone = 0.2f;
    protected Vector3 lookInput;
    public BothInputs[] mappedInputs;

    private bool rightTriggerWaitForRelease;
    private bool leftShouldWaitForRelease;

    private void Awake()
    {
        if (i == null)
        {
            i = this;
        }
        else
        {
            Debug.LogError("There is already an InputManager");
        }

    }


    public void ApplyInputChanges()
    {

    }

    void GamepadInput()
    {
        state = GamePad.GetState(playerIndex);
        Vector3 camForwardNormalized = cam.transform.forward;
        camForwardNormalized.y = 0;
        camForwardNormalized = camForwardNormalized.normalized;
        Vector3 camRightNormalized = cam.transform.right;
        camRightNormalized.y = 0;
        camRightNormalized = camRightNormalized.normalized;
        moveInput = (state.ThumbSticks.Left.X * camRightNormalized) + (state.ThumbSticks.Left.Y * camForwardNormalized);
        moveInput.y = 0;
        moveInput = moveInput.normalized * ((moveInput.magnitude - deadzone) / (1 - deadzone));
        lookInput = (state.ThumbSticks.Right.X * camRightNormalized) + (state.ThumbSticks.Right.Y * camForwardNormalized);
        if (lookInput.magnitude > 0.1f)
        {
            //passController.Aim();
        }
        else
        {
            //passController.StopAim();
        }

        if (state.Triggers.Right > triggerTreshold)
        {
            //if (!rightTriggerWaitForRelease) { passController.TryReception(); passController.Shoot(); }
            //rightTriggerWaitForRelease = true;
        }
        if (state.Triggers.Right < triggerTreshold)
        {
            //rightTriggerWaitForRelease = false;
        }
        if (state.Buttons.LeftShoulder == ButtonState.Pressed && !leftShouldWaitForRelease)
        {
            Highlighter.HighlightBall();
            //Highlighter.HighlightObject(transform.Find("Model"), highlightedColor, highlightedSecondColor);
            leftShouldWaitForRelease = true;
        }
        if (state.Buttons.LeftShoulder == ButtonState.Released)
        {
            leftShouldWaitForRelease = false;
        }
        if (state.Buttons.Y == ButtonState.Pressed)
        {
            //dunkController.Dunk();
        }
        if (state.Triggers.Left > triggerTreshold)
        {

        }
        else
        {
            //dashPressed = false;
        }
        if (state.Buttons.A == ButtonState.Pressed)
        {

        }
        if (Mathf.Abs(state.ThumbSticks.Left.X) > 0.5f || Mathf.Abs(state.ThumbSticks.Left.Y) > 0.5f)
        {

        }
        if (state.Triggers.Right > triggerTreshold && state.Triggers.Left > triggerTreshold)
        {

        }
        else
        {

        }



    }
}
