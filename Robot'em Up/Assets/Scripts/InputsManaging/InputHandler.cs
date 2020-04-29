using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using XInputDotNetPure;

//[System.Serializable]
//public struct SingleAxis
//{
//    public string axisName;
//    public float axisValue => Input.GetAxis(axisName);
//}

//[System.Serializable]
//public struct AxisAction
//{
//    SingleAxis[] axis;
//}

[System.Serializable]
public struct SingleButton
{
    public CustomKeyCode key;
    [NonSerialized] public PlayerIndex index;
    public ButtonState? keyState => InputHandler.instance.GetButtonState(key,index);
}

[System.Serializable]
public struct SingleKeyedAxis
{
    public CustomAxisCode key;
    [NonSerialized] public PlayerIndex index;
    public float? axisState => InputHandler.instance.GetAxisState(key, index);
}

    [System.Serializable]
public class ButtonAction
{
    public SingleButton[] buttons; // All the buttons assigned to this action
    [NonSerialized] public bool isPressed;
    [NonSerialized] public bool isReleasable;

    public bool keyDown
    {
        get
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].keyState == ButtonState.Pressed && isPressed == false)
                { Debug.Log("pressingButton"); return true; }
            }
            return false;
        }
    }

    public bool keyUp
    {
        get
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].keyState == ButtonState.Released && isReleasable == true)
                { Debug.Log("releasingButton"); isReleasable = false; return true; }
            }
            return false;
        }
    }

    public bool key
    {
        get
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].keyState == ButtonState.Pressed)
                { Debug.Log("holdingButton"); isPressed = true; return true; }
            }
            return false;
        }
    }
}

[System.Serializable]
public class KeyBindingStruct
{
    //public AxisAction moveX, moveY, aimX, aimY;
    public ButtonAction dunk, throwBall, interact, grapple, detectBall;
}

public class InputHandler : MonoBehaviour
{
    public static InputHandler instance;
    
    public KeyBindingStruct bindingP1;
    public KeyBindingStruct bindingP2;

    void Awake()
    {
        if (instance == null) { instance = this; }
    }

    private void Update()
    {
        //For player1
        ResetUnusedButtons(bindingP1.dunk);
        ResetUnusedButtons(bindingP1.throwBall);
        ResetUnusedButtons(bindingP1.grapple);
        ResetUnusedButtons(bindingP1.detectBall);

        //For player2
        ResetUnusedButtons(bindingP2.dunk);
        ResetUnusedButtons(bindingP2.throwBall);
        ResetUnusedButtons(bindingP2.grapple);
        ResetUnusedButtons(bindingP2.detectBall);
    }

    // Makes transition between serialized keys in inspector and XInput 
    public ButtonState? GetButtonState(CustomKeyCode _ck, PlayerIndex _index)
    {
        GamePadState i_state = GamePad.GetState(_index);
        switch (_ck)
        {
            case CustomKeyCode.A:
                return i_state.Buttons.A;
            case CustomKeyCode.B:
                return i_state.Buttons.B;
            case CustomKeyCode.Y:
                return i_state.Buttons.Y;
            case CustomKeyCode.X:
                return i_state.Buttons.X;
            case CustomKeyCode.LB:
                return i_state.Buttons.LeftShoulder;
            case CustomKeyCode.RB:
                return i_state.Buttons.RightShoulder;
            case CustomKeyCode.PadUp:
                return i_state.DPad.Up;
            case CustomKeyCode.PadDown:
                return i_state.DPad.Down;
            case CustomKeyCode.PadLeft:
                return i_state.DPad.Left;
            case CustomKeyCode.PadRight:
                return i_state.DPad.Right;
            case CustomKeyCode.L3:
                return i_state.Buttons.LeftStick;
            case CustomKeyCode.R3:
                return i_state.Buttons.RightStick;
            case CustomKeyCode.Start:
                return i_state.Buttons.Start;
            case CustomKeyCode.Back:
                return i_state.Buttons.Back;
            
            default:
                return null;
        }
    }

    public float? GetAxisState(CustomAxisCode _ca, PlayerIndex _index)
    {
        GamePadState i_state = GamePad.GetState(_index);
        switch (_ca)
        {
            case CustomAxisCode.LT:
                return i_state.Triggers.Left;
            case CustomAxisCode.RT:
                return i_state.Triggers.Right;
            case CustomAxisCode.LeftJoystickX:
                return i_state.ThumbSticks.Left.X;
            case CustomAxisCode.LeftJoystickY:
                return i_state.ThumbSticks.Left.Y;
            case CustomAxisCode.RightJoytickX:
                return i_state.ThumbSticks.Right.X;
            case CustomAxisCode.RightJoystickY:
                return i_state.ThumbSticks.Right.Y;
            default:
                return null;
        }
    }

    private void ResetUnusedButtons(ButtonAction _buttonAction)
    {
        foreach(var button in _buttonAction.buttons)
        {
            if (button.keyState == ButtonState.Pressed)
            {
                _buttonAction.isPressed = true;
                _buttonAction.isReleasable = true;
            }
            else { _buttonAction.isPressed = false; }
        }
    }

    public void AssignNewInputToAction(ButtonAction _concernedAction, CustomKeyCode _newKey, int _whichInputIndex)
    {
        _concernedAction.buttons[_whichInputIndex].key = _newKey;
        // make a check on all others input to see if there are multiples
    }
}

// Allows transition between serializable keys in inspector and XInput 
public enum CustomKeyCode
{
    A,
    B,
    Y,
    X,
    LB,
    RB,
    PadUp,
    PadDown,
    PadLeft,
    PadRight,
    L3,
    R3,
    Start,
    Back
}

public enum CustomAxisCode
{
    LT,
    RT,
    LeftJoystickX,
    LeftJoystickY,
    RightJoytickX,
    RightJoystickY
}



