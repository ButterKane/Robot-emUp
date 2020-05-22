using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class InputRemapperNavigation : MonoBehaviour
{
    [Range(0, 1)] public float joystickTreshold = 0.6f;

    private bool isInNavigation = true;
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

    // Start is called before the first frame update
    void Start()
    {
        isInNavigation = true;
    }

    void Update()
    {
        GamePadState i_state = GamePad.GetState(PlayerIndex.One);

        // Managing Up and Down
        if (i_state.ThumbSticks.Left.Y > joystickTreshold || i_state.DPad.Up == ButtonState.Pressed)
        {
            if (!waitForJoystickYReset)
            {
                //SelectPreviousSettings();
                waitForJoystickYReset = true;
            }
        }
        else if (i_state.ThumbSticks.Left.Y < -joystickTreshold || i_state.DPad.Down == ButtonState.Pressed)
        {
            if (!waitForJoystickYReset)
            {
                //SelectNextSettings();
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
                //IncreaseValue();
                waitForJoystickXReset = true;
            }
        }
        else if (i_state.ThumbSticks.Left.X < -joystickTreshold || i_state.DPad.Left == ButtonState.Pressed)
        {
            if (!waitForJoystickXReset)
            {
                //DecreaseValue();
                waitForJoystickXReset = true;
            }
        }
        else
        {
            waitForJoystickXReset = false;
        }

        if (currentRestTimeOfJoystickX <= 0)
        {
            waitForJoystickXReset = false;
        }



        // Managing Buttons
        if (i_state.Buttons.A == ButtonState.Pressed)
        {
            if (waitForAReset) { return; } else { PressingA(); waitForAReset = true; }
        }
        else
        {
            waitForAReset = false;
        }

        if (i_state.Buttons.B == ButtonState.Pressed)
        {
            if (isInputChangingOpen)
            {
                //CloseInputChanging();
            }
            else
            {
                //ReturnToMainMenu();
            }
        }

        if (i_state.Buttons.RightShoulder == ButtonState.Pressed)
        {
            if (waitForRightShoulderReset) { return; }
            else
            {
                waitForRightShoulderReset = true; //ChangeCategory(1); 
            }
        }
        else if (i_state.Buttons.RightShoulder == ButtonState.Released)
        {
            waitForRightShoulderReset = false;
        }

        if (i_state.Buttons.LeftShoulder == ButtonState.Pressed)
        {
            if (waitForLeftShoulderReset) { return; }
            else
            {
                waitForLeftShoulderReset = true; //ChangeCategory(-1); 
            }
        }
        else if (i_state.Buttons.LeftShoulder == ButtonState.Released)
        {
            waitForLeftShoulderReset = false;
        }


        if (i_state.Buttons.Back == ButtonState.Pressed)
        {
            if (waitForResetReset) { return; }
            else
            {
                waitForResetReset = true; //ResetToDefault(); 
            }
        }
        else if (i_state.Buttons.Back == ButtonState.Released)
        {
            waitForResetReset = false;
        }



        // Managing KeyBoard
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            //SelectNextSettings();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            //SelectPreviousSettings();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            //IncreaseValue();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            //DecreaseValue();
        }
    }


    void OpenInputWindow()
    {
        isInNavigation = true;
    }

    void PressingA()
    {
        if (isInNavigation)
        {
            SelectThisInput();
        }
    }

    void SelectThisInput()
    {

    }
}
