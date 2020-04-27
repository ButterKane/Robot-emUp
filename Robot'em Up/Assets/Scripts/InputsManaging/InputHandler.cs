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
    public GamePadButtons key;
    public ButtonState keyState;
    public bool keyUp;
    public bool keyDown;
    //public bool key => Input.GetKey(keyCode);
}

[System.Serializable]
public struct ButtonAction
{
    public SingleButton[] buttons;

    public bool keyUp
    {
        get { return true; }
    }

    //public bool keyDown
    //{
    //    get { return true; }
    //}

    //public bool key
    //{
    //    get { return true; }
    //}
}

[System.Serializable]
public struct KeyBindingStruct
{
    //public AxisAction moveX, moveY, aimX, aimY;
    public ButtonAction dunk, throwBall, interact, grapple; 
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

    // Update is called once per frame
    void Update()
    {
        
    }

    

    public ButtonState GetState(CustomKeyCode _ck, PlayerIndex _index)
    {
        GamePadState i_state = GamePad.GetState(_index);
        switch (_ck)
        {
            case CustomKeyCode.B:
                return i_state.Buttons.B;
            // Manque la ref au pad, parce que le bon Buttons.B c'est accessible par GamePadState, 
            //qui lui-même est accessible par ;
            default:
                return ButtonState.;
        }
    }
}

public enum CustomKeyCode
{
    A,
    B,
    etc
}



