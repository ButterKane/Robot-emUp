using System;
using System.Collections;
using System.Collections.Generic;
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
    public ButtonState? keyState => InputHandler.instance.GetState(key,index);
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
public struct KeyBindingStruct
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
    public ButtonState? GetState(CustomKeyCode _ck, PlayerIndex _index)
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
            case CustomKeyCode.Start:
                return i_state.Buttons.Start;
            case CustomKeyCode.Back:
                return i_state.Buttons.Back;
            default:
                return null;
        }
    }

    public void ResetUnusedButtons(ButtonAction _buttonAction)
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
    Start,
    Back
}



