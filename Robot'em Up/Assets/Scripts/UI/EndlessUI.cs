using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndlessUI : MonoBehaviour
{
	public static EndlessUI instance;
	public GameObject ToShow;
	private bool needTobeUpdated;

	public TMPro.TextMeshProUGUI scoreText;
	public TMPro.TextMeshProUGUI timeText;
	private void Awake ()
	{
		instance = this;

		ToShow.SetActive(false);
		needTobeUpdated = false;
	}

	public void HideEndlessUI()
	{
		ToShow.SetActive(false);
		needTobeUpdated = false;
	}

	public void ShowUI()
	{
		ToShow.SetActive(true);
		needTobeUpdated = true;
	}

	public void Update()
	{
        if (needTobeUpdated)
        {
			scoreText.text = GameManager.currentScore.ToString();
			string minutes = Mathf.Floor(GameManager.timeInZone / 60).ToString("00");
			string seconds = (GameManager.timeInZone % 60).ToString("00");
			timeText.text = string.Format("{0}:{1}", minutes, seconds);
		}
	}

}
