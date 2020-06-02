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

    [Separator ("Variables")]
    public AbilityGroupData[] abilitesData;
    public Color normalColor;
    public Color selectedColor;
    [Range(0, 1)] public float joystickTreshold = 0.6f;

    [ReadOnly] public MainMenu scriptLinkedToThisOne;

    private AbilityGroupData selectedAbility;
    [ReadOnly] public int selectedAbilityIndex;
    [ReadOnly] public string selectedAbilityName;
    public AnimationCurve newUpgradeTextVariation;

    private bool waitForJoystickYReset;
    private Sprite[] gifImagesToPlay;
    private int currentGifImageIndex;
    private bool isNavigationAllowed = true;
    // Start is called before the first frame update
    void Start()
    {
        ResetDisplay();
    }
    
    // Update is called once per frame
    void Update()
    {
        GamePadState i_state = GamePad.GetState(PlayerIndex.One);
        if (isNavigationAllowed)
        {
            // Managing Up and Down
            if (i_state.ThumbSticks.Left.Y > joystickTreshold || i_state.DPad.Up == ButtonState.Pressed)
            {
                if (!waitForJoystickYReset)
                {
                    ChangeAbility(-1);
                    waitForJoystickYReset = true;
                }
            }
            else if (i_state.ThumbSticks.Left.Y < -joystickTreshold || i_state.DPad.Down == ButtonState.Pressed)
            {
                if (!waitForJoystickYReset)
                {
                    ChangeAbility(1);
                    waitForJoystickYReset = true;
                }

            }
            else
            {
                waitForJoystickYReset = false;
            }
        }

        if (i_state.Buttons.B == ButtonState.Pressed) { ReturnToMainMenu(); }

        if (gifImagesToPlay.Length != 0)
        {
            PlayGif(currentGifImageIndex);
            currentGifImageIndex++;
        }
    }

    void ChangeAbility(int _plusOrMinus)
    {
        int i_addition = 0;
        if (_plusOrMinus != 0)
        {
            i_addition = (int)Mathf.Sign(_plusOrMinus);
        }

        if (selectedAbilityIndex + i_addition >= 0 && selectedAbilityIndex + i_addition < abilitesData.Length)
        {
            FeedbackManager.SendFeedback("event.SwitchSettingsPage", this);
            selectedAbilityIndex += i_addition;
            selectedAbility = abilitesData[selectedAbilityIndex];

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
        selectedAbility = abilitesData[selectedAbilityIndex];

        ChangeSelectedColor();

        DisplayAbility();
    }

    public void UnlockUpgrade(Upgrade _newAbilityLevel)
    {
        StartCoroutine(UpgradeSequence_C(_newAbilityLevel));
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
        foreach ( var abilityGroup in abilitesData)
        {
            abilityGroup.backgroundImage.color = normalColor;
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
        selectedAbility = abilitesData[selectedAbilityIndex];
        ChangeSelectedColor();
        DisplayAbility();
    }

    public void GoToSpecificAbility(ConcernedAbility _concernedAbility)
    {
        for (int i = 0; i < abilitesData.Length; i++)
        {
            if (abilitesData[i].ability == _concernedAbility)
            {
                GotToSpecificAbilityByIndex(i);
            }
        }
    }

    private IEnumerator UpgradeSequence_C(Upgrade _newAbilityLevel)
    {
        isNavigationAllowed = false;
        Debug.Log("laucnhing coroutine with " + _newAbilityLevel);
        yield return new WaitForSecondsRealtime(0.5f);
        Debug.Log("coroutine step 2");

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
            Debug.Log("coroutine anim step");
            i_concernedtext.outlineWidth = 0.3f * newUpgradeTextVariation.Evaluate(1 - (animTime / 1.5f));
            animTime -= Time.unscaledDeltaTime;
            yield return null;
        }

        i_concernedtext.outlineWidth = 0.147f;

        DisplayAbility();
        yield return new WaitForSecondsRealtime(0.5f);
        isNavigationAllowed = true;
    }
}
