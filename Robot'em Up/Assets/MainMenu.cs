using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XInputDotNetPure;

public class MainMenu : MonoBehaviour
{
	private List<Button> buttons = new List<Button>();

	public List<Button> menuButtons = new List<Button>();
	private Button selectedButton;
	public Image selectorArrow;
	public Image selectorOutline;
	private int selectedButtonIndex;
	private bool waitForJoystickResetOne;
	private bool waitForJoystickResetTwo;

	private bool waitForAResetOne;
	private bool waitForAResetTwo;
	public ScrollRect sceneList;
	private bool enableRBandRTButtons;

	void SelectButton(Button _button)
	{
		selectedButton = _button;
		RectTransform buttonTransform = _button.GetComponent<RectTransform>();
		if (sceneList != null) { CenterScrollOnItem(sceneList.GetComponent<LevelSelector>().GetComponent<ScrollRect>(), buttonTransform); }
		selectorArrow.rectTransform.position = _button.transform.position + ((Vector3.right * buttonTransform.sizeDelta.x / 2 * buttonTransform.localScale.x) + Vector3.right * (100 * selectorArrow.transform.localScale.x)) * transform.localScale.x;
		selectorOutline.rectTransform.position = buttonTransform.position;
		selectorOutline.rectTransform.sizeDelta = new Vector2(buttonTransform.sizeDelta.x * (buttonTransform.localScale.x / selectorOutline.rectTransform.localScale.x) * 0.8f, buttonTransform.sizeDelta.y * (buttonTransform.localScale.y / selectorOutline.rectTransform.localScale.y) * 0.6f);
		selectedButtonIndex = buttons.IndexOf(_button);
	}

	public void Close()
	{
		GameManager.CloseLevelMenu();
	}

	public void QuitGame ()
	{
		Application.Quit();
	}

	public void OpenMenu()
	{
		SceneManager.LoadScene(0);
	}
	public void OpenLevelSelector()
	{
		sceneList.gameObject.SetActive(true);
		LevelSelector lselector = sceneList.GetComponent<LevelSelector>();
		buttons = lselector.buttons;
		SelectButton(buttons[0]);
	}

	public void CloseLevelSelector()
	{
		sceneList.gameObject.SetActive(false);
		buttons = menuButtons;
		SelectButton(buttons[0]);
	}

	public void StartGame()
	{
		SceneManager.LoadScene(1);
		Time.timeScale = 1f;
	}

	private void Awake ()
	{
		buttons = menuButtons;
		waitForAResetOne = true;
		waitForAResetTwo = true;
		if (sceneList != null) { sceneList.gameObject.SetActive(false); }
		SelectButton(buttons[0]);
		GameManager gm = FindObjectOfType<GameManager>();
		if (gm != null) { enableRBandRTButtons = true; }
	}
	private void Update ()
	{
		GamePadState state = GamePad.GetState(PlayerIndex.One);
		for (int i = 0; i < 2; i++)
		{
			if (i == 0) { state = GamePad.GetState(PlayerIndex.One); }
			if (i == 1) { state = GamePad.GetState(PlayerIndex.Two); }
			if (state.ThumbSticks.Left.Y > 0)
			{
				if (i == 0 )
				{
					if (!waitForJoystickResetOne)
					{
						SelectPreviousButton();
						waitForJoystickResetOne = true;
					}
				} else if (i == 1)
				{
					if (!waitForJoystickResetTwo)
					{
						SelectPreviousButton();
						waitForJoystickResetTwo = true;
					}
				}
			}
			else if (state.ThumbSticks.Left.Y < 0)
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
				} else if (i == 1)
				{
					waitForJoystickResetTwo = false;
				}
			}
			if (state.Buttons.A == ButtonState.Pressed)
			{
				if (i == 0) { if (waitForAResetOne) { return; } else { selectedButton.onClick.Invoke(); waitForAResetOne = true; } }
				if (i == 1) { if (waitForAResetTwo) { return; } else { selectedButton.onClick.Invoke(); waitForAResetTwo = true; } }
			} else
			{
				if (i ==0) { waitForAResetOne = false; }
				if (i == 1) { waitForAResetTwo = false; }
			}
			if (state.Buttons.B == ButtonState.Pressed)
			{
				CloseLevelSelector();
			}
			if (enableRBandRTButtons)
			{
				if (state.Buttons.RightShoulder == ButtonState.Pressed)
				{
					if (SceneManager.GetActiveScene().buildIndex < SceneManager.sceneCountInBuildSettings - 1)
					{
						GameManager.LoadSceneByIndex(SceneManager.GetActiveScene().buildIndex + 1);
					}
				}
				if (state.Buttons.LeftShoulder == ButtonState.Pressed)
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

	void SelectNextButton()
	{
		selectedButtonIndex++;
		selectedButtonIndex = Mathf.Clamp(selectedButtonIndex, 0, buttons.Count-1);
		SelectButton(buttons[selectedButtonIndex]);
	}

	void SelectPreviousButton()
	{
		selectedButtonIndex--;
		selectedButtonIndex = Mathf.Clamp(selectedButtonIndex, 0, buttons.Count-1);
		SelectButton(buttons[selectedButtonIndex]);
	}

	public void CenterScrollOnItem ( ScrollRect _scroll, RectTransform _target )
	{
		// Item is here
		RectTransform scrollTransform = _scroll.GetComponent<RectTransform>();
		Mask mask = scrollTransform.GetComponentInChildren<Mask>();
		var itemCenterPositionInScroll = GetWorldPointInWidget(scrollTransform, GetWidgetWorldPoint(_target));
		// But must be here
		var targetPositionInScroll = GetWorldPointInWidget(scrollTransform, GetWidgetWorldPoint(mask.rectTransform));
		// So it has to move this distance
		var difference = targetPositionInScroll - itemCenterPositionInScroll;
		difference.z = 0f;

		//clear axis data that is not enabled in the scrollrect
		if (!_scroll.horizontal)
		{
			difference.x = 0f;
		}
		if (!_scroll.vertical)
		{
			difference.y = 0f;
		}

		var normalizedDifference = new Vector2(
			difference.x / (_scroll.content.rect.size.x - scrollTransform.rect.size.x),
			difference.y / (_scroll.content.rect.size.y - scrollTransform.rect.size.y));

		var newNormalizedPosition = _scroll.normalizedPosition - normalizedDifference;
		if (_scroll.movementType != ScrollRect.MovementType.Unrestricted)
		{
			newNormalizedPosition.x = Mathf.Clamp01(newNormalizedPosition.x);
			newNormalizedPosition.y = Mathf.Clamp01(newNormalizedPosition.y);
		}

		_scroll.normalizedPosition = newNormalizedPosition;
	}

	private Vector3 GetWidgetWorldPoint ( RectTransform target )
	{
		//pivot position + item size has to be included
		var pivotOffset = new Vector3(
			(0.5f - target.pivot.x) * target.rect.size.x,
			(0.5f - target.pivot.y) * target.rect.size.y,
			0f);
		var localPosition = target.localPosition + pivotOffset;
		return target.parent.TransformPoint(localPosition);
	}
	private Vector3 GetWorldPointInWidget ( RectTransform target, Vector3 worldPoint )
	{
		return target.InverseTransformPoint(worldPoint);
	}
}
