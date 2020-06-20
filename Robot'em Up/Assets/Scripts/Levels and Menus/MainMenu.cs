using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XInputDotNetPure;
using TMPro;
using DG.Tweening;
using System;
using UnityEngine.Events;

public class MainMenu : MonoBehaviour
{
	private List<Button> buttons = new List<Button>();

    public bool showOnAwake = false;
    public bool isMainMenuActive = true;
    public List<Button> menuButtons;

    public Canvas mainMenuCanvas;

    public GameObject optionMenuPrefab;
    private GameObject optionMenu;
    private Canvas optionMenuCanvas;

    public GameObject abilitiesMenuPrefab;
    private GameObject abilitiesMenu;
    private Canvas abilitiesMenuCanvas;

    public GameObject inputRemapMenuPrefab;
    private GameObject inputRemapMenu;
    public Canvas inputRemapMenuCanvas;

    public Color defaultColor;
    public Color selectedColor;


    private Button selectedButton;
	public Image selectorArrow;
	public Image selectorOutline;
	private int selectedButtonIndex;
	private bool waitForJoystickResetOne;
    private float selectedButtonDefaultPosition;
	private bool waitForJoystickResetTwo;

	private bool waitForAResetOne;
	private bool waitForAResetTwo;
    [NonSerialized] public bool waitForStartReset;
    [NonSerialized] public bool waitForBResetOne;
    public ScrollRect sceneList;

    public float menuDefaultPosition = 0.85f;
    public float menuSelectedPosition = 0.84f;
    public float menuHiddenPosition = 1.25f;
    public float menuTransitionSpeed = 0.4f;

    private void Start()
    {
        buttons = menuButtons;
        if (mainMenuCanvas == null) { mainMenuCanvas = GetComponent<Canvas>(); }
        waitForAResetOne = true;
        waitForAResetTwo = true;
        if (sceneList != null) { sceneList.gameObject.SetActive(false); }
        SelectButton(buttons[0]);
        GameManager gm = GameManager.i;
        InitiateSubMenus();
        if (!showOnAwake)
        {
            mainMenuCanvas.enabled = false;
            isMainMenuActive = false;
        } else
        {
            mainMenuCanvas.enabled = true;
            isMainMenuActive = true;
        }
        RestoreButtons();
    }

