using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using XInputDotNetPure;


[System.Serializable]
public struct SingleButton
{
    public CustomKeyCode key;
    public PlayerIndex index;
    public ButtonState? keyState => InputHandler.instance.GetButtonState(key,index);
}

[System.Serializable]
public struct SingleAxis
{
    public CustomAxisCode key;
    public PlayerIndex index;
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
            int i_inputToValidate = 0;
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].keyState == ButtonState.Pressed && isPressed == false)
                { i_inputToValidate++; }
            }
            if(i_inputToValidate == buttons.Length) { return true; }
            return false;
        }
    }

    public bool keyUp
    {
        get
        {
            int i_inputToValidate = 0;
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].keyState == ButtonState.Released && isReleasable == true)
                { i_inputToValidate++; }
            }
            if (i_inputToValidate == buttons.Length) { return true; }
            return false;
        }
    }

    public bool key
    {
        get
        {
            int i_inputToValidate = 0;
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].keyState == ButtonState.Pressed)
                { i_inputToValidate++; }
            }
            if (i_inputToValidate == buttons.Length) { return true; }
            return false;
        }
    }
}

[System.Serializable]
public class AxisAction
{
    public SingleAxis[] axis;
    // The float value of the axis
    public float axisValue
    {
        get
        {
            for (int i = 0; i < axis.Length; i++)
            {
                if (axis[i].axisState == null) { return 0; } else { return (float)axis[i].axisState; }
            }
            return 0;
        }
    }
    // Are we at the positive extreme value of the axis
    public bool axisPlus
    {
        get
        {
            for (int i = 0; i < axis.Length; i++)
            {
                if (axis[i].axisState == null) { continue; }
                else if ((float)axis[i].axisState == 1) { return true; }
            }
            return false;
        }
    }
    // Are we at the negative extreme value of the axis
    public bool axisMinus
    {
        get
        {
            for (int i = 0; i < axis.Length; i++)
            {
                if (axis[i].axisState == null) { continue; }
                else if ((float)axis[i].axisState == -1) { return true; }
            }
            return false;
        }
    }
}

[System.Serializable]
public class KeyBindingStruct
{
    public AxisAction moveX, moveY, aimX, aimY;
    public ButtonAction dunk, dash, throwBall, interact, grapple, detectBall;

    // Shortcuts
    public Vector2 moveStickVector => new Vector2(moveX.axisValue, moveY.axisValue);
    public Vector2 aimStickVector => new Vector2(aimX.axisValue, aimY.axisValue);
}

public class InputHandler : MonoBehaviour
{
    public static InputHandler instance;
    
    public KeyBindingStruct bindingP1;
    public KeyBindingStruct bindingP2;

    void Awake()
    {
        if (instance == null) { instance = this; }
        AssignPlayerIndexToBindings();
    }

