using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(PlayerPanel))]
public class DashPanel : MonoBehaviour
{
	public Sprite unfilledBar;
	public Sprite filledBar;
	public Transform dashBarHolder;
	public Color fillColor;


	private List<Image> dashBars;
	private DashController linkedDashController;
	private bool dashBarInited = false;
	private void Start ()
	{
		linkedDashController = GetComponent<PlayerPanel>().linkedPlayer.GetComponent<DashController>();
		if (linkedDashController == null) { Debug.LogWarning("No dash controller found on player, can't display related informations on UI"); Destroy(this); }
		StartCoroutine(GenerateDashBars_C());
	}

	private void Update ()
	{
		UpdateBars();
	}

	void UpdateBars()
	{
		if (!dashBarInited) { return; }
		float currentStackFill = linkedDashController.GetCurrentStackAmount() + linkedDashController.GetCurrentStackCooldown() / linkedDashController.stackCooldown;
		foreach (Image image in dashBars)
		{
			if (currentStackFill >= 1)
			{
				image.fillAmount = 1f;
				currentStackFill -= 1f;
			} else if (currentStackFill > 0)
			{
				image.fillAmount = currentStackFill;
				currentStackFill = 0;
			} else
			{
				image.fillAmount = 0;
			}
		}
	}

	IEnumerator GenerateDashBars_C ()
	{
		yield return new WaitForSeconds(0.5f);
		dashBars = new List<Image>();
		List<Image> dashBackgrounds = new List<Image>();
		for (int i = 0; i < linkedDashController.maxStackAmount; i++)
		{
			//Generate background image
			GameObject newDashBarBackground = new GameObject();
			newDashBarBackground.name = "StackBar[" + i + "]";
			Image backgroundImage = newDashBarBackground.AddComponent<Image>();
			backgroundImage.sprite = unfilledBar;
			newDashBarBackground.transform.SetParent(dashBarHolder);
			backgroundImage.GetComponent<RectTransform>().sizeDelta = new Vector2(unfilledBar.rect.width, unfilledBar.rect.height);
			newDashBarBackground.transform.localScale = Vector3.one * 0.25f;
			dashBackgrounds.Add(backgroundImage);
		}

		for (int i = 0; i < dashBackgrounds.Count; i++)
		{
			//Generate foreground image
			GameObject newDashBarFill = new GameObject();
			newDashBarFill.name = "Fill[" + i + "]";
			Image fillImage = newDashBarFill.AddComponent<Image>();
			fillImage.sprite = filledBar;
			fillImage.type = Image.Type.Filled;
			fillImage.fillMethod = Image.FillMethod.Horizontal;
			fillImage.color = fillColor;
			newDashBarFill.transform.SetParent(dashBackgrounds[i].transform);
			newDashBarFill.transform.localScale = Vector3.one;
			newDashBarFill.transform.localPosition = Vector3.zero;
			yield return new WaitForEndOfFrame();
			newDashBarFill.GetComponent<RectTransform>().sizeDelta = dashBackgrounds[i].GetComponent<RectTransform>().sizeDelta;
			dashBars.Add(fillImage);

			//dashBackgrounds[i].transform.DOShakeScale(0.1f, 0.1f);
			yield return new WaitForSeconds(0.1f);
			dashBackgrounds[i].transform.localScale = Vector3.one * 0.25f;
		}
		yield return null;
		dashBarInited = true;
	}
}
