using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;
using UnityEngine.UI;

public enum GamepadTrigger { Left, Right}
public class PressableUITrigger : MonoBehaviour
{
	GamePadState state;
	public GamepadTrigger trigger;
	private PlayerIndex playerIndex;
	public Sprite defaultButtonSprite;
	public Sprite pressedButtonSprite;
	public Image image;
	public float animationSpeed;
	private bool held;


	public void Init ( PlayerIndex _playerIndex)
	{
		playerIndex = _playerIndex;
		state = GamePad.GetState(_playerIndex);
		image.sprite = defaultButtonSprite;
	}

	
	private void Update ()
	{
		state = GamePad.GetState(playerIndex);
		switch (trigger)
		{
			case GamepadTrigger.Left:
				if (state.Triggers.Left > 0.1f && !held)
				{
					HoldButton();
				} else if (state.Triggers.Left <= 0 && held)
				{
					ReleaseButton();
				}
				break;
			case GamepadTrigger.Right:
				if (state.Triggers.Right > 0.1f && !held)
				{
					HoldButton();
				} else if (state.Triggers.Right <= 0 && held)
				{
					ReleaseButton();
				}
				break;
		}
		Color newColor = image.color;
		if (!held)
		{
			newColor.a = Mathf.PingPong(Time.time * 5f, 2f);
		} else
		{
			newColor.a = 1f;
		}
		image.color = newColor;

	}
	

	private void HoldButton()
	{
		image.sprite = pressedButtonSprite;
		held = true;
	}

	private void ReleaseButton()
	{
		image.sprite = defaultButtonSprite;
		held = false;
	}
}
