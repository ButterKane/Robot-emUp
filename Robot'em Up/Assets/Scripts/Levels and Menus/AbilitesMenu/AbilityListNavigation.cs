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

    private bool waitForJoystickYReset;
    private Sprite[] gifImagesToPlay;
    private int currentGifImageIndex;
    // Start is called before the first frame update
    void Start()
    {
        ResetDisplay();
    }
    
    // Update is called once per frame
    void Update()
    {
        GamePadState i_state = GamePad.GetState(PlayerIndex.One);

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

    public void DisplayAbility()
    {
        gifImagesToPlay = selectedAbility.gifImages;
        currentGifImageIndex = 0;

        descriptionName.text = selectedAbility.abilityName;
        descriptionMainText.text = selectedAbility.abilityDescription;
        descriptionUpgrade1.text = selectedAbility.upgrade1Description;
        descriptionUpgrade2.text = selectedAbility.upgrade2Description;
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
}