    private void Update()
    {
        //For player1
        ResetUnusedButtons(bindingP1.dunk);
        ResetUnusedButtons(bindingP1.dash);
        ResetUnusedButtons(bindingP1.interact);
        ResetUnusedButtons(bindingP1.throwBall);
        ResetUnusedButtons(bindingP1.grapple);
        ResetUnusedButtons(bindingP1.detectBall);

        //For player2
        ResetUnusedButtons(bindingP2.dunk);
        ResetUnusedButtons(bindingP2.dash);
        ResetUnusedButtons(bindingP2.interact);
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
            case CustomKeyCode.LT:
                if (_index == PlayerIndex.One)
                {
                    if (i_state.Triggers.Left > PlayerPrefs.GetFloat("REU_Trigger_Treshold", GameManager.playerOne.triggerTreshold))
                    {
                        return ButtonState.Pressed;
                    }
                    else
                    {
                        return ButtonState.Released;
                    }
                }
                else if (_index == PlayerIndex.Two)
                {
                    if (i_state.Triggers.Left > PlayerPrefs.GetFloat("REU_Trigger_Treshold", GameManager.playerOne.triggerTreshold))
                    {
                        return ButtonState.Pressed;
                    }
                    else
                    {
                        return ButtonState.Released;
                    }
                }
                return null;
            case CustomKeyCode.RT:
                if (_index == PlayerIndex.One)
                {
                    if (i_state.Triggers.Right > PlayerPrefs.GetFloat("REU_Trigger_Treshold", GameManager.playerOne.triggerTreshold))
                    {
                        return ButtonState.Pressed;
                    }
                    else
                    {
                        return ButtonState.Released;
                    }
                }
                else if (_index == PlayerIndex.Two)
                {
                    if (i_state.Triggers.Right > GameManager.playerTwo.triggerTreshold)
                    {
                        return ButtonState.Pressed;
                    }
                    else
                    {
                        return ButtonState.Released;
                    }
                }
                return null;
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
            case CustomAxisCode.LeftJoystickX:
                return i_state.ThumbSticks.Left.X;
            case CustomAxisCode.LeftJoystickY:
                return i_state.ThumbSticks.Left.Y;
            case CustomAxisCode.RightJoystickX:
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

    public void AssignNewInputsToAction(ButtonAction _concernedAction, List<CustomKeyCode> _newKeyList, PlayerIndex _whichInputIndex)
    {
        //_concernedAction.buttons[_whichInputIndex].key = _newKey;
        // make a check on all others input to see if there are multiples
    }

    //Badly wrote. TODO: generalize
    public void AssignPlayerIndexToBindings()
    {
        //// For player 1
        //foreach(var button in bindingP1.dunk.buttons) { button.index = PlayerIndex.One; } // Pas possible aprce que c'est une struct. La remplecer c'est détruire et reconstruire. Or on détruit pas un objet d'une liste
        //for (int i = 0; i < bindingP1.dunk.buttons.Count; i++) { bindingP1.dunk.buttons[i].index = PlayerIndex.One; }
        //for (int i = 0; i < bindingP1.dash.buttons.Count; i++) { bindingP1.dash.buttons[i].index = PlayerIndex.One; }
        //for (int i = 0; i < bindingP1.throwBall.buttons.Count; i++) { bindingP1.throwBall.buttons[i].index = PlayerIndex.One; }
        //for (int i = 0; i < bindingP1.grapple.buttons.Count; i++) { bindingP1.grapple.buttons[i].index = PlayerIndex.One; }
        //for (int i = 0; i < bindingP1.interact.buttons.Count; i++) { bindingP1.interact.buttons[i].index = PlayerIndex.One; }
        //for (int i = 0; i < bindingP1.detectBall.buttons.Count; i++) { bindingP1.detectBall.buttons[i].index = PlayerIndex.One; }
        //for (int i = 0; i < bindingP1.moveX.axis.Length; i++) { bindingP1.moveX.axis[i].index = PlayerIndex.One; }
        //for (int i = 0; i < bindingP1.moveY.axis.Length; i++) { bindingP1.moveY.axis[i].index = PlayerIndex.One; }
        //for (int i = 0; i < bindingP1.aimX.axis.Length; i++) { bindingP1.aimX.axis[i].index = PlayerIndex.One; }
        //for (int i = 0; i < bindingP1.aimY.axis.Length; i++) { bindingP1.aimY.axis[i].index = PlayerIndex.One; }

        //// For player 2
        //for (int i = 0; i < bindingP2.dunk.buttons.Count; i++) { bindingP2.dunk.buttons[i].index = PlayerIndex.Two; }
        //for (int i = 0; i < bindingP2.dash.buttons.Count; i++) { bindingP2.dash.buttons[i].index = PlayerIndex.Two; }
        //for (int i = 0; i < bindingP2.throwBall.buttons.Count; i++) { bindingP2.throwBall.buttons[i].index = PlayerIndex.Two; }
        //for (int i = 0; i < bindingP2.grapple.buttons.Count; i++) { bindingP2.grapple.buttons[i].index = PlayerIndex.Two; }
        //for (int i = 0; i < bindingP2.interact.buttons.Count; i++) { bindingP2.interact.buttons[i].index = PlayerIndex.Two; }
        //for (int i = 0; i < bindingP2.detectBall.buttons.Count; i++) { bindingP2.detectBall.buttons[i].index = PlayerIndex.Two; }
        //for (int i = 0; i < bindingP2.moveX.axis.Length; i++) { bindingP2.moveX.axis[i].index = PlayerIndex.Two; }
        //for (int i = 0; i < bindingP2.moveY.axis.Length; i++) { bindingP2.moveY.axis[i].index = PlayerIndex.Two; }
        //for (int i = 0; i < bindingP2.aimX.axis.Length; i++) { bindingP2.aimX.axis[i].index = PlayerIndex.Two; }
        //for (int i = 0; i < bindingP2.aimY.axis.Length; i++) { bindingP2.aimY.axis[i].index = PlayerIndex.Two; }
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
    LT,
    RT,
    PadUp,
    PadDown,
    PadLeft,
    PadRight,
    L3,
    R3,
    Start,
    Back,
    Null
}

public enum CustomAxisCode
{
    LeftJoystickX,
    LeftJoystickY,
    RightJoystickX,
    RightJoystickY,
    Null
}



