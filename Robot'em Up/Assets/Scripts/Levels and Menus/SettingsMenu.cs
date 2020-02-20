using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using XInputDotNetPure;

public class SettingsMenu : MonoBehaviour
{
    //private List<GameObject> categories = new List<GameObject>();

    public List<GameObject> menuCategories = new List<GameObject>();
    [ReadOnly] public string currentCategory; 
    private GameObject selectedCategory;
    private int selectedCategoryIndex;
    private bool waitForJoystickReset;
    private bool waitForButtonReset;

    private bool waitForAResetOne;

    private int categoryNumber;


    // Start is called before the first frame update
    void Start()
    {
        selectedCategoryIndex = 0;
        DisplayCategorySettings();
    }

    void ChangeCategory(int _plusOrMinus)
    {
        int i_addition = (int)Mathf.Sign(_plusOrMinus);
        if (selectedCategoryIndex + i_addition < menuCategories.Count && selectedCategoryIndex + i_addition >= 0)
        {
            selectedCategoryIndex += i_addition;
            DisplayCategorySettings();
        }
        else
        {
            Debug.Log("play out of array sound");

            // Play "error" sound
        }
    }

    void DisplayCategorySettings()
    {
        for (int i = 0; i < menuCategories.Count; i++)
        {
            if (i == selectedCategoryIndex)
            {
                menuCategories[i].SetActive(true);
                selectedCategory = menuCategories[i];
            }
            else
            {
                menuCategories[i].SetActive(false);
            }
        }


    }

    // TODO: make the navigation available.
    // Joystick = settings navigation and modification
    // LB & RB = Category navigation                        Script Done, needs tweaking to just go to next Category
    // Each Category changing makes the available settings change, but the easy way to do it is to get the UIBehaviour Script of each category main object, and store the settings in it.

    void Update()
    {
        currentCategory = selectedCategory.name;

        GamePadState i_state = GamePad.GetState(PlayerIndex.One);

        // Managing Up and Down
        if (i_state.ThumbSticks.Left.Y > 0 || i_state.DPad.Up == ButtonState.Pressed)
        {
            if (!waitForJoystickReset)
            {
                SelectPreviousSettings();
                waitForJoystickReset = true;
            }
        }
        else if (i_state.ThumbSticks.Left.Y < 0 || i_state.DPad.Down == ButtonState.Pressed)
        {
            if (!waitForJoystickReset)
            {
                SelectNextSettings();
                waitForJoystickReset = true;
            }

        }
        else
        {
            waitForJoystickReset = false;
        }

        // Managing Left and Right
        if (i_state.ThumbSticks.Left.X > 0 || i_state.DPad.Right == ButtonState.Pressed)
        {
            if (!waitForJoystickReset)
            {
                IncreaseValue();
                waitForJoystickReset = true;
            }
        }
        else if (i_state.ThumbSticks.Left.X < 0 || i_state.DPad.Left == ButtonState.Pressed)
        {
            if (!waitForJoystickReset)
            {
                DecreaseValue();
                waitForJoystickReset = true;
            }

        }
        else
        {
            waitForJoystickReset = false;
        }

        // Managing Buttons
        if (i_state.Buttons.A == ButtonState.Pressed)
        {
            //if (waitForAResetOne) { return; } else { selectedButton.onClick.Invoke(); waitForAResetOne = true; } 
        }
        else
        {
            waitForAResetOne = false;
        }

        if (i_state.Buttons.B == ButtonState.Pressed)
        {
            //CloseLevelSelector();
        }

        if (i_state.Buttons.RightShoulder == ButtonState.Pressed)
        {
            if (waitForButtonReset) { return; } else { waitForButtonReset = true; ChangeCategory(1); }
        }
        else
        {
            waitForButtonReset = false;
        }

        if (i_state.Buttons.LeftShoulder == ButtonState.Pressed)
        {
            if (waitForButtonReset) { return; } else { waitForButtonReset = true; ChangeCategory(-1);}
        }
        else
        {
            waitForButtonReset = false;
        }


        // Managing KeyBoard
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SelectNextSettings();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SelectPreviousSettings();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            IncreaseValue();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            DecreaseValue();
        }
    }


    void IncreaseValue()
    {

    }

    void DecreaseValue()
    {

    }

    void SelectNextSettings()
    {

    }

    void SelectPreviousSettings()
    {

    }
}
