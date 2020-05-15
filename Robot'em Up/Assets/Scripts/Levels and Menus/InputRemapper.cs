using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class InputRemapper : MonoBehaviour
{
    [Range(0, 1)] public float joystickTreshold = 0.6f;

    private bool isInNavigation = true;
    private bool isInRemappingMode = false;
    private bool waitForResetReset;
    private bool waitForJoystickYReset;
    private bool waitForJoystickXReset;
    private float restTimeOfJoystickX;
    private float currentRestTimeOfJoystickX;
    private bool waitForRightShoulderReset;
    private bool waitForLeftShoulderReset;
    private bool waitForAReset;
    private int categoryNumber;
    private bool isInputChangingOpen;


    void Update()
    {
        GamePadState i_state = GamePad.GetState(PlayerIndex.One);

        if (isInNavigation)
        {
            // Managing Up and Down
            if (i_state.ThumbSticks.Left.Y > joystickTreshold || i_state.DPad.Up == ButtonState.Pressed)
            {
                if (!waitForJoystickYReset)
                {
                    MoveAbove();
                    waitForJoystickYReset = true;
                }
            }
            else if (i_state.ThumbSticks.Left.Y < -joystickTreshold || i_state.DPad.Down == ButtonState.Pressed)
            {
                if (!waitForJoystickYReset)
                {
                    MoveBelow();
                    waitForJoystickYReset = true;
                }

            }
            else
            {
                waitForJoystickYReset = false;
            }

            // Managing Left and Right
            if (i_state.ThumbSticks.Left.X > joystickTreshold || i_state.DPad.Right == ButtonState.Pressed)
            {
                if (!waitForJoystickXReset)
                {
                    MoveRight();
                    waitForJoystickXReset = true;
                }
            }
            else if (i_state.ThumbSticks.Left.X < -joystickTreshold || i_state.DPad.Left == ButtonState.Pressed)
            {
                if (!waitForJoystickXReset)
                {
                    MoveLeft();
                    waitForJoystickXReset = true;
                }
            }
            else
            {
                waitForJoystickXReset = false;
            }

            // Managing Buttons
            if (i_state.Buttons.A == ButtonState.Pressed)// Open Remapping mode
            {
                if (waitForAReset) { return; } else { OpenRemapInput(); waitForAReset = true; }
            }
            else
            {
                waitForAReset = false;
            }

            if (i_state.Buttons.B == ButtonState.Pressed)
            {
                ReturnToSettings();
            }

            if (i_state.Buttons.Back == ButtonState.Pressed)
            {
                if (waitForResetReset) { return; } else { waitForResetReset = true; ResetToDefaultInputs(); }
            }
            else if (i_state.Buttons.Back == ButtonState.Released)
            {
                waitForResetReset = false;
            }

            // Managing KeyBoard
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoveBelow();
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveAbove();
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                MoveRight();
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MoveLeft();
            }
        }

        else if (isInRemappingMode)
        {
            // Left Joystick
            // Managing Up and Down
            if (i_state.ThumbSticks.Left.Y > joystickTreshold)
            {
                RemapUsingJoystick();
            }
            else if (i_state.ThumbSticks.Left.Y < -joystickTreshold)
            {
                RemapUsingJoystick(CustomAxisCode.LeftJoystickY);
            }

            // Managing Left and Right
            if (i_state.ThumbSticks.Left.X > joystickTreshold)
            {
                RemapUsingJoystick();
            }
            else if (i_state.ThumbSticks.Left.X < -joystickTreshold)
            {
                RemapUsingJoystick();
            }

            // Right Joystick
            // Managing Up and Down
            if (i_state.ThumbSticks.Right.Y > joystickTreshold)
            {
                RemapUsingJoystick();
            }
            else if (i_state.ThumbSticks.Right.Y < -joystickTreshold)
            {
                RemapUsingJoystick(CustomAxisCode.LeftJoystickY);
            }

            // Managing Left and Right
            if (i_state.ThumbSticks.Right.X > joystickTreshold)
            {
                RemapUsingJoystick();
            }
            else if (i_state.ThumbSticks.Right.X < -joystickTreshold)
            {
                RemapUsingJoystick();
            }

            // D Pad 
            if (i_state.DPad.Up == ButtonState.Pressed)
            {
                RemapUsingButton(CustomKeyCode.PadUp);
            }
            if (i_state.DPad.Down == ButtonState.Pressed)
            {
                RemapUsingButton(CustomKeyCode.PadDown);
            }
            if (i_state.DPad.Right == ButtonState.Pressed)
            {
                RemapUsingButton(CustomKeyCode.PadRight);
            }
            if (i_state.DPad.Left == ButtonState.Pressed)
            {
                RemapUsingButton(CustomKeyCode.PadLeft);
            }
            
            // Buttons
            if (i_state.Buttons.A == ButtonState.Pressed)
            {
                RemapUsingButton(CustomKeyCode.A);
            }
            if (i_state.Buttons.B == ButtonState.Pressed)
            {
                RemapUsingButton(CustomKeyCode.B);
            }
            if (i_state.Buttons.X == ButtonState.Pressed)
            {
                RemapUsingButton(CustomKeyCode.X);
            }
            if (i_state.Buttons.Y == ButtonState.Pressed)
            {
                RemapUsingButton(CustomKeyCode.Y);
            }

            if (i_state.Buttons.RightShoulder == ButtonState.Pressed)
            {
                RemapUsingButton(CustomKeyCode.RB);
            }
            if (i_state.Buttons.LeftShoulder == ButtonState.Pressed)
            {
                RemapUsingButton(CustomKeyCode.LB);
            }

            if (i_state.Triggers.Right >= 0.8f)
            {
                RemapUsingButton(CustomKeyCode.RT);
            }
            if (i_state.Triggers.Left >= 0.8f)
            {
                RemapUsingButton(CustomKeyCode.LT);
            }

            if (i_state.Buttons.Back == ButtonState.Pressed)
            {
                RemapUsingButton(CustomKeyCode.Back);
            }
        }
    }

    public void MoveAbove()
    {

    }

    public void MoveBelow()
    {

    }

    public void MoveRight()
    {

    }

    public void MoveLeft()
    {

    }

    void OpenInputWindow()
    {
        isInNavigation = true;
    }

    public void ReturnToSettings()
    {

    }

    public void OpenRemapInput()
    {
        isInNavigation = false;
        isInRemappingMode = true;
        // Greys everything else
        // Removes the current input text
        // Waits until an input is pressed
        // Once one is pressed, wait X sec before closing the mode
        // If another input is pressed, add it as a combination
        // Quit remap and return as navigation mode
    }

    public void RemapUsingButton(CustomKeyCode keyCode = CustomKeyCode.Null)
    {
        if (keyCode != CustomKeyCode.Null)
        {

        }
    }

    public void RemapUsingJoystick(CustomAxisCode axisCode = CustomAxisCode.Null)
    {
        if (axisCode != CustomAxisCode.Null)
        {

        }
    }

    public void DeactivateRemapperMode()
    {

    }

    public void ResetToDefaultInputs()
    {

    }
}
