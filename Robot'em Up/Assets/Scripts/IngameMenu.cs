using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XInputDotNetPure;
using TMPro;
using DG.Tweening;

public class IngameMenu : MonoBehaviour
{
    public static IngameMenu instance;

    [Header("Settings")]
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color defaultColor;
    [SerializeField] private float transitionDuration = 0.2f;

    [Header("References")]
    [SerializeField] private List<Button> buttons;
    [SerializeField] private Image selectorArrow;
    [SerializeField] private Image selectorOutline;
    [SerializeField] private Image background;
    [SerializeField] private Image holder;

    private Canvas canvas;
	private bool opened;

    private bool waitForAReset;
    private bool waitForJoystickReset;
    private bool waitForStartReset;

    private Button selectedButton;
    private int selectedButtonIndex;

    public GameObject optionMenuPrefab;
    private GameObject optionMenu;
    private Canvas optionMenuCanvas;

    public GameObject abilitiesMenuPrefab;

    private bool subMenuOpened;

    private bool inTransition;

    private Tweener selectorArrowTween;
    private Tweener selectorOutlineTween;
    private Tweener transitionTween;

    private void Awake ()
	{
        instance = this;
		canvas = GetComponent<Canvas>();
		canvas.enabled = false;
		opened = false;
		SelectButton(buttons[0]);
        InitiateSubMenus();
    }

    public bool IsOpened()
    {
        if (opened) return true;
        return false;
    }