	private void Update ()
	{
        if (LoadingScreen.loading) { return; }
		GamePadState i_state = GamePad.GetState(PlayerIndex.One);
        if (isMainMenuActive && gameObject.activeSelf)
        {
            for (int i = 0; i < 2; i++)
            {
                if (i == 0) { i_state = GamePad.GetState(PlayerIndex.One); }
                if (i == 1) { i_state = GamePad.GetState(PlayerIndex.Two); }
                if (i_state.ThumbSticks.Left.Y > 0.3f)
                {
                    if (i == 0)
                    {
                        if (!waitForJoystickResetOne)
                        {
                            SelectPreviousButton();
                            waitForJoystickResetOne = true;
                        }
                    }
                    else if (i == 1)
                    {
                        if (!waitForJoystickResetTwo)
                        {
                            SelectPreviousButton();
                            waitForJoystickResetTwo = true;
                        }
                    }
                }
                else if (i_state.ThumbSticks.Left.Y < -0.3f)
                {
                    if (i == 0)
                    {
                        if (!waitForJoystickResetOne)
                        {
                            SelectNextButton();
                            waitForJoystickResetOne = true;
                        }
                    }
                    else if (i == 1)
                    {
                        if (!waitForJoystickResetTwo)
                        {
                            SelectNextButton();
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
                if (i_state.Buttons.A == ButtonState.Pressed)
                {
                    if (i == 0) { if (waitForAResetOne) { return; } else { selectedButton.onClick.Invoke(); waitForAResetOne = true; } }
                    if (i == 1) { if (waitForAResetTwo) { return; } else { selectedButton.onClick.Invoke(); waitForAResetTwo = true; } }
                }
                else
                {
                    if (i == 0) { waitForAResetOne = false; }
                    if (i == 1) { waitForAResetTwo = false; }
                }
                if (i_state.Buttons.B == ButtonState.Pressed && sceneList != null && sceneList.gameObject.activeSelf == true)
                {
                    CloseLevelSelector();
                }
                else { waitForBResetOne = true; }

                if (i_state.Buttons.Start == ButtonState.Pressed)
                {
                    if (waitForStartReset) { return; } else { waitForStartReset = true; Close(); }
                }
                else if (i_state.Buttons.Start == ButtonState.Released)
                {
                    waitForStartReset = false;
                }
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                SelectNextButton();
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                SelectPreviousButton();
            }
        }
	}

    void HideButtons()
    {
        if (selectedButton != null)
        {
            DOTween.Complete(selectedButton);
            selectedButton.transform.DOMoveX(Screen.width * menuDefaultPosition, 0.1f).OnComplete(HideButtons2);
            selectedButton = null;
        } else
        {
            HideButtons2();
        }
    }

    void HideButtons2()
    {
        float i = 0;
        foreach (Button b in menuButtons)
        {
            if (i == menuButtons.Count - 1)
            {
                b.transform.DOMoveX(Screen.width * menuHiddenPosition, menuTransitionSpeed + i * 0.025f);
            } else
            {
                b.transform.DOMoveX(Screen.width * menuHiddenPosition, menuTransitionSpeed + i * 0.025f);
            }
            i++;
        }
    }

    void SelectDefaultButton()
    {
        SelectButton(buttons[0], false);
    }
    void SelectLevelSelectorButton ()
    {
        SelectButton(buttons[2], false);
    }
    void RestoreButtons ()
    {
        float i = 0;
        foreach (Button b in menuButtons)
        {
            if (i == menuButtons.Count - 1)
            {
                b.transform.DOMoveX(Screen.width * menuDefaultPosition, menuTransitionSpeed + i * 0.025f);
            }
            else
            {
                b.transform.DOMoveX(Screen.width * menuDefaultPosition, menuTransitionSpeed + i * 0.025f);
            }
            i++;
        }
    }


    void SelectButton ( Button _button, bool _showAnimation = true )
    {
        if (selectedButton != null)
        {
            if (selectedButton == _button) { return; }
            if (_showAnimation)
            {
                 selectedButton.transform.DOMoveX(Screen.width * menuDefaultPosition, 0.1f);
            }
        }
        selectedButton = _button;
        selectedButtonDefaultPosition = selectedButton.transform.position.x;
        RectTransform i_buttonTransform = _button.GetComponent<RectTransform>();
        if (sceneList != null) { CenterScrollOnItem(sceneList.GetComponent<LevelSelector>().GetComponent<ScrollRect>(), i_buttonTransform); }
        float leftValue = i_buttonTransform.sizeDelta.x;
        selectorArrow.rectTransform.position = i_buttonTransform.position + (Vector3.left * (leftValue / 2f)) + (Vector3.left * 70);
        Image buttonImage = i_buttonTransform.GetComponent<Image>();
        foreach (Button b in buttons)
        {
            if (b.name != "BUTTON_Quit")
            {
                b.GetComponent<Image>().color = defaultColor;
            }
            TextMeshProUGUI tmp = b.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.color = defaultColor;
            }
            else
            {
                Text txt = b.GetComponentInChildren<Text>();
                if (txt != null)
                {
                    txt.color = defaultColor;
                }
            }
        }
        if (_button.name != "BUTTON_Quit") {
            buttonImage.color = selectedColor;
        }
        if (_showAnimation)
        {
            _button.transform.DOMoveX(Screen.width * menuSelectedPosition, 0.1f);
        }
        if (buttonImage != null)
        {
            TextMeshProUGUI tmpro = buttonImage.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpro != null)
            {
                tmpro.color = selectedColor;
            } else
            {
                Text txt = buttonImage.GetComponentInChildren<Text>();
                if (txt != null)
                {
                    txt.color = selectedColor;
                }
            }
        }
        selectorArrow.color = buttonImage.color;
        selectorOutline.transform.SetParent(_button.transform);
        selectorOutline.transform.localPosition = Vector3.zero;
        selectorOutline.rectTransform.sizeDelta = new Vector2(i_buttonTransform.sizeDelta.x, i_buttonTransform.sizeDelta.y) ;
        selectorOutline.transform.localScale = Vector3.one;
        selectorOutline.color = buttonImage.color;
        selectedButtonIndex = buttons.IndexOf(_button);
    }

    public void Close ()
    {
        GameManager.i.waitForStartReset = true;
        FeedbackManager.SendFeedback("event.ClosePauseMenu", this);
        GameManager.CloseLevelMenu();
    }

    public void QuitGame ()
    {
        Application.Quit();
    }

    public void OpenMainMenu ()
    {
        FeedbackManager.SendFeedback("event.OpenPauseMenu", this);
        EndlessUI.instance.HideEndlessUI();
        GameManager.LoadSceneByIndex(0);

    }

    public void OpenLevelSelector ()
    {
        HideButtons();
        sceneList.gameObject.SetActive(true);
        LevelSelector lselector = sceneList.GetComponent<LevelSelector>();
        buttons = lselector.buttons;
        SelectDefaultButton();
        float i = 0;
        foreach (Button b in buttons)
        {
            if (i == buttons.Count - 1)
            {
                b.transform.DOMoveX(Screen.width * menuDefaultPosition, menuTransitionSpeed + i * 0.025f);
            }
            else
            {
                b.transform.DOMoveX(Screen.width * menuDefaultPosition, menuTransitionSpeed + i * 0.025f);
            }
            i++;
        }
    }

    public void CloseLevelSelector ()
    {
        FeedbackManager.SendFeedback("event.PressExit", this);
        float i = 0;
        bool callBackCalled = false;
        foreach (Button b in buttons)
        {
            if (i >= buttons.Count - 1 || i >= 5 && !callBackCalled)
            {
                b.transform.DOMoveX(Screen.width * menuHiddenPosition, menuTransitionSpeed + i * 0.025f).OnComplete(HideLevelSelector);
                callBackCalled = true;
            }
            else
            {
                b.transform.DOMoveX(Screen.width * menuHiddenPosition, menuTransitionSpeed + i * 0.025f);
            }
            i++;
        }
        buttons = menuButtons;
    }

    private void HideLevelSelector()
    {
        sceneList.gameObject.SetActive(false);
        Time.timeScale = 1;
        RestoreButtons();
        SelectLevelSelectorButton();
    }

    public void StartGame ()
    {
        FeedbackManager.SendFeedback("event.PressPlay", this);
        LoadingScreen.StartLoadingScreen(new UnityAction(() => SceneManager.LoadScene(1)));
        EnergyManager.DecreaseEnergy(1);
        AbilityManager.ResetUpgrades();
    }

    public void GoToSettings ()
    {
        FeedbackManager.SendFeedback("event.PressSettings", this);
        optionMenuCanvas.enabled = true;
        optionMenu.GetComponent<SettingsMenu>().CheckListWhenLaunchingSettings();
        optionMenu.GetComponent<SettingsMenu>().settingsMenuIsActive = true;
        isMainMenuActive = false;
    }

    public void OpenInputRemap()
    {
        FeedbackManager.SendFeedback("event.PressSettings", this);
        inputRemapMenuCanvas.enabled = true;
        isMainMenuActive = false;
    }

    public void OpenAbilitiesMenu()
    {
        FeedbackManager.SendFeedback("event.PressSettings", this);
        abilitiesMenuCanvas.enabled = true;
        AbilityListNavigation i_script = abilitiesMenu.GetComponent<AbilityListNavigation>();
        i_script.GetUpgradeLevelsToDisplayInAbilities();
        i_script.organizer.OrganizeAbilities();
        i_script.ResetDisplay();
        isMainMenuActive = false;
        i_script.isNavigationAllowed = true;
    }

    public void OpenAbilitiesMenuAtSpecificOne(ConcernedAbility _concernedAbility, Upgrade _newAbilityLevel)
    {
        abilitiesMenuCanvas.enabled = true;
        AbilityListNavigation i_script = abilitiesMenu.GetComponent<AbilityListNavigation>();
        if (_concernedAbility == ConcernedAbility.PerfectReception)
        {
            i_script.GoToSpecificAbility(_concernedAbility);
            i_script.UnlockNextUpgradeForPerfectReception();
        }
        else
        {
            i_script.GoToSpecificAbility(_concernedAbility);
            i_script.UnlockUpgrade(_newAbilityLevel);
        }
        //AbilityManager.UnlockAbility(_concernedAbility, _newAbilityLevel)
        i_script.isNavigationAllowed = true;
        isMainMenuActive = false;
    }

    public bool DoesAbilityMenuExist()
    {
        if(abilitiesMenu != null) { return true; }
        else { return false; }
    }

    public void InitiateSubMenus()
    {
        if (optionMenuPrefab != null && optionMenu == null)
        {
            optionMenu = Instantiate(optionMenuPrefab, gameObject.transform);
            optionMenu.GetComponent<SettingsMenu>().scriptLinkedToThisOne = this;
            optionMenuCanvas = optionMenu.GetComponent<Canvas>();
            optionMenuCanvas.enabled = false;
        }
        if (abilitiesMenuPrefab != null && abilitiesMenu == null)
        {
            abilitiesMenu = Instantiate(abilitiesMenuPrefab, gameObject.transform);
            abilitiesMenu.GetComponent<AbilityListNavigation>().scriptLinkedToThisOne = this;
            abilitiesMenuCanvas = abilitiesMenu.GetComponent<Canvas>();
            abilitiesMenuCanvas.enabled = false;
        }
        //if (inputRemapMenuPrefab != null && inputRemapMenu == null)
        //{
        //    inputRemapMenu = Instantiate(abilitiesMenuPrefab);
        //    inputRemapMenu.GetComponent<InputRemapper>().scriptLinkedToThisOne = this;
        //    inputRemapMenuCanvas = abilitiesMenu.GetComponent<Canvas>();
        //    inputRemapMenuCanvas.enabled = false;
        //}
    }

    void SelectNextButton()
	{
        FeedbackManager.SendFeedback("event.MenuUpAndDown", this);
        selectedButtonIndex++;
		selectedButtonIndex = Mathf.Clamp(selectedButtonIndex, 0, buttons.Count-1);
		SelectButton(buttons[selectedButtonIndex]);
	}

	void SelectPreviousButton()
	{
        FeedbackManager.SendFeedback("event.MenuUpAndDown", this);
        selectedButtonIndex--;
		selectedButtonIndex = Mathf.Clamp(selectedButtonIndex, 0, buttons.Count-1);
		SelectButton(buttons[selectedButtonIndex]);
	}

	public void LoadScene(string _name)
	{
		GameManager.LoadSceneByIndex(GameManager.GetSceneIndexFromName(_name));
	}

	public void CenterScrollOnItem ( ScrollRect _scroll, RectTransform _target )
	{
		// Item is here
		RectTransform i_scrollTransform = _scroll.GetComponent<RectTransform>();
		Mask i_mask = i_scrollTransform.GetComponentInChildren<Mask>();
		var i_itemCenterPositionInScroll = GetWorldPointInWidget(i_scrollTransform, GetWidgetWorldPoint(_target));
		// But must be here
		var i_targetPositionInScroll = GetWorldPointInWidget(i_scrollTransform, GetWidgetWorldPoint(i_mask.rectTransform));
		// So it has to move this distance
		var i_difference = i_targetPositionInScroll - i_itemCenterPositionInScroll;
		i_difference.z = 0f;

		//clear axis data that is not enabled in the scrollrect
		if (!_scroll.horizontal)
		{
			i_difference.x = 0f;
		}
		if (!_scroll.vertical)
		{
			i_difference.y = 0f;
		}

		var i_normalizedDiff = new Vector2(
			i_difference.x / (_scroll.content.rect.size.x - i_scrollTransform.rect.size.x),
			i_difference.y / (_scroll.content.rect.size.y - i_scrollTransform.rect.size.y));

		var newNormalizedPosition = _scroll.normalizedPosition - i_normalizedDiff;
		if (_scroll.movementType != ScrollRect.MovementType.Unrestricted)
		{
			newNormalizedPosition.x = Mathf.Clamp01(newNormalizedPosition.x);
			newNormalizedPosition.y = Mathf.Clamp01(newNormalizedPosition.y);
		}

		_scroll.normalizedPosition = newNormalizedPosition;
	}

	private Vector3 GetWidgetWorldPoint ( RectTransform _target )
	{
		//pivot position + item size has to be included
		var i_pivotOffset = new Vector3(
			(0.5f - _target.pivot.x) * _target.rect.size.x,
			(0.5f - _target.pivot.y) * _target.rect.size.y,
			0f);
		var i_localPosition = _target.localPosition + i_pivotOffset;
		return _target.parent.TransformPoint(i_localPosition);
	}
	private Vector3 GetWorldPointInWidget ( RectTransform target, Vector3 worldPoint )
	{
		return target.InverseTransformPoint(worldPoint);
	}
}
