using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XInputDotNetPure;

public class SettingsMenuOld : MonoBehaviour
{
    public static Dictionary<string, int> sliderSettings = new Dictionary<string, int>();
    public static Dictionary<string, int> multiChoiceSettings = new Dictionary<string, int>();
    public static Dictionary<string, bool> toggleSettings = new Dictionary<string, bool>();

    public static SettingsMenuOld instance;

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
    private SettingsMenuOrganizerOld settingsParentScript;
    //private GameObject[] currentCategorySettings;
    private UIBehaviour selectedSetting;
    [ReadOnly] public int selectedSettingIndex;
    [ReadOnly] public string selectedSettingName;
    public float normalRestTimeOfJoystick = 0.5f;
    public IngameMenu scriptLinkedToThisOne;
    public InputRemapper inputRemapper;

    [Separator("Slider variables")]
    public Vector2 minMaxTimeBeforeReset = new Vector2(0.2f, 0.5f);
    public float timeToReachMinTimeBeforeReset;
    public AnimationCurve timeBeforeResetEvolution;
    private float currentSliderTimeRatioBeforeReset;
    private float timeJoystickHeldDown;
    private float sliderSpecificRestTime;

    [NonSerialized] public bool settingsMenuIsActive;
    private bool waitForResetReset;
    private bool waitForStartReset;
    private bool waitForJoystickYReset;
    private bool waitForJoystickXReset;
    private float restTimeOfJoystickX;
    private float currentRestTimeOfJoystickX;
    private bool waitForRightShoulderReset;
    private bool waitForLeftShoulderReset;
    private bool waitForAReset;
    private int categoryNumber;
    private bool isInputChangingOpen;


