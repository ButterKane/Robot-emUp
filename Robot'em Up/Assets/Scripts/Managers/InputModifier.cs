using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using XInputDotNetPure;

public class InputModifier : MonoBehaviour
{
    public SettingsInputGroup[] remappableInputs;

    public Dictionary<Inputs, UnityEvent> dicoP1Inputs;
    public Dictionary<Inputs, UnityEvent> dicoP2Inputs;

    private InputManager manager;

    private UnityEvent startButtonEvent;
    private UnityEvent backButtonEvent;
    private UnityEvent LeftJoystickEvent;
    private UnityEvent RightJoystickEvent;
    private UnityEvent RTEvent;
    private UnityEvent LTEvent;
    private UnityEvent LBEvent;
    private UnityEvent RBEvent;
    private UnityEvent AButtonEvent;
    private UnityEvent BButtonEvent;
    private UnityEvent YButtonEvent;
    private UnityEvent XButtonEvent;

    private bool waitForJoystickYReset;
    private bool waitForJoystickXReset;

    void NavigateInInputs()
    {
        GamePadState i_state = GamePad.GetState(PlayerIndex.One);
        if (i_state.Buttons.B == ButtonState.Pressed)
        {
            if (!CheckIfError())
            {
                FillInputDicos();
                AssignNewInputsInManager();
                ///Close input window
            }
        }
        if (i_state.Buttons.A == ButtonState.Pressed)
        {
            /// Change input
        }

        // Managing Up and Down
        if (i_state.ThumbSticks.Left.Y > 0.7f || i_state.DPad.Up == ButtonState.Pressed)
        {
            if (!waitForJoystickYReset)
            {
                /// Move Up
                waitForJoystickYReset = true;
            }
        }
        else if (i_state.ThumbSticks.Left.Y < -0.7f || i_state.DPad.Down == ButtonState.Pressed)
        {
            if (!waitForJoystickYReset)
            {
                /// Move Down
                waitForJoystickYReset = true;
            }

        }
        else
        {
            waitForJoystickYReset = false;
        }

        // Managing Left and Right
        if (i_state.ThumbSticks.Left.X > 0.7f || i_state.DPad.Right == ButtonState.Pressed)
        {
            if (!waitForJoystickXReset)
            {
                /// Move Right
                waitForJoystickXReset = true;
            }
        }
        else if (i_state.ThumbSticks.Left.X < -0.7f || i_state.DPad.Left == ButtonState.Pressed)
        {
            if (!waitForJoystickXReset)
            {
                /// Move Left
                waitForJoystickXReset = true;
            }
        }
        else
        {
            waitForJoystickXReset = false;
        }
    }

    public bool CheckIfError()
    {
        bool i_isInputNotSingle = false;
        for (int i = 0; i < remappableInputs.Length; i++)
        {
            for (int j = 0; j < remappableInputs.Length; j++)
            {
                if (i == j) { continue; }
                else
                {
                    if (remappableInputs[i].actionAndInputs.inputP1 == remappableInputs[j].actionAndInputs.inputP1)
                    {
                        // There is a duplicate
                        i_isInputNotSingle = true;
                    }

                    if (remappableInputs[i].actionAndInputs.inputP2 == remappableInputs[j].actionAndInputs.inputP2)
                    {
                        // There is a duplicate
                        i_isInputNotSingle = true;
                    }
                }
            }
        }

        return i_isInputNotSingle;
    }

    public void FillInputDicos()
    {
        dicoP1Inputs.Clear();
        dicoP2Inputs.Clear();
        foreach (var group in remappableInputs)
        {
            dicoP1Inputs.Add(group.actionAndInputs.inputP1, group.actionAndInputs.inputEvent);
            dicoP2Inputs.Add(group.actionAndInputs.inputP2, group.actionAndInputs.inputEvent);
        }
    }

    public void AssignNewInputsInManager()
    {
        InputManager.i.BButtonEvent = dicoP1Inputs[Inputs.BButton];
    }

    public void LaunchDash()
    {

    }

    public void LaunchDunk()
    {

    }

    public void LaunchThrowBall()
    {

    }

    public void LaunchGrapple()
    {

    }

    public void LaunchLocateBall()
    {

    }
}
