using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class InputRemapper : MonoBehaviour
{
    [Range(0, 1)] public float joystickTreshold = 0.6f;
    public GameObject[] remappableActions;
    private SettingsInputGroup[] remappableActionsGroups;

    [NonSerialized] public MainMenu scriptLinkedToThisOne;
    public GameObject highlightObject;


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
    private bool isInputChangingOpen;

    private int focusedLineIndex;
    private int focusedColumnIndex;

    private ButtonAction currentButtonAction;
    private AxisAction currentAxisAction;
    private PlayerIndex whichPlayerIndex;

    private bool assigningNewInput;
    private float allocatedTimeToInput = 1;
    private float timeLeftToInput;
    public List<CustomKeyCode> sentInputs;

    private void Start()
    {
        remappableActionsGroups = new SettingsInputGroup[remappableActions.Length];
        for (int i = 0; i < remappableActions.Length; i++)
        {
            remappableActionsGroups[i] = remappableActions[i].GetComponent<SettingsInputGroup>();
        }
        focusedLineIndex = 0;
        focusedColumnIndex = 0;
        HighlightCurrentCase();

    }

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
                RemapUsingJoystick(CustomAxisCode.LeftJoystickY);
            }
            else if (i_state.ThumbSticks.Left.Y < -joystickTreshold)
            {
                RemapUsingJoystick(CustomAxisCode.LeftJoystickY);
            }

            // Managing Left and Right
            if (i_state.ThumbSticks.Left.X > joystickTreshold)
            {
                RemapUsingJoystick(CustomAxisCode.LeftJoystickX);
            }
            else if (i_state.ThumbSticks.Left.X < -joystickTreshold)
            {
                RemapUsingJoystick(CustomAxisCode.LeftJoystickX);
            }

            // Right Joystick
            // Managing Up and Down
            if (i_state.ThumbSticks.Right.Y > joystickTreshold)
            {
                RemapUsingJoystick(CustomAxisCode.RightJoystickY);
            }
            else if (i_state.ThumbSticks.Right.Y < -joystickTreshold)
            {
                RemapUsingJoystick(CustomAxisCode.RightJoystickY);
            }

            // Managing Left and Right
            if (i_state.ThumbSticks.Right.X > joystickTreshold)
            {
                RemapUsingJoystick(CustomAxisCode.RightJoystickX);
            }
            else if (i_state.ThumbSticks.Right.X < -joystickTreshold)
            {
                RemapUsingJoystick(CustomAxisCode.RightJoystickX);
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

        //Timer for new inputs
        if (assigningNewInput)
        {
            timeLeftToInput -= Time.unscaledDeltaTime;
            if (timeLeftToInput < 0) { sentInputs.Clear(); InputHandler.instance.AssignNewInputsToAction(currentButtonAction, sentInputs, whichPlayerIndex); assigningNewInput = false; }
        }

    }

    public void MoveAbove()
    {
        if (focusedLineIndex-1 >=0) { focusedLineIndex--; HighlightCurrentCase(); }
        else
        { 
            //Play "impossible" feedback;
        }
        
    }

    public void MoveBelow()
    {
        if (focusedLineIndex + 1 <= remappableActions.Length-1 ) { focusedLineIndex++; HighlightCurrentCase(); }
        else
        {
            //Play "impossible" feedback;
        }
    }

    public void MoveRight()
    {
        if (focusedColumnIndex == 0) { focusedColumnIndex = 1; HighlightCurrentCase(); }
        else
        {
            //Play "impossible" feedback;
        }
    }

    public void MoveLeft()
    {
        if (focusedColumnIndex == 1) { focusedColumnIndex = 0; HighlightCurrentCase(); }
        else
        {
            //Play "impossible" feedback;
        }
    }

    private void HighlightCurrentCase()
    {
        if (focusedColumnIndex == 0) { highlightObject.transform.position = remappableActionsGroups[focusedLineIndex].inputP1TMP.gameObject.transform.position; }
        else if (focusedColumnIndex == 1) { highlightObject.transform.position = remappableActionsGroups[focusedLineIndex].inputP2TMP.gameObject.transform.position; }
    }

    void OpenInputWindow()
    {
        isInNavigation = true;
    }

    public void ReturnToSettings()
    {
        FeedbackManager.SendFeedback("event.MenuBack", this);

        Time.timeScale = 0; // make sure it is still stopped

        scriptLinkedToThisOne.waitForBResetOne = true;
        scriptLinkedToThisOne.isMainMenuActive = true;

        GetComponent<Canvas>().enabled = false;
    }

    public void OpenRemapInput()
    {
        isInNavigation = false;
        isInRemappingMode = true;
        IdentifyTheFocusedInputInMatrix();
        // Greys everything else
        // Removes the current input text
        // Waits until an input is pressed
        // Once one is pressed, wait X sec before closing the mode
        // If another input is pressed, add it as a combination
        // Quit remap and return as navigation mode
    }

    public void RemapUsingButton(CustomKeyCode keyCode = CustomKeyCode.Null)
    {
        if (keyCode == CustomKeyCode.Null) { return; }

        if (!assigningNewInput)
        {
            assigningNewInput = true;
            timeLeftToInput = allocatedTimeToInput;
        }

        if (timeLeftToInput > 0)
        {
            sentInputs.Add(keyCode);
        }
    }

    public void RemapUsingJoystick(CustomAxisCode axisCode = CustomAxisCode.Null)
    {
        if (axisCode == CustomAxisCode.Null) { return; }

        if (axisCode == CustomAxisCode.LeftJoystickX || axisCode == CustomAxisCode.RightJoystickX)
        {
            // Change joystick action to left joystick (so X and Y)
        }

        if (axisCode == CustomAxisCode.RightJoystickX || axisCode == CustomAxisCode.RightJoystickX)
        {
            // Change joystick action to right joystick (so X and Y)
        }
    }

    public void DeactivateRemapperMode()
    {

    }

    public void ResetToDefaultInputs()
    {

    }

    public void IdentifyTheFocusedInputInMatrix()
    {
        if (remappableActionsGroups[focusedLineIndex].actionAndInputs.inputType == InputType.Button) { currentButtonAction = remappableActionsGroups[focusedLineIndex].actionAndInputs.buttonInfoPlayer1; }
        if (remappableActionsGroups[focusedLineIndex].actionAndInputs.inputType == InputType.Axis) { currentAxisAction = remappableActionsGroups[focusedLineIndex].actionAndInputs.axisInfoPlayer1; }

        if (focusedColumnIndex == 0) { whichPlayerIndex = PlayerIndex.One; }
        else if (focusedColumnIndex == 1) { whichPlayerIndex = PlayerIndex.Two; }
    }
}
