using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using XInputDotNetPure;

public class SettingsMenu : MonoBehaviour
{
    //private List<GameObject> categories = new List<GameObject>();
    public SettingsDescriptionManaging descriptionManaging;
    public List<GameObject> menuCategories = new List<GameObject>();
    [ReadOnly] public string currentCategory;
    private GameObject selectedCategory;
    private int selectedCategoryIndex;
    private SettingsMenuOrganizer settingsParentScript;
    //private GameObject[] currentCategorySettings;
    private UIBehaviour selectedSetting;
    [ReadOnly] public int selectedSettingIndex;
    [ReadOnly] public string selectedSettingName;

    private bool waitForResetReset;
    private bool waitForJoystickYReset;
    private bool waitForJoystickXReset;
    public float normalRestTimeOfJoystick = 0.5f;
    private float restTimeOfJoystickX;
    private float currentRestTimeOfJoystickX;
    private bool waitForRightShoulderReset;
    private bool waitForLeftShoulderReset;
    private bool waitForAReset;
    private int categoryNumber;


    // Start is called before the first frame update
    void Start()
    {
        restTimeOfJoystickX = normalRestTimeOfJoystick;
        selectedCategoryIndex = 0;
        ChangeCategory(0);
    }

    void ChangeCategory(int _plusOrMinus)
    {
        int i_addition = 0;
        if (_plusOrMinus != 0)
        {
            i_addition = (int)Mathf.Sign(_plusOrMinus);
        }

        if (selectedCategoryIndex + i_addition < menuCategories.Count && selectedCategoryIndex + i_addition >= 0)
        {
            selectedCategoryIndex += i_addition;
            settingsParentScript = menuCategories[selectedCategoryIndex].GetComponent<SettingsMenuOrganizer>();

            DisplayCategorySettings();

            selectedSettingIndex = 0;
            selectedSetting = settingsParentScript.SelectSetting(selectedSettingIndex);    // Always reset to the first setting of the new category
            SetDescriptionTexts(selectedSetting);

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
        selectedSettingName = selectedSetting.gameObject.name;

        GamePadState i_state = GamePad.GetState(PlayerIndex.One);

        // Managing Up and Down
        if (i_state.ThumbSticks.Left.Y > 0 || i_state.DPad.Up == ButtonState.Pressed)
        {
            if (!waitForJoystickYReset)
            {
                SelectPreviousSettings();
                waitForJoystickYReset = true;
            }
        }
        else if (i_state.ThumbSticks.Left.Y < 0 || i_state.DPad.Down == ButtonState.Pressed)
        {
            if (!waitForJoystickYReset)
            {
                SelectNextSettings();
                waitForJoystickYReset = true;
            }

        }
        else
        {
            waitForJoystickYReset = false;
        }

        // Managing Left and Right
        if (i_state.ThumbSticks.Left.X > 0 || i_state.DPad.Right == ButtonState.Pressed)
        {
            if (!waitForJoystickXReset)
            {
                IncreaseValue();
                waitForJoystickXReset = true;
                currentRestTimeOfJoystickX = restTimeOfJoystickX;
            }
        }
        else if (i_state.ThumbSticks.Left.X < 0 || i_state.DPad.Left == ButtonState.Pressed)
        {
            if (!waitForJoystickXReset)
            {
                DecreaseValue();
                waitForJoystickXReset = true;
                currentRestTimeOfJoystickX = restTimeOfJoystickX;
            }
        }
        else
        {
            waitForJoystickXReset = false;
            restTimeOfJoystickX = normalRestTimeOfJoystick;
        }
        currentRestTimeOfJoystickX -= Time.deltaTime;
        if (currentRestTimeOfJoystickX <= 0)
        {
            waitForJoystickXReset = false;
            if (selectedSetting is SliderUI)
            {
                restTimeOfJoystickX -= 0.05f;
            }
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
            //CloseLevelSelector();
        }

        if (i_state.Buttons.RightShoulder == ButtonState.Pressed)
        {
            if (waitForRightShoulderReset) { return; } else { waitForRightShoulderReset = true; ChangeCategory(1); }
        }
        else if (i_state.Buttons.RightShoulder == ButtonState.Released)
        {
            waitForRightShoulderReset = false;
        }

        if (i_state.Buttons.LeftShoulder == ButtonState.Pressed)
        {
            if (waitForLeftShoulderReset) { return; } else { waitForLeftShoulderReset = true; ChangeCategory(-1); }
        }
        else if (i_state.Buttons.LeftShoulder == ButtonState.Released)
        {
            waitForLeftShoulderReset = false;
        }


        if (i_state.Buttons.Back == ButtonState.Pressed)
        {
            if (waitForResetReset) { return; } else { waitForResetReset = true; ResetToDefault(); }
        }
        else if (i_state.Buttons.Back == ButtonState.Released)
        {
            waitForResetReset = false;
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
        selectedSetting.IncreaseValue();
    }

    void DecreaseValue()
    {
        selectedSetting.DecreaseValue();
    }

    void PressingA()
    {
        selectedSetting.PressingA();
    }

    void SelectNextSettings()
    {
        if (selectedSettingIndex + 1 < settingsParentScript.childrenObjects.Length)
        {
            selectedSettingIndex++;
            selectedSetting = settingsParentScript.SelectSetting(selectedSettingIndex);
            SetDescriptionTexts(selectedSetting);

        }
    }

    void SelectPreviousSettings()
    {
        if (selectedSettingIndex - 1 >= 0)
        {
            selectedSettingIndex--;
            selectedSetting = settingsParentScript.SelectSetting(selectedSettingIndex);
            SetDescriptionTexts(selectedSetting);
        }
    }

    void SetDescriptionTexts(UIBehaviour _selectedSetting)
    {
        descriptionManaging.UpdateTitle(_selectedSetting.settingsTitle);
        descriptionManaging.UpdateDescription(_selectedSetting.settingsDescription);
    }

    void ResetToDefault()
    {
        SettingsMenuOrganizer i_categoryScript = selectedCategory.GetComponent<SettingsMenuOrganizer>();
        foreach (var setting in i_categoryScript.childrenObjects)
        {
            setting.GetComponent<UIBehaviour>().ResetValueToDefault();
        }
    }
}