	private void Update ()
	{
		if (LoadingScreen.loading) return;
        GamePadState i_state = GamePad.GetState(PlayerIndex.One);
        if (i_state.ThumbSticks.Left.Y > 0.3f)
        {
            if (waitForJoystickReset || !opened || subMenuOpened) return;
            SelectPreviousButton();
            waitForJoystickReset = true;
        }
        else if (i_state.ThumbSticks.Left.Y < -0.3f)
        {
            if (waitForJoystickReset || !opened || subMenuOpened) return;
            SelectNextButton();
            waitForJoystickReset = true;
        }
        else
        {
            waitForJoystickReset = false;
        }

        if (i_state.Buttons.A == ButtonState.Pressed)
        {
            if (waitForAReset || !opened || subMenuOpened) return;
            selectedButton.onClick.Invoke();
            waitForAReset = true;
        }
        else
        {
            waitForAReset = false;
        }

        if (i_state.Buttons.Start == ButtonState.Pressed)
        {
            if (waitForStartReset || subMenuOpened) return;
            if (opened) { Close(); } else { Open(); }
            waitForStartReset = true;
        }
        else
        {
            waitForStartReset = false;
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
    public void Open()
    {
        if (inTransition) return;
        inTransition = true;
        selectorArrow.enabled = false;
        selectorOutline.enabled = false;
        holder.transform.localScale = new Vector3(0, holder.transform.localScale.y, 1f);
        transitionTween = holder.transform.DOScaleX(1f, transitionDuration).SetEase(Ease.OutSine).SetUpdate(true).OnComplete(FinishOpen);

        background.transform.localScale = new Vector3(background.transform.localScale.x, 0, 1f);
        background.transform.DOScaleY(1f, transitionDuration).SetEase(Ease.OutSine).SetUpdate(true);

        FeedbackManager.SendFeedback("event.OpenPauseMenu", this);
        foreach (PlayerController p in GameManager.players)
        {
            p.DisableInput();
        }
        Time.timeScale = 0f;
        VibrationManager.CancelAllVibrations();

        transform.SetAsLastSibling();
        canvas.enabled = true;
        opened = true;
        //LayoutRebuilder.ForceRebuildLayoutImmediate(holder.rectTransform);
    }

    public void Close()
    {
        if (inTransition) return;
        inTransition = true;
        selectorArrow.enabled = false;
        selectorOutline.enabled = false;
        transitionTween = holder.transform.DOScaleX(0, transitionDuration).SetEase(Ease.OutSine).SetUpdate(true).OnComplete(SelectDefaultButton).OnComplete(FinishClose);

        background.transform.DOScaleY(0, transitionDuration).SetEase(Ease.OutSine).SetUpdate(true);

        FeedbackManager.SendFeedback("event.ClosePauseMenu", this);
        foreach (PlayerController p in GameManager.players)
        {
            p.EnableInput();
        }
        Time.timeScale = PlayerPrefs.GetFloat("REU_GameSpeed", GameManager.i.gameSpeed) / 100f;
        opened = false;
    }

    private void FinishClose()
    {
        canvas.enabled = false;
        inTransition = false;
    }

    private void FinishOpen()
    {
        SelectDefaultButton();
        inTransition = false;
    }

    public void OpenMainPanel()
    {
        if (transitionTween != null) transitionTween.Kill(true);
        if (selectorOutlineTween != null) selectorOutlineTween.Kill(true);
        if (selectorArrowTween != null) selectorArrowTween.Kill(true);

        subMenuOpened = false;
        selectorArrow.enabled = true;
        selectorOutline.enabled = true;
        Color newColor = selectorOutline.color;
        newColor.a = 0;
        selectorOutline.color = newColor;
        selectorArrow.color = newColor;
        selectorOutlineTween = selectorOutline.DOFade(1f, transitionDuration).SetUpdate(true).SetEase(Ease.OutSine);
        selectorArrowTween = selectorArrow.DOFade(1f, transitionDuration).SetUpdate(true).SetEase(Ease.OutSine);
        holder.gameObject.SetActive(true);
    }

    public void CloseMainPanel()
    {
        if (transitionTween != null) transitionTween.Kill(true);
        if (selectorOutlineTween != null) selectorOutlineTween.Kill(true);
        if (selectorArrowTween != null) selectorArrowTween.Kill(true);

        selectorArrow.enabled = false;
        selectorOutline.enabled = false;
        holder.gameObject.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadMainMenu()
    {
        FeedbackManager.SendFeedback("event.OpenPauseMenu", this);
        EndlessUI.instance.HideEndlessUI();
        GameManager.LoadSceneByIndex(0);
    }

    private void SelectDefaultButton ()
    {
        selectorOutline.enabled = true;
        selectorArrow.enabled = true;
        if (selectedButton != null)
        {
            SelectButton(selectedButton);
        }
        else
        {
            SelectButton(buttons[0]);
        }
    }

    private void SelectNextButton ()
    {
        selectedButtonIndex++;
        selectedButtonIndex = Mathf.Clamp(selectedButtonIndex, 0, buttons.Count - 1);
        FeedbackManager.SendFeedback("event.MenuUpAndDown", this);
        SelectButton(buttons[selectedButtonIndex]);
    }

    private void SelectPreviousButton ()
    {
        selectedButtonIndex--;
        selectedButtonIndex = Mathf.Clamp(selectedButtonIndex, 0, buttons.Count - 1);
        FeedbackManager.SendFeedback("event.MenuUpAndDown", this);
        SelectButton(buttons[selectedButtonIndex]);
    }
    private void SelectButton ( Button _button, bool _showAnimation = true )
    {
        selectedButton = _button;
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
            tmpro.color = wantedColor;
        }
        selectorArrow.color = wantedColor;
        selectorOutline.transform.position = i_buttonTransform.position;
        selectorOutline.rectTransform.transform.localScale = new Vector3(0, selectorOutline.rectTransform.transform.localScale.y, 1);
        selectorOutline.rectTransform.DOScaleX(-1f, 0.2f).SetUpdate(true).SetEase(Ease.OutSine);
        selectorOutline.color = wantedColor;
        selectedButtonIndex = buttons.IndexOf(_button);
    }

    public void OpenSettingsMenu ()
    {
        subMenuOpened = true;
        CloseMainPanel();
        FeedbackManager.SendFeedback("event.PressSettings", this);
        optionMenuCanvas.enabled = true;
        optionMenu.GetComponent<SettingsMenu>().CheckListWhenLaunchingSettings();
        optionMenu.GetComponent<SettingsMenu>().settingsMenuIsActive = true;
    }
    public void OpenAbilitiesMenu ()
    {
        subMenuOpened = true;
        AbilityMenu.instance.Open();
    }

    public void OpenAbilitiesMenuAtSpecificOne ( ConcernedAbility _concernedAbility, Upgrade _newAbilityLevel )
    {
        subMenuOpened = true;
        AbilityMenu.instance.Open();
        AbilityMenu.instance.HighlightUpgrade(_concernedAbility, _newAbilityLevel);
    }

    public void InitiateSubMenus ()
    {
        if (optionMenuPrefab != null && optionMenu == null)
        {
            optionMenu = Instantiate(optionMenuPrefab, gameObject.transform);
            optionMenu.GetComponent<SettingsMenu>().scriptLinkedToThisOne = this;
            optionMenuCanvas = optionMenu.GetComponent<Canvas>();
            optionMenuCanvas.enabled = false;
        }
        if (abilitiesMenuPrefab != null && AbilityMenu.instance == null)
        {
            GameObject abilityMenu = Instantiate(abilitiesMenuPrefab, gameObject.transform);
            abilityMenu.transform.localScale = Vector3.one;
            AbilityMenu.instance.Close();
        }
    }
}
