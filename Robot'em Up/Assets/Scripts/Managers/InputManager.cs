using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
    public string inputName;
    public UnityEvent inputEvent;
    public Inputs p1Input;
    public Inputs p2Input;
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
    //private bool leftTriggerWaitForRelease; //Uncomment if needed (Commented to avoid errors)
    private bool leftShouldWaitForRelease;
    private bool rightShouldWaitForRelease;

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

    private void Awake()
    {
        cam = GameManager.mainCamera;
        if (i == null)
        {
            i = this;
        }
        else
        {
            Debug.LogError("There is already an InputManager");
        }
        //ApplyInputChanges();
    }

    public void ApplyInputChanges()
    {
        startButtonEvent = GetActionFromInput(mappedInputs, Inputs.StartButton, playerIndex);
        backButtonEvent = GetActionFromInput(mappedInputs, Inputs.BackButton, playerIndex);
        LeftJoystickEvent = GetActionFromInput(mappedInputs, Inputs.LeftJoystick, playerIndex);
        RightJoystickEvent = GetActionFromInput(mappedInputs, Inputs.RightJoystick, playerIndex);
        RTEvent = GetActionFromInput(mappedInputs, Inputs.RT, playerIndex);
        LTEvent = GetActionFromInput(mappedInputs, Inputs.LT, playerIndex);
        LBEvent = GetActionFromInput(mappedInputs, Inputs.LB, playerIndex);
        RBEvent = GetActionFromInput(mappedInputs, Inputs.RB, playerIndex);
        AButtonEvent = GetActionFromInput(mappedInputs, Inputs.AButton, playerIndex);
        BButtonEvent = GetActionFromInput(mappedInputs, Inputs.BButton, playerIndex);
        YButtonEvent = GetActionFromInput(mappedInputs, Inputs.YButton, playerIndex);
        XButtonEvent = GetActionFromInput(mappedInputs, Inputs.XButton, playerIndex);
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
            //leftTriggerWaitForRelease = true;
        }
        else
        {
            //leftTriggerWaitForRelease = false;
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
        startButtonEvent.Invoke();
    }

    private void BackButtonAction()
    {
        backButtonEvent.Invoke();
    }

    private void XButtonAction()
    {
        XButtonEvent.Invoke();
    }

    private void YButtonAction()
    {
        YButtonEvent.Invoke();
    }

    private void BButtonAction()
    {
        BButtonEvent.Invoke();
    }

    private void AButtonAction()
    {
        AButtonEvent.Invoke();
    }

    private void RBAction()
    {
        RBEvent.Invoke();
    }

    private void LBAction()
    {
        LBEvent.Invoke();
    }

    private void LTAction()
    {
        LTEvent.Invoke();
    }

    private void RTAction()
    {
        RTEvent.Invoke();
    }

    private void RightJoystickAction(Vector3 rightJoystickInput)
    {
        RightJoystickEvent.Invoke();
    }

    private void LeftJoystickAction(Vector3 leftJoystickInput)
    {
        LeftJoystickEvent.Invoke();
    }

    public static UnityEvent GetActionFromInput(BothInputs[] _dict, Inputs _input, PlayerIndex _playerIndex)
    {
        if (_playerIndex == PlayerIndex.One)
        {
            foreach (var bothInput in _dict)
            {

                if (bothInput.p1Input == _input)
                {
                    return bothInput.inputEvent;
                }
            }
        }
        else if (_playerIndex == PlayerIndex.Two)
        {
            foreach (var bothInput in _dict)
            {
                if (bothInput.p2Input == _input)
                {
                    return bothInput.inputEvent;
                }
            }
        }
        return null;
    }
}
