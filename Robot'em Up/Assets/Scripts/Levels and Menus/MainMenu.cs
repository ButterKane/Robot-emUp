using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XInputDotNetPure;

public class MainMenu : MonoBehaviour
{
	private List<Button> buttons = new List<Button>();

    public bool isMainMenuActive = true;
    public List<Button> menuButtons = new List<Button>();
    public GameObject optionMenuPrefab;
    private GameObject optionMenu;
    private Canvas optionMenuCanvas;
    private Button selectedButton;
	public Image selectorArrow;
	public Image selectorOutline;
	private int selectedButtonIndex;
	private bool waitForJoystickResetOne;
	private bool waitForJoystickResetTwo;

	private bool waitForAResetOne;
	private bool waitForAResetTwo;
    [ReadOnly] public bool waitForBResetOne;
    public ScrollRect sceneList;
	private bool enableRBandRTButtons;

    private void Start()
    {
        buttons = menuButtons;
        waitForAResetOne = true;
        waitForAResetTwo = true;
        if (sceneList != null) { sceneList.gameObject.SetActive(false); }
        SelectButton(buttons[0]);
        GameManager gm = GameManager.i; ;
        if (gm != null) { enableRBandRTButtons = true; }
        if (optionMenuPrefab != null)
        {
            optionMenu = Instantiate(optionMenuPrefab);
            optionMenu.GetComponent<SettingsMenu>().scriptLinkedToThisOne = this;
            optionMenuCanvas = optionMenu.GetComponent<Canvas>();
            optionMenuCanvas.enabled = false;
        }
        SelectButton(menuButtons[0]);
    }
	private void Update ()
	{
		GamePadState i_state = GamePad.GetState(PlayerIndex.One);
        if (isMainMenuActive && gameObject.activeSelf)
        {
            for (int i = 0; i < 2; i++)
            {
                if (i == 0) { i_state = GamePad.GetState(PlayerIndex.One); }
                if (i == 1) { i_state = GamePad.GetState(PlayerIndex.Two); }
                if (i_state.ThumbSticks.Left.Y > 0)
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
                else if (i_state.ThumbSticks.Left.Y < 0)
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
                if (i_state.Buttons.B == ButtonState.Pressed && waitForBResetOne == false)
                {
                    CloseLevelSelector();
                }
                else
                {
                    waitForBResetOne = true;
                }
                if (enableRBandRTButtons)
                {
                    if (i_state.Buttons.RightShoulder == ButtonState.Pressed)
                    {
                        if (SceneManager.GetActiveScene().buildIndex < SceneManager.sceneCountInBuildSettings - 1)
                        {
                            GameManager.LoadSceneByIndex(SceneManager.GetActiveScene().buildIndex + 1);
                        }
                    }
                    if (i_state.Buttons.LeftShoulder == ButtonState.Pressed)
                    {
                        if (SceneManager.GetActiveScene().buildIndex > 0)
                        {
                            GameManager.LoadSceneByIndex(SceneManager.GetActiveScene().buildIndex - 1);
                        }
                    }
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


    void SelectButton ( Button _button )
    {
        selectedButton = _button;
        RectTransform i_buttonTransform = _button.GetComponent<RectTransform>();
        if (sceneList != null) { CenterScrollOnItem(sceneList.GetComponent<LevelSelector>().GetComponent<ScrollRect>(), i_buttonTransform); }
        selectorArrow.rectTransform.position = _button.transform.position + ((Vector3.right * i_buttonTransform.sizeDelta.x / 2 * i_buttonTransform.localScale.x) + Vector3.right * (100 * selectorArrow.transform.localScale.x)) * transform.localScale.x;
        selectorOutline.rectTransform.position = i_buttonTransform.position;
        selectorOutline.rectTransform.sizeDelta = new Vector2(i_buttonTransform.sizeDelta.x * (i_buttonTransform.localScale.x / selectorOutline.rectTransform.localScale.x) * 0.8f, i_buttonTransform.sizeDelta.y * (i_buttonTransform.localScale.y / selectorOutline.rectTransform.localScale.y) * 0.6f);
        selectedButtonIndex = buttons.IndexOf(_button);
    }

    public void Close ()
    {
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
        GameManager.LoadSceneByIndex(0);
    }

    public void OpenLevelSelector ()
    {
        sceneList.gameObject.SetActive(true);
        LevelSelector lselector = sceneList.GetComponent<LevelSelector>();
        buttons = lselector.buttons;
        SelectButton(buttons[0]);
    }

    public void CloseLevelSelector ()
    {
        FeedbackManager.SendFeedback("event.PressExit", this);
        sceneList.gameObject.SetActive(false);
        buttons = menuButtons;
        SelectButton(buttons[0]);
        Time.timeScale = PlayerPrefs.GetFloat("REU_GameSpeed");
    }

    public void StartGame ()
    {
        FeedbackManager.SendFeedback("event.PressPlay", this);
        SceneManager.LoadScene(1);
        Time.timeScale = PlayerPrefs.GetFloat("REU_GameSpeed");
    }

    public void GoToSettings ()
    {
        FeedbackManager.SendFeedback("event.PressSettings", this);
        optionMenuCanvas.enabled = true;
        optionMenu.GetComponent<SettingsMenu>().FillSettingsDisplayWithPlayerPrefs();
        isMainMenuActive = false;
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
