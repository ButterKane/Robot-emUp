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
    public static MainMenu instance;
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

    private int firstShownLevelIndex;
    private int lastShownLevelIndex;

    private Animator animator;
    private bool creditShown = false;

    public Image levelSelectorUpArrow;
    public Image levelSelectorDownArrow;

    public Transform goBackInformation;
    public Transform confirmInformation;

    private Canvas canvas;

    private void Start()
    {
        instance = this;
        canvas = GetComponent<Canvas>();
        Time.timeScale = PlayerPrefs.GetFloat("gamespeed", 1f);
        animator = GetComponent<Animator>();
        buttons = menuButtons;
        if (mainMenuCanvas == null) { mainMenuCanvas = GetComponent<Canvas>(); }
        waitForAResetOne = true;
        waitForAResetTwo = true;
        if (sceneList != null) { sceneList.gameObject.SetActive(false); }
       // SelectButton(buttons[0]);
        GameManager gm = GameManager.i;
        if (!showOnAwake)
        {
            mainMenuCanvas.enabled = false;
            isMainMenuActive = false;
        } else
        {
            mainMenuCanvas.enabled = true;
            isMainMenuActive = true;
        }
        levelSelectorDownArrow.enabled = false;
        levelSelectorUpArrow.enabled = false;
        Invoke("SelectDefaultButton", Time.deltaTime);
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
                if (i_state.Buttons.B == ButtonState.Pressed || Input.GetKeyDown(KeyCode.Escape))
                {
                    if (sceneList != null && sceneList.gameObject.activeSelf)
                    {
                        CloseLevelSelector();
                    }
                    if (creditShown)
                    {
                        HideCredits();
                    }
                }
                else { waitForBResetOne = true; }
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


    public void ShowCredits()
    {
        confirmInformation.gameObject.SetActive(false);
        animator.SetBool("ShowCredits", true);
        creditShown = true;
        MainMenuCameraPivot.RotateToCredits();
    }

    public void HideCredits()
    {
        confirmInformation.gameObject.SetActive(true);
        animator.SetBool("ShowCredits", false);
        creditShown = false;
        MainMenuCameraPivot.RotateToMainPanel();
    }

    public void ShowSelector()
    {
        selectorOutline.enabled = true;
        selectorArrow.enabled = true;
    }

    public void HideSelector()
    {
        selectorOutline.enabled = false;
        selectorArrow.enabled = false;
    }

    void HideButtons()
    {
        foreach (Button b in menuButtons)
        {
            b.gameObject.SetActive(false);
        }
        /*
        if (selectedButton != null)
        {
            DOTween.Complete(selectedButton);
            selectedButton.transform.DOMoveX(Screen.width * menuDefaultPosition, 0.1f).OnComplete(HideButtons2);
            selectedButton = null;
        } else
        {
            HideButtons2();
        }
        */
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

    void SelectSettingButton()
    {
        SelectButton(buttons[3], false);
    }


    void SelectButton ( Button _button, bool _showAnimation = true )
    {
        if (creditShown) { return; }
        if (selectedButton != null)
        {
            if (selectedButton == _button) { return; }
        }
        selectedButton = _button;
        selectedButtonDefaultPosition = selectedButton.transform.position.x;
        RectTransform i_buttonTransform = _button.GetComponent<RectTransform>();
        selectorArrow.rectTransform.position = i_buttonTransform.position;

        //Reset every button color
        foreach (Button b in buttons)
        {
            if (b.name != "BUTTON_Quit")
            {
                TextMeshProUGUI tmp = b.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.color = defaultColor;
                }
            }
        }
        Color wantedColor = selectedColor;
        if (selectedButton.name == "BUTTON_Quit")
        {
            wantedColor = selectedButton.GetComponent<Image>().color;
        }
        TextMeshProUGUI tmpro = selectedButton.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpro != null)
        {
            //tmpro.color = wantedColor;
        }
        selectorArrow.color = wantedColor;
        selectorOutline.transform.DOMove(i_buttonTransform.position, 0.1f).SetEase(Ease.OutSine).SetUpdate(true);
        //selectorOutline.rectTransform.transform.localScale = new Vector3(0, selectorOutline.rectTransform.transform.localScale.y, 1);
        selectorOutline.color = wantedColor;
        selectedButtonIndex = buttons.IndexOf(_button);
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
        goBackInformation.gameObject.SetActive(true);
        HideButtons();
        sceneList.gameObject.SetActive(true);
        LevelSelector lselector = sceneList.GetComponent<LevelSelector>();
        buttons = lselector.buttons;
        Invoke("SelectDefaultButton", Time.deltaTime);
        float i = 0;
        foreach (Button b in buttons)
        {
            b.gameObject.SetActive(true);
        }
        firstShownLevelIndex = 0;
        lastShownLevelIndex = 6;
        CenterScrollOnItem(sceneList.transform.Find("Scrollbar").GetComponent<Scrollbar>(), 0);
        /*
        foreach (Button b in buttons)
        {
            if (i == buttons.Count - 1)
            {
                b.transform.DOLocalMoveX(Screen.width * menuDefaultPosition, menuTransitionSpeed + i * 0.025f);
            }
            else
            {
                b.transform.DOMoveX(Screen.width * menuDefaultPosition, menuTransitionSpeed + i * 0.025f);
            }
            i++;
        }
        */
    }

    public void CloseLevelSelector ()
    {
        goBackInformation.gameObject.SetActive(false);
        levelSelectorDownArrow.enabled = false;
        levelSelectorUpArrow.enabled = false;
        FeedbackManager.SendFeedback("event.PressExit", this);
        float i = 0;
        bool callBackCalled = false;
        foreach (Button b in buttons)
        {
            b.gameObject.SetActive(true);
        }
        HideLevelSelector();
        /*
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
        */
        buttons = menuButtons;
    }

    private void HideLevelSelector()
    {
        sceneList.gameObject.SetActive(false);
        Time.timeScale = PlayerPrefs.GetFloat("gamespeed", 1f);
        foreach (Button b in menuButtons)
        {
            b.gameObject.SetActive(true);
        }
        Invoke("SelectLevelSelectorButton", Time.deltaTime);
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
        confirmInformation.gameObject.SetActive(false);
        goBackInformation.gameObject.SetActive(true);
        isMainMenuActive = false;
        foreach (Button b in menuButtons)
        {
            b.gameObject.SetActive(false);
        }
        Invoke("OpenSettings", Time.deltaTime);
        canvas.enabled = false;
    }

    public void CloseSettings()
    {
        confirmInformation.gameObject.SetActive(true);
        goBackInformation.gameObject.SetActive(false);
        Time.timeScale = PlayerPrefs.GetFloat("gamespeed", 1f);
        foreach (Button b in menuButtons)
        {
            b.gameObject.SetActive(true);
        }
        Invoke("SelectSettingButton", Time.deltaTime);
        isMainMenuActive = true;
        canvas.enabled = true;
    }

    private void OpenSettings()
    {
        SettingsMenu.instance.Open();
    }

    public void OpenInputRemap()
    {
        FeedbackManager.SendFeedback("event.PressSettings", this);
        inputRemapMenuCanvas.enabled = true;
        isMainMenuActive = false;
    }

    void SelectNextButton()
	{
        FeedbackManager.SendFeedback("event.MenuUpAndDown", this);
        selectedButtonIndex++;
		selectedButtonIndex = Mathf.Clamp(selectedButtonIndex, 0, buttons.Count-1);
        if (sceneList != null && sceneList.gameObject.activeSelf) { CenterScrollOnItem(sceneList.transform.Find("Scrollbar").GetComponent<Scrollbar>(), 1); }
        SelectButton(buttons[selectedButtonIndex]);
    }

	void SelectPreviousButton()
	{
        FeedbackManager.SendFeedback("event.MenuUpAndDown", this);
        selectedButtonIndex--;
		selectedButtonIndex = Mathf.Clamp(selectedButtonIndex, 0, buttons.Count-1);
        if (sceneList != null && sceneList.gameObject.activeSelf) { CenterScrollOnItem(sceneList.transform.Find("Scrollbar").GetComponent<Scrollbar>(), -1); }
        SelectButton(buttons[selectedButtonIndex]);
    }

	public void LoadScene(string _name)
	{
		GameManager.LoadSceneByIndex(GameManager.GetSceneIndexFromName(_name));
	}

	public void CenterScrollOnItem ( Scrollbar scrollbar, int direction )
	{
        float newScrollValue = scrollbar.value;
        switch (direction)
        {
            case 1:
                //Scrolling down
                if (selectedButtonIndex >= lastShownLevelIndex + 1 && selectedButtonIndex <= buttons.Count - 1)
                {
                    newScrollValue = Mathf.Clamp(scrollbar.value - (1f / scrollbar.numberOfSteps), 0f, 1f);
                    //StartCoroutine(ScrollBar_C(scrollbar, newScrollValue));
                    scrollbar.value = newScrollValue;
                    firstShownLevelIndex = Mathf.Clamp(firstShownLevelIndex + 1, 0, buttons.Count - 1);
                    lastShownLevelIndex = Mathf.Clamp(lastShownLevelIndex + 1, 0, buttons.Count - 1);
                }
                break;
            case -1:
                //Scrolling up
                if (selectedButtonIndex < firstShownLevelIndex && selectedButtonIndex >= 0)
                {
                    newScrollValue = Mathf.Clamp(scrollbar.value + (1f / scrollbar.numberOfSteps), 0f, 1f);
                    scrollbar.value = newScrollValue;
                   // StartCoroutine(ScrollBar_C(scrollbar, newScrollValue));
                    firstShownLevelIndex = Mathf.Clamp(firstShownLevelIndex - 1, 0, buttons.Count - 1);
                    lastShownLevelIndex = Mathf.Clamp(lastShownLevelIndex - 1, 0, buttons.Count - 1);
                }
                break;
            case 0:
                //Reset scrollbar
                scrollbar.value = 1;
                break;
        }
        if (firstShownLevelIndex > 0)
        {
            levelSelectorUpArrow.enabled = true;
        } else
        {
            levelSelectorUpArrow.enabled = false;
        }
        if (lastShownLevelIndex < buttons.Count -1)
        {
            levelSelectorDownArrow.enabled = true;
        } else
        {
            levelSelectorDownArrow.enabled = false;
        }
	}

    IEnumerator ScrollBar_C(Scrollbar sb, float value)
    {
        float initValue = sb.value;
        for (float i = 0; i < 1f; i+= Time.deltaTime * 10f)
        {
            sb.value = Mathf.Lerp(initValue, value, i / 1f);
            yield return null;
        }
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
