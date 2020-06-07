using MyBox;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XInputDotNetPure;

public class AbilityListNavigation : MonoBehaviour
{
    [Separator("References")]
    public Image gifImage;
    public TextMeshProUGUI descriptionName;
    public TextMeshProUGUI descriptionMainText;
    public TextMeshProUGUI descriptionUpgrade1;
    public TextMeshProUGUI descriptionUpgrade2;
    public AbilityListOrganizer organizer;

    [Separator ("Variables")]
    public AbilityGroupData[] abilitiesData;
    public List<AbilityGroupData> availableAbilitesData;
    public Color normalColor;
    public Color selectedColor;
    public Color notAvailableColor;
    [Range(0, 1)] public float joystickTreshold = 0.6f;

    [ReadOnly] public MainMenu scriptLinkedToThisOne;

    private AbilityGroupData selectedAbility;
    [ReadOnly] public int selectedAbilityIndex;
    [ReadOnly] public string selectedAbilityName;
    public AnimationCurve newUpgradeTextVariation;

    private bool waitForJoystickYReset;
    private Sprite[] gifImagesToPlay;
    private int currentGifImageIndex;
    public bool isNavigationAllowed = true;
    private bool waitForJoystickResetOne;
    private bool waitForJoystickResetTwo;
    // Start is called before the first frame update
    void Start()
    {
        isNavigationAllowed = false;
        GetAvailableAbilitiesDatas();
        ResetDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        GamePadState i_state = GamePad.GetState(PlayerIndex.One);
        if (isNavigationAllowed)
        {
            Debug.Log("Hello it is main menu");
            for (int i = 0; i < 2; i++)
            {
                if (i == 0) { i_state = GamePad.GetState(PlayerIndex.One); }
                if (i == 1) { i_state = GamePad.GetState(PlayerIndex.Two); }

                // Managing Up and Down
                if (i_state.ThumbSticks.Left.Y > joystickTreshold || i_state.DPad.Up == ButtonState.Pressed)
                {
                    if (i == 0)
                    {
                        if (!waitForJoystickResetOne)
                        {
                            ChangeAbility(-1);
                            waitForJoystickResetOne = true;
                        }
                    }
                    else if (i == 1)
                    {
                        if (!waitForJoystickResetTwo)
                        {
                            ChangeAbility(-1);
                            waitForJoystickResetTwo = true;
                        }
                    }
                }
                else if (i_state.ThumbSticks.Left.Y < -joystickTreshold || i_state.DPad.Down == ButtonState.Pressed)
                {
                    if (i == 0)
                    {
                        if (!waitForJoystickResetOne)
                        {
                            ChangeAbility(1);
                            waitForJoystickResetOne = true;
                        }
                    }
                    else if (i == 1)
                    {
                        if (!waitForJoystickResetTwo)
                        {
                            ChangeAbility(1);
                            waitForJoystickResetTwo = true;
                        }
                    }
                }
                else
                {
                    if (i == 0)
                    {
                        waitForJoystickResetOne = false;
                    }
                    else if (i == 1)
                    {
                        waitForJoystickResetTwo = false;
                    }
                }
                
                if (i_state.Buttons.B == ButtonState.Pressed) { ReturnToMainMenu(); }
                if (i_state.Buttons.Start == ButtonState.Pressed) { ReturnToMainMenu(); scriptLinkedToThisOne.Close(); }
            }
            
            if (gifImagesToPlay != null && gifImagesToPlay.Length != 0)
            {
                PlayGif(currentGifImageIndex);
                currentGifImageIndex++;
            }
        }
    }

    void ChangeAbility(int _plusOrMinus)
    {
        int i_addition = 0;
        if (_plusOrMinus != 0)
        {
            i_addition = (int)Mathf.Sign(_plusOrMinus);
        }

        if (selectedAbilityIndex + i_addition >= 0 && selectedAbilityIndex + i_addition < availableAbilitesData.Count)
        {
            FeedbackManager.SendFeedback("event.SwitchSettingsPage", this);
            selectedAbilityIndex += i_addition;
            selectedAbility = availableAbilitesData[selectedAbilityIndex];

            ChangeSelectedColor();

            DisplayAbility();
        }
        else
        {
            FeedbackManager.SendFeedback("event.MenuImpossibleAction", this);
        }
    }

    public void GotToSpecificAbilityByIndex(int _abilityIndex)
    {
        selectedAbilityIndex = _abilityIndex;
        selectedAbility = abilitiesData[selectedAbilityIndex];

        ChangeSelectedColor();

        DisplayAbility();
    }

    public void UnlockUpgrade(Upgrade _newAbilityLevel)
    {
        StartCoroutine(UpgradeSequence_C(_newAbilityLevel));
    }

    public void UnlockNextUpgradeForPerfectReception()
    {
        StartCoroutine(UpgradePrefectReceptionSequence_C());
    }

    public void DisplayAbility()
    {
        gifImagesToPlay = selectedAbility.gifImages;
        currentGifImageIndex = 0;

        descriptionName.text = selectedAbility.abilityName;
        descriptionMainText.text = selectedAbility.abilityDescription;
        descriptionUpgrade1.text = selectedAbility.upgrade1Description;
        descriptionUpgrade2.text = selectedAbility.upgrade2Description;

        if (!selectedAbility.isBaseUnlocked) { descriptionMainText.alpha = 0.3f; }
        else { descriptionMainText.alpha=1;}
        if (!selectedAbility.isUpgrade1Unlocked) { descriptionUpgrade1.alpha = 0.3f; }
        else { descriptionUpgrade1.alpha = 1; }
        if (!selectedAbility.isUpgrade2unlocked) { descriptionUpgrade2.alpha = 0.3f; }
        else { descriptionUpgrade2.alpha = 1; }
    }

