using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using XInputDotNetPure;

public class Disclaimer : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Image background;
	[SerializeField] private TextMeshProUGUI textA;
	[SerializeField] private TextMeshProUGUI textB;
	[SerializeField] private Image imageSkip;
	[SerializeField] private TextMeshProUGUI textSkip;
	[SerializeField] private MusicLoader mloader;
	[SerializeField] private MainMenu menu;

	[Header("Settings")]
	[SerializeField] private float delayBeforeSkipping;

	private bool opened = false;
	private bool canSkip = false;

	private float openedDuration = 0;

	private bool skipPanelShown = false;


	private void Awake ()
	{
		Time.timeScale = 1f;
		bool hadDisclaimer = PlayerPrefs.GetInt("Disclaimer") == 1 ? true : false;
		if (hadDisclaimer)
		{
			mloader.LoadMusic();
			Destroy(this.gameObject);
		} else
		{
			OpenDisclaimer();
		}
	}

	private void OpenDisclaimer ()
	{
		opened = true;
		menu.gameObject.SetActive(false);
		background.DOFade(1f, 0f).SetUpdate(true);
		textA.DOFade(1f, 1f).SetEase(Ease.OutSine).SetUpdate(true);
		textB.DOFade(1f, 1f).SetEase(Ease.OutSine).SetUpdate(true);
	}

	private void CloseDisclaimer()
	{
		background.DOFade(0, 1f).SetEase(Ease.OutSine).SetUpdate(true);
		textA.DOFade(0, 1f).SetEase(Ease.OutSine).SetUpdate(true);
		textB.DOFade(0, 1f).SetEase(Ease.OutSine).SetUpdate(true);
		imageSkip.DOFade(0, 1f).SetEase(Ease.OutSine).SetUpdate(true);
		textSkip.DOFade(0, 1f).SetEase(Ease.OutSine).SetUpdate(true);
		menu.gameObject.SetActive(true);
		mloader.LoadMusic();
		opened = false;
		PlayerPrefs.SetInt("Disclaimer", 1);
	}

	private void Update ()
	{
		GamePadState i_state = GamePad.GetState(PlayerIndex.One);
		if (canSkip)
		{
			if (i_state.Buttons.A == ButtonState.Pressed)
			{
				CloseDisclaimer();
			}
		}
		if (opened)
		{
			if (!skipPanelShown)
			{
				openedDuration += Time.deltaTime;
				if (openedDuration >= delayBeforeSkipping)
				{
					ShowSkipPanel();
				}
			}
		}
	}

	private void ShowSkipPanel()
	{
		skipPanelShown = true;
		imageSkip.DOFade(1, 1f).SetEase(Ease.OutSine).SetUpdate(true);
		textSkip.DOFade(1, 1f).SetEase(Ease.OutSine).SetUpdate(true);
		canSkip = true;
	}
}