    void Awake()
    {
        instance = this;
        ComputeSettings();
        foreach (var category in menuCategories)
        {
            category.SetActive(true);
        }
        foreach (var slider in sliderSettings)
        {
          //  Debug.Log("Current setting " + slider.Key + " with value " + slider.Value);
        }
        foreach (var multiChoice in multiChoiceSettings)
        {
          //  Debug.Log("Current setting " + multiChoice.Key + " with value " + multiChoice.Value);
        }
        foreach (var toggle in toggleSettings)
        {
          //  Debug.Log("Current setting " + toggle.Key + " with value " + toggle.Value);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        restTimeOfJoystickX = normalRestTimeOfJoystick;
        selectedCategoryIndex = 0;
        ChangeCategory(0);
        CheckDefaultValueOrNot();
    }

    public static void SetSelectedSetting(UIBehaviour setting)
    {
        instance.selectedSetting = setting;
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
            FeedbackManager.SendFeedback("event.SwitchSettingsPage", this);
            selectedCategoryIndex += i_addition;
            settingsParentScript = menuCategories[selectedCategoryIndex].GetComponent<SettingsMenuOrganizerOld>();

            DisplayCategory();

            selectedSettingIndex = 0;
            selectedSetting = settingsParentScript.SelectSetting(selectedSettingIndex);    // Always reset to the first setting of the new category
            SetDescriptionTexts(selectedSetting);

        }
        else
        {
            FeedbackManager.SendFeedback("event.MenuImpossibleAction", this);
        }

        if (selectedCategoryIndex > 0)
        {
            LBImage.SetAlpha(1);
        }
        else
        {
            LBImage.SetAlpha(unselectedCategoryTitleOpacity);
        }

        if (selectedCategoryIndex >= menuCategories.Count - 1)
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

    void Update()
    {

        GamePadState i_state = GamePad.GetState(PlayerIndex.One);
        if (settingsMenuIsActive)
        {
            currentCategory = selectedCategory.name;
            selectedSettingName = selectedSetting.gameObject.name;

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
                timeJoystickHeldDown += Time.unscaledDeltaTime;
                if (!waitForJoystickXReset)
                {
                    IncreaseValue();
                    waitForJoystickXReset = true;
                    currentRestTimeOfJoystickX = restTimeOfJoystickX;
                    sliderSpecificRestTime = 0;
                }
            }
            else if (i_state.ThumbSticks.Left.X < -joystickTreshold || i_state.DPad.Left == ButtonState.Pressed)
            {
                timeJoystickHeldDown += Time.unscaledDeltaTime;
                if (!waitForJoystickXReset)
                {
                    DecreaseValue();
                    waitForJoystickXReset = true;
                    currentRestTimeOfJoystickX = restTimeOfJoystickX;
                    sliderSpecificRestTime = 0;
                }
            }
            else
            {
                waitForJoystickXReset = false;
                restTimeOfJoystickX = normalRestTimeOfJoystick;
                timeJoystickHeldDown = 0;
            }

            if (currentRestTimeOfJoystickX <= 0)
            {
                waitForJoystickXReset = false;
            }

            if (selectedSetting is SliderUI)
            {
                currentSliderTimeRatioBeforeReset = timeBeforeResetEvolution.Evaluate(timeJoystickHeldDown / timeToReachMinTimeBeforeReset); //from 1 to 0
                sliderSpecificRestTime += Time.unscaledDeltaTime;

                if (sliderSpecificRestTime >= Mathf.Lerp(minMaxTimeBeforeReset.x, minMaxTimeBeforeReset.y, currentSliderTimeRatioBeforeReset)) // from max to min
                {
                    waitForJoystickXReset = false;
                }
            }

            currentRestTimeOfJoystickX -= Time.unscaledDeltaTime;


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

            if (i_state.Buttons.Start == ButtonState.Pressed)
            {
                if (waitForStartReset) { return; } else { waitForStartReset = true; ReturnToMainMenu(); scriptLinkedToThisOne.Close(); }
            }
            else if (i_state.Buttons.Start == ButtonState.Released)
            {
                waitForStartReset = false;
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
    }


    void IncreaseValue()
    {
        FeedbackManager.SendFeedback("event.TweakValuesLeftAndRight", this);
        selectedSetting.IncreaseValue();
    }

    void DecreaseValue()
    {
        FeedbackManager.SendFeedback("event.TweakValuesLeftAndRight", this);
        selectedSetting.DecreaseValue();
    }

    void PressingA()
    {
        selectedSetting.PressingA();
    }

    void SelectNextSettings()
    {
        FeedbackManager.SendFeedback("event.MenuUpAndDown", this);
        if (selectedSettingIndex + 1 < settingsParentScript.availableSettingsToOrganize.Count)
        {
            selectedSettingIndex++;
            selectedSetting = settingsParentScript.SelectSetting(selectedSettingIndex);
            SetDescriptionTexts(selectedSetting);

        }
    }

    void SelectPreviousSettings()
    {
        FeedbackManager.SendFeedback("event.MenuUpAndDown", this);
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
        FeedbackManager.SendFeedback("event.SettingsResetToDefault", this);
        selectedCategory.GetComponent<SettingsMenuOrganizerOld>().selectedSettingInChildren.ResetValueToDefault(); // Reset the current setting to its default value
        CheckDefaultValueOrNot();
    }

    void ReturnToMainMenu()
    {
        FeedbackManager.SendFeedback("event.MenuBack", this);
        ComputeSettings();
        ModifyPlayerPrefsValues();

        //Time.timeScale = 0; // make sure it is still stopped
        settingsMenuIsActive = false;
        scriptLinkedToThisOne.OpenMainPanel();

        GetComponent<Canvas>().enabled = false;
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

    public void CheckListWhenLaunchingSettings()
    {
        FillSettingsDisplayWithPlayerPrefs();
        //CheckDefaultValueOrNot();
    }


    void CheckDefaultValueOrNot()
    {
        for (int i = 0; i < menuCategories.Count; i++)
        {
            GameObject[] i_settingsRef = menuCategories[i].GetComponent<SettingsMenuOrganizerOld>().childrenObjects;

            for (int j = 0; j < i_settingsRef.Length; j++)
            {
                UIBehaviour i_thisSetting = i_settingsRef[j].GetComponent<UIBehaviour>();

                if (i_thisSetting is SliderUI)
                {
                    SliderUI i_sliderRef = i_thisSetting as SliderUI;
                    if (i_sliderRef.currentValue != i_sliderRef.defaultValue)
                    {
                        i_sliderRef.ToggleChangeIcon(true);
                    }
                    else { i_sliderRef.ToggleChangeIcon(false);}
                }
                else if (i_thisSetting is MultichoiceUI)
                {
                    MultichoiceUI i_multiChoiceRef = i_thisSetting as MultichoiceUI;
                    if (i_multiChoiceRef.selectedChoiceIndex != i_multiChoiceRef.defaultValue)
                    {
                        i_multiChoiceRef.ToggleChangeIcon(true);
                    }
                    else { i_multiChoiceRef.ToggleChangeIcon(false); }
                }
                else if (i_thisSetting is ToggleUI)
                {
                    ToggleUI i_toggleRef = i_thisSetting as ToggleUI;
                    if (i_toggleRef.buttonIsYes != i_toggleRef.defaultValueIsYes)
                    {
                        i_toggleRef.ToggleChangeIcon(true);
                    }
                    else { i_toggleRef.ToggleChangeIcon(false); }
                }
            }
        }
    }

    // Get all settings in the option menu and save their names + current value in matching dictionaries (one for the sliders, one for multichoices and one for toggles)
    public void ComputeSettings()
    {
        sliderSettings.Clear();
        sliderSettings = new Dictionary<string, int>();
        multiChoiceSettings.Clear();
        multiChoiceSettings = new Dictionary<string, int>();
        toggleSettings.Clear();
        toggleSettings = new Dictionary<string, bool>();

        for (int i = 0; i < menuCategories.Count; i++)
        {
            GameObject[] i_settingsRef = menuCategories[i].GetComponent<SettingsMenuOrganizerOld>().childrenObjects;

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

        //DisplaySettingsValues();

    }

    // Display all the values saved for the settings, keyed with their names, as Debug Logs
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
    public void AssignSavedValuesInSettings()
    {
        for (int i = 0; i < menuCategories.Count; i++)
        {
            GameObject[] i_settingsRef = menuCategories[i].GetComponent<SettingsMenuOrganizerOld>().childrenObjects;

            for (int j = 0; j < i_settingsRef.Length; j++)
            {
                UIBehaviour i_thisSetting = i_settingsRef[j].GetComponent<UIBehaviour>();

                if (i_thisSetting is SliderUI)
                {
                    SliderUI i_sliderRef = i_thisSetting as SliderUI;

                    foreach (var savedSetting in sliderSettings)
                    {
                        if (savedSetting.Key == i_sliderRef.name)
                        {
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

    public void ModifyPlayerPrefsValues()
    {
        // SLIDER SECTION ------------------------------------

        // 0 to 100
        if (sliderSettings.TryGetValue("Screenshake_intensity", out int valueScreenShake))
        {
            PlayerPrefs.SetFloat("REU_Screenshake_intensity", valueScreenShake);
            //CameraShaker.shakeSettingsMod = ((float)valueScreenShake) / 100;
        }

        // 0 to 100
        if (sliderSettings.TryGetValue("Haptic_intensity", out int valueVibrations))
        {
            PlayerPrefs.SetFloat("REU_Haptic_intensity", valueVibrations);
            //VibrationManager.vibrationSettingsMod = ((float)valueVibrations)/50; // Only divided by 50 because 50 is the base value. We want it to be stronger if we go above 50
        }

        // 20 to 100
        if (sliderSettings.TryGetValue("GameSpeed", out int valueGameSpeed))
        {
            PlayerPrefs.SetFloat("REU_GameSpeed", valueGameSpeed);
            //GameManager.i.gameSpeed = ((float)valueGameSpeed) / 100;
        }

        // 0 to 100
        if (sliderSettings.TryGetValue("Damage Taken", out int valueDamageTaken))
        {
            PlayerPrefs.SetFloat("REU_Damage Taken", valueDamageTaken);
            //GameManager.i.damageTakenSettingsMod = ((float)valueDamageTaken)/100;
        }

        // 0 to 100
        if (sliderSettings.TryGetValue("Assisting Aim", out int valueAimAssistance))
        {
            PlayerPrefs.SetFloat("REU_Assisting Aim", valueAimAssistance);
            if (EnemyManager.i != null)
            {
                EnemyManager.i.ChangeAimAssistanceForAllEnemies(valueAimAssistance);
            }
            //GameManager.i.aimAssistanceSettingsMod = ((float)valueAimAssistance)/100;
        }

        // 10 to 100
        if (sliderSettings.TryGetValue("Trigger_Treshold", out int valueTriggerTreshold))
        {
            PlayerPrefs.SetFloat("REU_Trigger_Treshold", valueTriggerTreshold);
            //GameManager.playerOne.triggerTreshold = ((float)valueTriggerTreshold)/100;
            //GameManager.playerTwo.triggerTreshold = ((float)valueTriggerTreshold)/100; 
        }

        // 0 to 100
        if (sliderSettings.TryGetValue("Contrast", out int valueContrast))
        {
            PlayerPrefs.SetFloat("REU_Contrast", valueContrast);
            if (PostProcessManager.i != null)
            {
                //PostProcessManager.i.UpdateContrastWithSettings();
            }
            // It's post process wesh
        }

        // 0 to 100
        if (sliderSettings.TryGetValue("SFX Volume", out int valueSfxVolume))
        {
            PlayerPrefs.SetFloat("REU_SFX Volume", valueSfxVolume);
            //FeedbackManager.fXVolumeSettingsMod = ((float)valueSfxVolume) / 100;
        }

        // 0 to 100
        if (sliderSettings.TryGetValue("Dialogue Volume", out int valueDialogueVolume))
        {
            PlayerPrefs.SetFloat("REU_Dialogue Volume", valueDialogueVolume);
            //NarrationManager.narrationManager.dialogueVolumeSettingsMod = ((float)valueDialogueVolume) / 100;
        }

        // 0 to 100
        if (sliderSettings.TryGetValue("Ambiance Volume", out int valueAmbianceVolume))
        {
            PlayerPrefs.SetFloat("REU_Ambiance Volume", valueAmbianceVolume); // there's no "ambiance sound" yet, but it's ready
        }

        // 0 to 100
        if (sliderSettings.TryGetValue("Music Volume", out int valueMusicVolume))
        {
            PlayerPrefs.SetFloat("REU_Music Volume", valueMusicVolume); // Saving in prefs
            MusicManager.ChangeVolume(valueMusicVolume); // actually changing the value
        }


        // MULTICHOICE SECTION ---------------------------------------------

        // Adaptative - Easy - Medium - Difficult
        if (multiChoiceSettings.TryGetValue("Overall Difficulty", out int valueDifficulty))
        {
            PlayerPrefs.SetInt("REU_Overall Difficulty", valueDifficulty);

            if (GameManager.i != null)
            {
               // if (valueDifficulty == 0) { GameManager.ChangeDifficulty(true, valueDifficulty); }
               // else { GameManager.ChangeDifficulty(false, valueDifficulty); }
            }
        }

        // Gentle - Classic - Aggressive
        if (multiChoiceSettings.TryGetValue("Enemies Agressivity", out int valueEnemiesAgressivity))
        {
            PlayerPrefs.SetInt("REU_Enemies Agressivity", valueEnemiesAgressivity);
            if (EnemyManager.i != null) { EnemyManager.i.ChangeEnemiesAgressivity(valueEnemiesAgressivity); }
        }

        // White - Yellow - Blue - Green - Red
        if (multiChoiceSettings.TryGetValue("In-Game Text Color", out int valueTextColor))
        {
            PlayerPrefs.SetInt("REU_In-Game Text Color", valueTextColor);
            if (NarrationManager.narrationManager != null) { NarrationManager.narrationManager.ChangeTextColor(valueTextColor); }
        }

        // Small - Regular - Big - Very Big
        if (multiChoiceSettings.TryGetValue("In-Game Text Size", out int valueTextSize))
        {
            PlayerPrefs.SetInt("REU_In-Game Text Size", valueTextSize);
            if (NarrationManager.narrationManager != null) { NarrationManager.narrationManager.ChangeTextSize(valueTextSize); }
        }


        // TOGGLE SECTION ------------------------------------------------------

        if (toggleSettings.TryGetValue("Hold Down Input", out bool valueHoldDown))
        {
            if (valueHoldDown) { PlayerPrefs.SetString("REU_Hold Down Input", "true"); }
            else { PlayerPrefs.SetString("REU_Hold Down Input", "false"); }
        }

        if (toggleSettings.TryGetValue("FullScreen/Window", out bool valueFullScreen))
        {
            if (valueFullScreen) { PlayerPrefs.SetString("REU_FullScreen/Window", "true"); }
            else { PlayerPrefs.SetString("REU_FullScreen/Window", "false"); }

            if (valueFullScreen)
            {
                Screen.fullScreen = true;
            }
            else
            {
                Screen.fullScreen = false;
            }
        }

        if (toggleSettings.TryGetValue("Background Animation", out bool valueBackground))
        {
            if (valueBackground) { PlayerPrefs.SetString("REU_Background Animation", "true"); }
            else { PlayerPrefs.SetString("REU_Background Animation", "false"); }
            // Change if the grass is moving or this kind of things
        }

        if (toggleSettings.TryGetValue("Stylized In-Game Text Font", out bool valueStylizedFont))
        {
            if (valueStylizedFont) { PlayerPrefs.SetString("REU_Stylized In-Game Text Font", "true"); }
            else { PlayerPrefs.SetString("REU_Stylized In-Game Text Font", "false"); }

            if (NarrationManager.narrationManager != null) { NarrationManager.narrationManager.ChangeSelectedFont(valueStylizedFont); }
        }

        PlayerPrefs.Save();
    }

    public void FillSettingsDisplayWithPlayerPrefs()
    {
        for (int i = 0; i < menuCategories.Count; i++)
        {
            GameObject[] i_settingsRef = menuCategories[i].GetComponent<SettingsMenuOrganizerOld>().childrenObjects;

            for (int j = 0; j < i_settingsRef.Length; j++)
            {
                UIBehaviour i_thisSetting = i_settingsRef[j].GetComponent<UIBehaviour>();

                if (i_thisSetting is SliderUI)
                {
                    SliderUI i_sliderRef = i_thisSetting as SliderUI;
                    int value = Mathf.RoundToInt(PlayerPrefs.GetFloat("REU_" + i_sliderRef.name, i_sliderRef.defaultValue));
                    i_sliderRef.ForceModifyValue(value);
                }
                else if (i_thisSetting is MultichoiceUI)
                {
                    MultichoiceUI i_multiChoiceRef = i_thisSetting as MultichoiceUI;

                    i_multiChoiceRef.ForceModifyValue(PlayerPrefs.GetInt("REU_" + i_multiChoiceRef.name, i_multiChoiceRef.defaultValue));
                }
                else if (i_thisSetting is ToggleUI)
                {
                    ToggleUI i_toggleRef = i_thisSetting as ToggleUI;

                    i_toggleRef.ForceModifyValue(SwissArmyKnife.ConvertPlayerPrefStringAsBool(PlayerPrefs.GetString("REU_" + i_toggleRef.name, i_toggleRef.defaultValueIsYes? "true":"false")));
                }
            }
        }
    }
}
