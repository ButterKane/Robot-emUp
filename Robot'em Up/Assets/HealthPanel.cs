using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HealthPanel : MonoBehaviour
{
	public PlayerController linkedPlayer;
	public Sprite unfilledBar;
	public Sprite filledBar;
	public int barCount;


	public Transform healthBarHolder;
	private List<Image> healthBars;
	private bool healthBarInited = false;
	private void Awake ()
	{
		StartCoroutine(GenerateHealthBars_C());
	}

	private void Update ()
	{
		UpdateHealthBars();
	}

	void UpdateHealthBars()
	{
		if (linkedPlayer != null && healthBarInited)
		{
			float playerHealth = (float)linkedPlayer.GetHealth() / (float)linkedPlayer.GetMaxHealth() * (float)barCount;
			int filledBarCount = Mathf.CeilToInt(playerHealth);
			for (int i = 0; i < barCount; i++)
			{
				if (i < filledBarCount)
				{
					healthBars[i].sprite = filledBar;
				} else
				{
					healthBars[i].sprite = unfilledBar;
				}
			}
		}
	}

	IEnumerator GenerateHealthBars_C()
	{
		healthBars = new List<Image>();
		for (int i = 0; i < barCount; i++)
		{
			GameObject newHealthBar = new GameObject();
			newHealthBar.name = "HealthPoint";
			Image newImage = newHealthBar.AddComponent<Image>();
			newImage.sprite = filledBar;
			healthBars.Add(newImage);
			newHealthBar.transform.SetParent(healthBarHolder);
			newImage.GetComponent<RectTransform>().sizeDelta = new Vector2(unfilledBar.rect.width, unfilledBar.rect.height);
			newHealthBar.transform.localScale = Vector3.one * 0.25f;
			newHealthBar.transform.DOShakeScale(0.1f, 0.1f);
			yield return new WaitForSeconds(0.1f);
			newHealthBar.transform.localScale = Vector3.one * 0.25f;
		}
		yield return null;
		healthBarInited = true;
	}
}
