using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using XInputDotNetPure;
using DG.Tweening;

public class AbilityMenu : MonoBehaviour
{
	public static AbilityMenu instance;
	[Header("Settings")]
	[SerializeField] private string upgradeLockedText = "This upgrade is locked";

	[SerializeField] private Color unselectedAbilityColor = Color.white;
	[SerializeField] private Color selectedAbilityColor = Color.red;

	[SerializeField] private Color lockedAbilityColor = Color.grey;
	[SerializeField] private Color unlockedAbilityColor = Color.white;

	[Header("References")]
	[SerializeField] private List<AbilityGroupData> abilities = default;
	[SerializeField] private TextMeshProUGUI abilityName = default;
	[SerializeField] private TextMeshProUGUI abilityDescription = default;
	[SerializeField] private TextMeshProUGUI firstUpgradeDescription = default;
	[SerializeField] private TextMeshProUGUI secondUpgradeDescription = default;
	[SerializeField] private TextMeshProUGUI upgradeHeader = default;

	[SerializeField] private Image GIF = default;

	[SerializeField] private Image selectorOutline = default;
	[SerializeField] private Image selectorArrow = default;

	private Canvas canvas;
	private bool opened;
	private Sprite[] currentGifImages;
	private int currentGifIndex;

	private bool waitForStartReset;
	private bool waitForBReset;
	private bool waitForJoystickReset;

	private List<AbilityGroupData> unlockedAbilities = new List<AbilityGroupData>();
	private int selectedAbilityIndex;
	private bool canInteract;
	private void Awake ()
	{
		instance = this;
		canvas = GetComponent<Canvas>();
		canInteract = true;
	}

	private void Update()
	{
		if (LoadingScreen.loading) return;
		UpdateGIF();
		GetInputs();
	}

	private void GetInputs()
	{
		GamePadState i_state = GamePad.GetState(PlayerIndex.One);
		if (opened && canInteract)
		{
			if (i_state.Buttons.Start == ButtonState.Pressed)
			{
				if (waitForStartReset) return;
				Close();
				waitForStartReset = true;
			}
			else
			{
				waitForStartReset = false;
			}
			if (i_state.Buttons.B == ButtonState.Pressed)
			{
				if (waitForBReset) return;
				Close();
				waitForBReset = true;
			}
			else
			{
				waitForBReset = false;
			}
			if (i_state.ThumbSticks.Left.Y > 0.3f)
			{
				if (waitForJoystickReset) return;
				SelectPreviousAbility();
				waitForJoystickReset = true;
			}
			else if (i_state.ThumbSticks.Left.Y < -0.3f)
			{
				if (waitForJoystickReset) return;
				SelectNextAbility();
				waitForJoystickReset = true;
			}
			else
			{
				waitForJoystickReset = false;
			}
		}
	}

	public void Open()
	{
		if (opened) return;
		transform.localScale = Vector3.one;
		FeedbackManager.SendFeedback("event.PressSettings", this);
		IngameMenu.instance.CloseMainPanel();
		canvas.enabled = true;
		opened = true;
		UpdateAbilityList();
		selectedAbilityIndex = 0;
		DisplayAbilityInformation(abilities[selectedAbilityIndex]);
	}

	public void Close ()
	{
		FeedbackManager.SendFeedback("event.MenuBack", this);
		IngameMenu.instance.OpenMainPanel();
		canvas.enabled = false;
		opened = false;
	}

	private void SelectNextAbility()
	{
		FeedbackManager.SendFeedback("event.MenuUpAndDown", this);
		selectedAbilityIndex++;
		if (selectedAbilityIndex >= unlockedAbilities.Count)
		{
			selectedAbilityIndex = 0;
		}
		DisplayAbilityInformation(unlockedAbilities[selectedAbilityIndex]);
	}

	private void SelectPreviousAbility()
	{
		FeedbackManager.SendFeedback("event.MenuUpAndDown", this);
		selectedAbilityIndex--;
		if (selectedAbilityIndex < 0)
		{
			selectedAbilityIndex = unlockedAbilities.Count - 1;
		}
		DisplayAbilityInformation(unlockedAbilities[selectedAbilityIndex]);
	}

	private void UpdateGIF()
	{
		if (!opened) return;
		if (currentGifImages == null || currentGifImages.Length <= 0) return;
		if (currentGifIndex < currentGifImages.Length - 1)
		{
			currentGifIndex++;
		} else
		{
			currentGifIndex = 0;
		}
		GIF.sprite = currentGifImages[currentGifIndex];
	}
	private void UpdateAbilityList()
	{
		unlockedAbilities = new List<AbilityGroupData>();
		foreach (AbilityGroupData ab in abilities) 
		{
			Upgrade currentAbilityUpgrade = AbilityManager.GetAbilityLevel(ab.ability);
			if (currentAbilityUpgrade != Upgrade.Locked) 
			{ 
				unlockedAbilities.Add(ab);
				ab.text.color = unlockedAbilityColor;
			} 
			else
			{
				ab.text.color = lockedAbilityColor;
				ab.transform.SetAsLastSibling();
			}
		}
		for (int i = 0; i < unlockedAbilities.Count; i++)
		{
			unlockedAbilities[i].transform.SetSiblingIndex(i);
		}
	}