    public void ChangeSelectedColor()
    {
        foreach ( var abilityGroup in abilitiesData)
        {
            if (abilityGroup.isBaseUnlocked) { abilityGroup.backgroundImage.color = normalColor; }
            else { abilityGroup.backgroundImage.color = notAvailableColor; }
        }
        selectedAbility.backgroundImage.color = selectedColor;
    }

    public void PlayGif(int _gifImageIndex)
    {
        int index = _gifImageIndex;
        if (index >= gifImagesToPlay.Length - 1) { index = 0; currentGifImageIndex = 0;}
        gifImage.sprite = gifImagesToPlay[index];
    }

    void ReturnToMainMenu()
    {
        isNavigationAllowed = false;
        FeedbackManager.SendFeedback("event.MenuBack", this);

        Time.timeScale = 0; // make sure it is still stopped

        scriptLinkedToThisOne.waitForBResetOne = true;
        scriptLinkedToThisOne.isMainMenuActive = true;

        GetComponent<Canvas>().enabled = false;
    }

    public void ResetDisplay()
    {
        selectedAbilityIndex = 0;
        currentGifImageIndex = 0;
        selectedAbility = availableAbilitesData[selectedAbilityIndex];
        ChangeSelectedColor();
        DisplayAbility();
    }

    public void GoToSpecificAbility(ConcernedAbility _concernedAbility)
    {
        for (int i = 0; i < abilitiesData.Length; i++)
        {
            if (abilitiesData[i].ability == _concernedAbility)
            {
                GotToSpecificAbilityByIndex(i);
            }
        }
    }
    
    private void GetAvailableAbilitiesDatas()
    {
        availableAbilitesData.Clear();
        foreach (var ability in abilitiesData)
        {
            if (ability.isBaseUnlocked) { availableAbilitesData.Add(ability); }
        }
    }

    private IEnumerator UpgradeSequence_C(Upgrade _newAbilityLevel)
    {
        isNavigationAllowed = false;
        yield return new WaitForSecondsRealtime(0.5f);

        TextMeshProUGUI i_concernedtext = null;
        switch (_newAbilityLevel)
        {
            case Upgrade.Base:
                i_concernedtext = descriptionMainText;
                selectedAbility.isBaseUnlocked = true;
                break;
            case Upgrade.Upgrade1:
                i_concernedtext = descriptionUpgrade1;
                selectedAbility.isUpgrade1Unlocked = true;
                break;
            case Upgrade.Upgrade2:
                i_concernedtext = descriptionUpgrade2;
                selectedAbility.isUpgrade2unlocked = true;
                break;
            default:
                break;
        }

        i_concernedtext.alpha = 1;
        float animTime = 1.5f;
        while(animTime > 0)
        {
            i_concernedtext.outlineWidth = 0.3f * newUpgradeTextVariation.Evaluate(1 - (animTime / 1.5f));
            animTime -= Time.unscaledDeltaTime;
            yield return null;
        }

        i_concernedtext.outlineWidth = 0.147f;
        GetAvailableAbilitiesDatas();
        organizer.OrganizeAbilities();
        DisplayAbility();
        yield return new WaitForSecondsRealtime(0.5f);
        isNavigationAllowed = true;
    }

    public IEnumerator UpgradePrefectReceptionSequence_C()
    {
        isNavigationAllowed = false;
        yield return new WaitForSecondsRealtime(0.5f);
        Upgrade _newLevel;
        if (selectedAbility.isBaseUnlocked)
        {
            if (selectedAbility.isUpgrade1Unlocked)
            {
                _newLevel = Upgrade.Upgrade1;
            }
            else
            {
                _newLevel = Upgrade.Upgrade2;
            }
        }
        else
        {
            _newLevel = Upgrade.Base;
        }

        TextMeshProUGUI i_concernedtext = null;
        switch (_newLevel)
        {
            case Upgrade.Base:
                i_concernedtext = descriptionMainText;
                selectedAbility.isBaseUnlocked = true;
                break;
            case Upgrade.Upgrade1:
                i_concernedtext = descriptionUpgrade1;
                selectedAbility.isUpgrade1Unlocked = true;
                break;
            case Upgrade.Upgrade2:
                i_concernedtext = descriptionUpgrade2;
                selectedAbility.isUpgrade2unlocked = true;
                break;
            default:
                break;
        }
        GetAvailableAbilitiesDatas();
        organizer.OrganizeAbilities();
        DisplayAbility();
        i_concernedtext.alpha = 1;
        float animTime = 1.5f;
        while (animTime > 0)
        {
            i_concernedtext.outlineWidth = 0.3f * newUpgradeTextVariation.Evaluate(1 - (animTime / 1.5f));
            animTime -= Time.unscaledDeltaTime;
            yield return null;
        }

        i_concernedtext.outlineWidth = 0.147f;
        GetAvailableAbilitiesDatas();
        organizer.OrganizeAbilities();
        DisplayAbility();
        yield return new WaitForSecondsRealtime(0.5f);
        isNavigationAllowed = true;
    }
}
