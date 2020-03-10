﻿using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XInputDotNetPure;

public class SettingsMenu : MonoBehaviour
{
    public static Dictionary<string, int> sliderSettings = new Dictionary<string, int>();
    public static Dictionary<string, int> multiChoiceSettings =  new Dictionary<string, int>();
    public static Dictionary<string, bool> toggleSettings =  new Dictionary<string, bool>();

    //private List<GameObject> categories = new List<GameObject>();
    public SettingsDescriptionManaging descriptionManaging;
    public List<GameObject> menuCategories = new List<GameObject>();
    public List<Image> categoriesTitles = new List<Image>();
    public Image LBImage;
    public Image RBImage;
    [Range(0, 1)] public float unselectedCategoryTitleOpacity = 0.3f;
    [Range(0, 1)] public float joystickTreshold = 0.6f;
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
    private bool isInputChangingOpen;

    void Awake()
    {
        foreach(var category in menuCategories)
        {
            category.SetActive(true);
        }
        foreach (var slider in sliderSettings)
        {
            Debug.Log("Current setting " + slider.Key + " with value " + slider.Value);
        }
        foreach (var multiChoice in multiChoiceSettings)
        {
            Debug.Log("Current setting " + multiChoice.Key + " with value " + multiChoice.Value);
        }
        foreach (var toggle in toggleSettings)
        {
            Debug.Log("Current setting " + toggle.Key + " with value " + toggle.Value);
        }
    }

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

        if (selectedCategoryIndex + i_addition >= 0 && selectedCategoryIndex + i_addition < menuCategories.Count)
        {
            selectedCategoryIndex += i_addition;
            settingsParentScript = menuCategories[selectedCategoryIndex].GetComponent<SettingsMenuOrganizer>();

            DisplayCategory();

            selectedSettingIndex = 0;
            selectedSetting = settingsParentScript.SelectSetting(selectedSettingIndex);    // Always reset to the first setting of the new category
            SetDescriptionTexts(selectedSetting);

        }

        if (selectedCategoryIndex > 0)
        {
            LBImage.SetAlpha(1);
        }
        else
        {
            LBImage.SetAlpha(unselectedCategoryTitleOpacity);
        }

