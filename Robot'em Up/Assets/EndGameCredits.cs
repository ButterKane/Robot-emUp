using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XInputDotNetPure;
public class EndGameCredits : MonoBehaviour
{
	public Image blurPanel;
	private bool active = false;
	private bool allowMenuComeback;
	public static EndGameCredits instance;
	private void Awake ()
	{
		instance = this;
		blurPanel.material = new Material(blurPanel.material);
	}

	public static void DisplayEndGameCredits(float _delay)
	{
		if (instance.active) { return; }
		instance.StartCoroutine(instance.DisplayCreditsAfterDelay_C(_delay));
	}

	public void AllowMenuComeback()
	{
		allowMenuComeback = true;
	}
	public void BlurScreen()
	{
		StartCoroutine(BlurScreen_C());
	}

	public void PlayMusic()
	{
		MusicManager.PlayMusic("MenuTheme");
	}

	IEnumerator DisplayCreditsAfterDelay_C(float _delay)
	{
		yield return new WaitForSeconds(_delay);
		instance.active = true;
		instance.GetComponent<Animator>().SetTrigger("EndGame");
	}
	IEnumerator BlurScreen_C()
	{
		GameManager.playerOne.Freeze();
		GameManager.playerTwo.Freeze();
		for (float i = 0; i < 1f; i+=Time.deltaTime)
		{
			float blurValue = Mathf.Lerp(0f, 2f, i / 1f);
			blurPanel.material.SetFloat("_Size", blurValue);
			yield return null;
		}
	}

	private void Update ()
	{
		if (active && allowMenuComeback)
		{
			GamePadState i_state = GamePad.GetState(PlayerIndex.One);
			for (int i = 0; i < 2; i++)
			{
				if (i == 0) { i_state = GamePad.GetState(PlayerIndex.One); }
				if (i == 1) { i_state = GamePad.GetState(PlayerIndex.Two); }
				if (i_state.Buttons.B == ButtonState.Pressed || i_state.Buttons.Back == ButtonState.Pressed)
				{
					GameManager.playerOne.UnFreeze();
					GameManager.playerTwo.UnFreeze();
					MusicManager.StopMusic();
					GameManager.LoadSceneByIndex(0);
				}
			}
		}
	}
}
