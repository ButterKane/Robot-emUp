using MyBox;
using System;
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
    [ReadOnly] public Vector3 leftJoystickInput;
    public float deadzone = 0.2f;
    protected Vector3 rightJoystickInput;
    public BothInputs[] mappedInputs;

    private bool rightTriggerWaitForRelease;
    private bool leftTriggerWaitForRelease;
    private bool leftShouldWaitForRelease;
    private bool rightShouldWaitForRelease;

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
        leftJoystickInput = (state.ThumbSticks.Left.X * camRightNormalized) + (state.ThumbSticks.Left.Y * camForwardNormalized);
        leftJoystickInput.y = 0;
        leftJoystickInput = leftJoystickInput.normalized * ((leftJoystickInput.magnitude - deadzone) / (1 - deadzone));

        rightJoystickInput = (state.ThumbSticks.Right.X * camRightNormalized) + (state.ThumbSticks.Right.Y * camForwardNormalized);

        // JOYSTICKS
        if (leftJoystickInput.magnitude > 0.1f)
        {
            LeftJoystickAction(leftJoystickInput);
        }
        if (rightJoystickInput.magnitude > 0.1f)
        {
            RightJoystickAction(rightJoystickInput);
        }

        // TRIGGERS
        if (state.Triggers.Right > triggerTreshold)
        {
            if (!rightTriggerWaitForRelease) { RTAction();}
            rightTriggerWaitForRelease = true;
        }
        else if (state.Triggers.Right < triggerTreshold)
        {
            rightTriggerWaitForRelease = false;
        }
        if (state.Triggers.Left > triggerTreshold)
        {
            if (!rightTriggerWaitForRelease) { LTAction(); }
            leftTriggerWaitForRelease = true;
        }
        else
        {
            leftTriggerWaitForRelease = false;
        }

        // BUMPERS
        if (state.Buttons.LeftShoulder == ButtonState.Pressed && !leftShouldWaitForRelease)
        {
            LBAction();
            leftShouldWaitForRelease = true;
        }
        else if (state.Buttons.LeftShoulder == ButtonState.Released)
        {
            leftShouldWaitForRelease = false;
        }

        if (state.Buttons.RightShoulder == ButtonState.Pressed && !rightShouldWaitForRelease)
        {
            RBAction();
            rightShouldWaitForRelease = true;
        }
        else if (state.Buttons.RightShoulder == ButtonState.Released)
        {
            rightShouldWaitForRelease = false;
        }

        // BUTTONS
        if (state.Buttons.A == ButtonState.Pressed)
        {
            AButtonAction();
        }
        if (state.Buttons.B == ButtonState.Pressed)
        {
            BButtonAction();
        }
        if (state.Buttons.Y == ButtonState.Pressed)
        {
            YButtonAction();
        }
        if (state.Buttons.X == ButtonState.Pressed)
        {
            XButtonAction();
        }
        if (state.Buttons.Back == ButtonState.Pressed)
        {
            BackButtonAction();
        }
        if (state.Buttons.Start == ButtonState.Pressed)
        {
            StartButtonAction();
        }
    }

    private void StartButtonAction()
    {
        throw new NotImplementedException();
    }

    private void BackButtonAction()
    {
        throw new NotImplementedException();
    }

    private void XButtonAction()
    {
        throw new NotImplementedException();
    }

    private void YButtonAction()
    {
        throw new NotImplementedException();
    }

    private void BButtonAction()
    {
        throw new NotImplementedException();
    }

    private void AButtonAction()
    {
        throw new NotImplementedException();
    }

    private void RBAction()
    {
        throw new NotImplementedException();
    }

    private void LBAction()
    {
        throw new NotImplementedException();
    }

    private void LTAction()
    {
        throw new NotImplementedException();
    }

    private void RTAction()
    {
        throw new NotImplementedException();
    }

    private void RightJoystickAction(Vector3 rightJoystickInput)
    {
        throw new NotImplementedException();
    }

    private void LeftJoystickAction(Vector3 leftJoystickInput)
    {
        throw new NotImplementedException();
    }
}