	public void HighlightUpgrade(ConcernedAbility ability, Upgrade upgrade)
	{
		StartCoroutine(HighlightUpgrade_C(ability, upgrade));
	}

	IEnumerator HighlightUpgrade_C(ConcernedAbility ability, Upgrade upgrade)
	{
		yield return new WaitForEndOfFrame();
		canInteract = false;
		foreach (AbilityGroupData ab in abilities)
		{
			if (ab.ability == ability)
			{
				DisplayAbilityInformation(ab);
			}
		}
		switch (upgrade)
		{
			case Upgrade.Base:
				abilityDescription.GetComponentInChildren<Image>().DOFade(0.5f, 0.6f).SetLoops(6, LoopType.Yoyo).SetEase(Ease.OutSine).OnComplete(AllowNavigation).SetUpdate(true);
				break;
			case Upgrade.Upgrade1:
				firstUpgradeDescription.GetComponentInChildren<Image>().DOFade(0.5f, 0.6f).SetLoops(6, LoopType.Yoyo).SetEase(Ease.OutSine).OnComplete(AllowNavigation).SetUpdate(true);
				break;
			case Upgrade.Upgrade2:
				secondUpgradeDescription.GetComponentInChildren<Image>().DOFade(0.5f, 0.6f).SetLoops(6, LoopType.Yoyo).SetEase(Ease.OutSine).OnComplete(AllowNavigation).SetUpdate(true);
				break;
		}
	}

	private void AllowNavigation()
	{
		canInteract = true;
	}

	private void DisplayAbilityInformation(AbilityGroupData abilityData)
	{
		foreach (AbilityGroupData ad in unlockedAbilities)
		{
			ad.text.color = unselectedAbilityColor;
		}
		abilityData.text.color = selectedAbilityColor;

		selectedAbilityIndex = abilityData.transform.GetSiblingIndex();
		abilityName.text = abilityData.abilityName;
		abilityDescription.text = abilityData.abilityDescription;

		//Is ability upgraded ?
		Upgrade currentAbilityUpgrade = AbilityManager.GetAbilityLevel(abilityData.ability);

		//Hide "Upgrade" text if there is no upgrade for this ability
		if (abilityData.upgrade1Description == "")
		{
			upgradeHeader.alpha = 0;
		} else
		{
			upgradeHeader.alpha = 1;
		}

		//Update upgrade descriptions and colors
		switch (currentAbilityUpgrade)
		{
			case Upgrade.Locked:
				//First and second descriptions are locked
				firstUpgradeDescription.text = abilityData.upgrade1Description == "" ? "" : upgradeLockedText;
				secondUpgradeDescription.text = abilityData.upgrade2Description == "" ? "" : upgradeLockedText;
				firstUpgradeDescription.color = lockedAbilityColor;
				secondUpgradeDescription.color = lockedAbilityColor;
				break;
			case Upgrade.Base:
				//First and second descriptions are locked
				firstUpgradeDescription.text = abilityData.upgrade1Description == "" ? "" : upgradeLockedText;
				secondUpgradeDescription.text = abilityData.upgrade2Description == "" ? "" : upgradeLockedText;
				firstUpgradeDescription.color = lockedAbilityColor;
				secondUpgradeDescription.color = lockedAbilityColor;
				break;
			case Upgrade.Upgrade1:
				//Second descriptions locked
				firstUpgradeDescription.text = abilityData.upgrade1Description;
				secondUpgradeDescription.text = abilityData.upgrade2Description == "" ? "" : upgradeLockedText;
				firstUpgradeDescription.color = unlockedAbilityColor;
				secondUpgradeDescription.color = lockedAbilityColor;
				break;
			case Upgrade.Upgrade2:
				//No description locked
				firstUpgradeDescription.text = abilityData.upgrade1Description;
				secondUpgradeDescription.text = abilityData.upgrade2Description;
				firstUpgradeDescription.color = unlockedAbilityColor;
				secondUpgradeDescription.color = unlockedAbilityColor;
				break;
		}

		//Update selector
		selectorArrow.transform.position = abilityData.transform.position;
		selectorOutline.transform.position = abilityData.transform.position;
		selectorOutline.rectTransform.transform.localScale = new Vector3(0, selectorOutline.rectTransform.transform.localScale.y, 1);
		selectorOutline.rectTransform.DOScaleX(1, 0.2f).SetUpdate(true).SetEase(Ease.OutSine);

		//Update GIF
		currentGifImages = abilityData.gifImages;
		currentGifIndex = 0;
	}
}