        if (selectedCategoryIndex >= menuCategories.Count-1)
        {
            RBImage.SetAlpha(unselectedCategoryTitleOpacity);
        }
        else
        {
            RBImage.SetAlpha(1);
        }
    }

    void DisplayCategory()
    {
        for (int i = 0; i < menuCategories.Count; i++)
        {
            if (i == selectedCategoryIndex)
            {
                categoriesTitles[i].SetAlpha(1);
                menuCategories[i].SetActive(true);
                selectedCategory = menuCategories[i];
            }
            else
            {
                categoriesTitles[i].SetAlpha(unselectedCategoryTitleOpacity);
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
        if (i_state.ThumbSticks.Left.Y > joystickTreshold || i_state.DPad.Up == ButtonState.Pressed)
        {
            if (!waitForJoystickYReset)
            {
                SelectPreviousSettings();
                waitForJoystickYReset = true;
            }
        }
        else if (i_state.ThumbSticks.Left.Y < -joystickTreshold || i_state.DPad.Down == ButtonState.Pressed)
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
        if (i_state.ThumbSticks.Left.X > joystickTreshold || i_state.DPad.Right == ButtonState.Pressed)
        {
            if (!waitForJoystickXReset)
            {
                IncreaseValue();
                waitForJoystickXReset = true;
                currentRestTimeOfJoystickX = restTimeOfJoystickX;
            }
        }
        else if (i_state.ThumbSticks.Left.X < -joystickTreshold || i_state.DPad.Left == ButtonState.Pressed)
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
            if (isInputChangingOpen)
            {
                CloseInputChanging();
            }
            //else if (gameIsPlaying) // So if the settings were reached through the pause menu
            else
            { 
                ReturnToMainMenu();
            }
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

    void ReturnToMainMenu()
    {
        GameManager.LoadSceneByIndex(GameManager.GetSceneIndexFromName("MainMenu"));
    }

    void OpenInputChanging()
    {
        Debug.Log("Opening input changing");
        isInputChangingOpen = true;
    }
    void CloseInputChanging()
    {
        Debug.Log("Closing input changing");
        isInputChangingOpen = false;
    }

    // Get all settings in the option menu and save their names + current value in matching dictionaries (one for the sliders, one for multichoices and one for toggles)
    public void ComputeSettingsSaved()
    {
        sliderSettings.Clear();
        sliderSettings = new Dictionary<string, int>();
        multiChoiceSettings.Clear();
        multiChoiceSettings = new Dictionary<string, int>();
        toggleSettings.Clear();
        toggleSettings = new Dictionary<string, bool>();

        for (int i = 0; i < menuCategories.Count; i++)
        {
            GameObject[] i_settingsRef = menuCategories[i].GetComponent<SettingsMenuOrganizer>().childrenObjects;

            for (int j = 0; j < i_settingsRef.Length; j++)
            {
                UIBehaviour i_thisSetting = i_settingsRef[j].GetComponent<UIBehaviour>();
                if (i_thisSetting is SliderUI)
                {
                    SliderUI i_sliderRef = i_thisSetting as SliderUI;
                    sliderSettings.Add(i_sliderRef.name, i_sliderRef.currentValue);
                }
                else if (i_thisSetting is MultichoiceUI)
                {
                    MultichoiceUI i_multiChoiceRef = i_thisSetting as MultichoiceUI;
                    multiChoiceSettings.Add(i_multiChoiceRef.name, i_multiChoiceRef.selectedChoiceIndex);
                }
                else if (i_thisSetting is ToggleUI)
                {
                    ToggleUI i_toggleRef = i_thisSetting as ToggleUI;
                    toggleSettings.Add(i_toggleRef.name, i_toggleRef.buttonIsYes);
                }
            }
        }

        DisplaySettingsValues();

    }

    // Display all the values saved for the settings, keyed with their names
    public void DisplaySettingsValues()
    {
        foreach (var slider in sliderSettings)
        {
            Debug.Log("Computed setting " + slider.Key + " with value " + slider.Value);
        }
        foreach (var multiChoice in multiChoiceSettings)
        {
            Debug.Log("Computed setting " + multiChoice.Key + " with value " + multiChoice.Value);
        }
        foreach (var toggle in toggleSettings)
        {
            Debug.Log("Computed setting " + toggle.Key + " with value " + toggle.Value);
        }
    }

    // Assign saved salues to settings
    public void AssignSavedValues()
    {
        for (int i = 0; i < menuCategories.Count; i++)
        {
            GameObject[] i_settingsRef = menuCategories[i].GetComponent<SettingsMenuOrganizer>().childrenObjects;

            for (int j = 0; j < i_settingsRef.Length; j++)
            {
                UIBehaviour i_thisSetting = i_settingsRef[j].GetComponent<UIBehaviour>();

                if (i_thisSetting is SliderUI)
                {
                    SliderUI i_sliderRef = i_thisSetting as SliderUI;

                    foreach(var savedSetting in sliderSettings)
                    {
                        if (savedSetting.Key == i_sliderRef.name)
                        {
                            Debug.Log(savedSetting.Key + " new value is " + savedSetting.Value);
                            i_sliderRef.ForceModifyValue(savedSetting.Value);
                        }
                    }
                }
                else if (i_thisSetting is MultichoiceUI)
                {
                    MultichoiceUI i_multiChoiceRef = i_thisSetting as MultichoiceUI;

                    foreach (var savedSetting in multiChoiceSettings)
                    {
                        if (savedSetting.Key == i_multiChoiceRef.name)
                        {
                            i_multiChoiceRef.ForceModifyValue(savedSetting.Value);
                        }
                    }
                }
                else if (i_thisSetting is ToggleUI)
                {
                    ToggleUI i_toggleRef = i_thisSetting as ToggleUI;
                    foreach (var savedSetting in toggleSettings)
                    {
                        if (savedSetting.Key == i_toggleRef.name)
                        {
                            i_toggleRef.ForceModifyValue(savedSetting.Value);
                        }
                    }
                }
            }
        }
    }
}
