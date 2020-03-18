using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public enum HealthAnimationType { Loss, Gain }
public class PlayerUI : MonoBehaviour
{

	//Global settings
	public bool displayHealth;
	public bool displayDashes;

	//Health settings
	public float healthGainFadeInSpeed;
	public float healthGainFadeOutSpeed;
	public float healthGainShowDuration;
	public float healthGainStartScale;
	public float healthGainEndScale;
	public AnimationCurve healthGainAnimationCurve;

	public float healthLossFadeInSpeed;
	public float healthLossFadeOutSpeed;
	public float healthLossShowDuration;
	public float healthLossStartScale;
	public float healthLossEndScale;
	public AnimationCurve healthLossAnimationCurve;

	public float healthFontSize = 200f;
	public TMPro.TMP_FontAsset healthFont;
	[Range(0f,1f)] public float healthAlwaysDisplayedTreshold = 0.5f;
	public float healthGainLerpSpeed = 1f;
	public float healthLossLerpSpeed = 1f;
	public Gradient healthColorGradient;
	[Range(0f, 1f)] public float healthGradientInterpolationRate = 0.1f;

	public GameObject healthBarPrefab;
	public float healthBarHeight = 2;
	private HealthBar healthBar;


	//Dash settings
	public float dashFadeInSpeed;
	public float dashFadeOutSpeed;
	public float dashShowDuration;
	public float dashStartScale;
	public float dashEndScale;
	public AnimationCurve dashAnimationCurve;

	public Sprite dashBarSpriteBackground;
	public Sprite dashBarSpriteFill;
	public Color dashBarColor;

	private PawnController pawnController;
	private DashController dashController;
	private Canvas playerCanvas;
	private List<Image> dashStacks;
	private TextMeshProUGUI healthText;

	private GameObject dashPanel;
	private GameObject healthPanel;

	private Dictionary<GameObject, Coroutine> currentCoroutines = new Dictionary<GameObject, Coroutine>();
	private List<GameObject> displayedPanels;
	private List<GameObject> panelShowedPermanently;
	private float currentHealth;
	private float displayedHealth;
	public RectTransform playerCanvasRectTransform;
	public RectTransform playerCanvasLateralRectTransform;

	private void Awake ()
	{
		pawnController = GetComponent<PawnController>();
		dashController = GetComponent<DashController>();
		dashStacks = new List<Image>();
		currentCoroutines = new Dictionary<GameObject, Coroutine>();
		displayedPanels = new List<GameObject>();
		panelShowedPermanently = new List<GameObject>();

		if (!pawnController) { Debug.LogWarning("No pawnController found"); return; }

		GenerateCanvas();
		GenerateHealthPanel();
		GenerateHealthBar();
		if (dashController)
		{
			GenerateDashBars();
		}
	}

	private void Update ()
	{
		UpdateDashBars();
		UpdateHealth();
	}

	void UpdateHealth()
	{
		currentHealth = (float)pawnController.GetHealth() / (float)pawnController.GetMaxHealth();
		float i_healthLerpSpeed = healthLossLerpSpeed;
		if (currentHealth > displayedHealth)
		{
			i_healthLerpSpeed = healthGainLerpSpeed;
		}
		displayedHealth = Mathf.Lerp(displayedHealth, currentHealth, Time.deltaTime * i_healthLerpSpeed);
		displayedHealth = Mathf.Clamp(displayedHealth, 0f, 1f);
		healthText.text = "" + Mathf.RoundToInt((displayedHealth * 100f)).ToString() + "%";

		float i_evaluateTime = 1f - Mathf.Clamp(displayedHealth - (displayedHealth % healthGradientInterpolationRate), 0f, 1f);
		healthText.color = healthColorGradient.Evaluate(i_evaluateTime);

		float i_HealthNormalized = (float)pawnController.GetHealth() / (float)pawnController.GetMaxHealth();
		if (i_HealthNormalized < healthAlwaysDisplayedTreshold && i_HealthNormalized > 0)
		{
			if (!panelShowedPermanently.Contains(healthPanel))
			{
				panelShowedPermanently.Add(healthPanel);
			}
		} else
		{
			panelShowedPermanently.Remove(healthPanel);
		}
	}

	void UpdateDashBars()
	{
		float i_totalFillAmount = dashController.GetCurrentStackAmount() + (dashController.GetCurrentStackCooldown() / dashController.defaultStackRecoveryDuration);
		for (int i = 0; i < dashStacks.Count; i++)
		{
			float fillAmount = Mathf.Clamp(i_totalFillAmount, 0f, 1f);
			dashStacks[i].fillAmount = fillAmount;
			i_totalFillAmount -= fillAmount;
		}
	}

	void GenerateCanvas()
	{
		playerCanvas = transform.Find("PlayerUIMain").GetComponent<Canvas>();
		playerCanvasRectTransform = playerCanvas.GetComponent<RectTransform>();
		playerCanvasLateralRectTransform = transform.Find("PlayerUILateral").GetComponentInChildren<RectTransform>();
	}

	void GenerateHealthPanel()
	{
		healthPanel = new GameObject();
		healthPanel.name = "HealthPanel";
		RectTransform i_healthRT = healthPanel.AddComponent<RectTransform>();
		healthText = healthPanel.AddComponent<TextMeshProUGUI>();
		healthPanel.transform.SetParent(playerCanvas.transform);
		healthPanel.transform.localRotation = Quaternion.identity;
		healthPanel.transform.localPosition = Vector3.zero;
		i_healthRT.sizeDelta = new Vector2(playerCanvasRectTransform.sizeDelta.x, playerCanvasRectTransform.sizeDelta.x / 2f);
		i_healthRT.localScale = new Vector3(1f, 1f, 1f) * healthFontSize;
		healthText.fontSize = (int)healthFontSize;
		healthText.font = healthFont;
		healthText.alignment = TMPro.TextAlignmentOptions.Center;
		healthPanel.SetActive(false);
		i_healthRT.pivot = new Vector2(0.5f, 0f);
	}

	void GenerateHealthBar()
	{
		healthBar = Instantiate(healthBarPrefab, GameManager.mainCanvas.transform).GetComponent<HealthBar>();
		healthBar.target = pawnController;
		healthBar.heightOffset = healthBarHeight;
		healthBar.name = "HealthBar";
	}

	void GenerateDashBars()
	{
		dashPanel = new GameObject();
		dashPanel.name = "DashUI";
		RectTransform i_dashRT = dashPanel.AddComponent<RectTransform>();
		dashPanel.transform.SetParent(playerCanvas.transform);
		dashPanel.transform.localRotation = Quaternion.identity;
		dashPanel.transform.localPosition = Vector3.zero;
		i_dashRT.sizeDelta = new Vector2(playerCanvasRectTransform.sizeDelta.x, 0.4f);
		HorizontalLayoutGroup i_hlg = dashPanel.AddComponent<HorizontalLayoutGroup>();
		i_hlg.childAlignment = TextAnchor.MiddleCenter;
		i_hlg.childForceExpandHeight = false;
		i_hlg.childForceExpandWidth = false;
		i_hlg.childScaleHeight = false;
		i_hlg.childScaleWidth = false;
		i_hlg.childControlHeight = true;
		i_hlg.childControlWidth = true;
		i_hlg.spacing = 0.1f;

		for (int i = 0; i < dashController.maxStackAmount; i++)
		{
			GameObject i_dashStackBackground = new GameObject();
			i_dashStackBackground.transform.SetParent(dashPanel.transform);
			i_dashStackBackground.transform.localPosition = Vector3.zero;
			i_dashStackBackground.name = "Dash bar BG [" + i + "]";

			Image i_dashStackBackgroundImage = i_dashStackBackground.AddComponent<Image>();
			i_dashStackBackgroundImage.sprite = dashBarSpriteBackground;

			GameObject i_dashStackFill = new GameObject();
			i_dashStackFill.transform.SetParent(i_dashStackBackground.transform);
			i_dashStackFill.transform.localPosition = Vector3.zero;
			i_dashStackFill.name = "Dash bar fill [" + i + "]";

			Image i_dashStackFillImage = i_dashStackFill.AddComponent<Image>();
			i_dashStackFillImage.sprite = dashBarSpriteFill;
			i_dashStackFillImage.color = dashBarColor;
			i_dashStackFillImage.type = Image.Type.Filled;
			i_dashStackFillImage.fillMethod = Image.FillMethod.Horizontal;
			dashStacks.Add(i_dashStackFillImage);
		}

		Canvas.ForceUpdateCanvases();
		foreach (Image stack in dashStacks)
		{
			stack.GetComponent<RectTransform>().sizeDelta = stack.transform.parent.GetComponent<RectTransform>().sizeDelta;
		}
		dashPanel.SetActive(false);

	}

	public void DisplayHealth( HealthAnimationType _type = HealthAnimationType.Gain)
	{
		switch (_type)
		{
			case HealthAnimationType.Gain:
				if (displayedPanels.Contains(healthPanel))
				{
					HideHealthBar();
					StopCoroutine(currentCoroutines[healthPanel]);
					currentCoroutines[healthPanel] = StartCoroutine(UpdatePanel_C(healthPanel, healthGainShowDuration, healthGainFadeOutSpeed, healthGainEndScale, ShowHealthBar));
				}
				else if (!currentCoroutines.ContainsKey(healthPanel))
				{
					HideHealthBar();
					currentCoroutines.Add(healthPanel, StartCoroutine(DisplayPanel_C(healthPanel, healthGainShowDuration, healthGainFadeInSpeed, healthGainFadeOutSpeed, healthGainStartScale, healthGainEndScale, healthGainAnimationCurve, ShowHealthBar)));
				}
				break;
			case HealthAnimationType.Loss:
				FeedbackManager.SendFeedback("event.PlayerHealthDecreasing", healthPanel);
				if (displayedPanels.Contains(healthPanel))
				{
					HideHealthBar();
					StopCoroutine(currentCoroutines[healthPanel]);
					currentCoroutines[healthPanel] = StartCoroutine(UpdatePanel_C(healthPanel, healthLossShowDuration, healthLossFadeOutSpeed, healthLossEndScale, ShowHealthBar));
				} else if (!currentCoroutines.ContainsKey(healthPanel))
				{
					HideHealthBar();
					currentCoroutines.Add(healthPanel, StartCoroutine(DisplayPanel_C(healthPanel, healthLossShowDuration, healthLossFadeInSpeed, healthLossFadeOutSpeed, healthLossStartScale, healthLossEndScale, healthLossAnimationCurve, ShowHealthBar)));
				}
				break;
		}
	}

	void ShowHealthBar ()
	{
		if (healthBar != null && pawnController.currentHealth < pawnController.GetMaxHealth())
		{
			healthBar.ToggleHealthBar(true);
		}
	}

	void HideHealthBar()
	{
		if (healthBar != null)
		{
			healthBar.ToggleHealthBar(false);
		}
	}

	public void DisplayDashes()
	{
		if (displayedPanels.Contains(dashPanel))
		{
			StopCoroutine(currentCoroutines[dashPanel]);
			currentCoroutines[dashPanel] = StartCoroutine(UpdatePanel_C(dashPanel, dashShowDuration, dashFadeOutSpeed, dashEndScale));
		}
		else
		{
			if (!currentCoroutines.ContainsKey(dashPanel))
			{
				currentCoroutines.Add(dashPanel, StartCoroutine(DisplayPanel_C(dashPanel, dashShowDuration, dashFadeInSpeed, dashFadeOutSpeed, dashStartScale, dashEndScale, dashAnimationCurve)));
			}
		}
	}

	IEnumerator UpdatePanel_C ( GameObject _panel, float _duration, float _fadeOutSpeed, float _endScale, Action _callBack = default )
	{
		_panel.SetActive(true);
		Canvas.ForceUpdateCanvases();
		Image[] i_images = _panel.GetComponentsInChildren<Image>();
		TextMeshProUGUI[] i_texts = _panel.GetComponentsInChildren<TextMeshProUGUI>();

		foreach (Image image in i_images)
		{
			Color startColor = image.color;
			Color endColor = image.color;
			startColor.a = 1;
			endColor.a = 0;
			image.color = startColor;
		}
		foreach (TextMeshProUGUI text in i_texts)
		{
			Color startColor = text.color;
			Color endColor = text.color;
			startColor.a = 1;
			endColor.a = 0;
			text.color = startColor;
		}
		Vector3 i_initialScale = _panel.transform.localScale / _endScale;
		yield return new WaitForSeconds(_duration);
		if (panelShowedPermanently.Contains(_panel))
		{
			StopCoroutine(currentCoroutines[_panel]);
			currentCoroutines[_panel] = StartCoroutine(UpdatePanel_C(_panel, _duration, _fadeOutSpeed, _endScale, _callBack));
			yield return null;
		}
		for (float i = 0; i < 1f / _fadeOutSpeed; i += Time.deltaTime)
		{
			foreach (Image image in i_images)
			{
				Color startColor = image.color;
				Color endColor = image.color;
				startColor.a = 1;
				endColor.a = 0;
				image.color = Color.Lerp(startColor, endColor, i / (1f / _fadeOutSpeed));
			}
			foreach (TextMeshProUGUI text in i_texts)
			{
				Color startColor = text.color;
				Color endColor = text.color;
				startColor.a = 1;
				endColor.a = 0;
				text.color = Color.Lerp(startColor, endColor, i / (1f / _fadeOutSpeed));
			}
			yield return null;
		}
		_panel.transform.localScale = i_initialScale;
		displayedPanels.Remove(_panel);
		currentCoroutines.Remove(_panel);
		_panel.SetActive(false);
		if (_callBack != default) { _callBack.Invoke(); }
	}

	IEnumerator DisplayPanel_C(GameObject _panel, float _duration, float _fadeInSpeed, float _fadeOutSpeed, float _startScale, float _endScale, AnimationCurve _scaleCurve, Action _callBack = default)
	{
		_panel.SetActive(true);
		Image[] i_images = _panel.GetComponentsInChildren<Image>();
		TextMeshProUGUI[] i_texts = _panel.GetComponentsInChildren<TextMeshProUGUI>();
		Vector3 i_initialScale = _panel.transform.localScale;

		_panel.transform.localScale = i_initialScale * _startScale;
		for (float i = 0; i < 1f/_fadeInSpeed; i+=Time.deltaTime)
		{
			_panel.transform.localScale = Vector3.Lerp(i_initialScale * _startScale, i_initialScale * _endScale, _scaleCurve.Evaluate(i / (1f / _fadeInSpeed)));
			foreach (Image image in i_images)
			{
				Color startColor = image.color;
				Color endColor = image.color;
				startColor.a = 0;
				endColor.a = 1;
				image.color = Color.Lerp(startColor, endColor, i / (1f/_fadeInSpeed));
			}
			foreach (TextMeshProUGUI text in i_texts)
			{
				Color startColor = text.color;
				Color endColor = text.color;
				startColor.a = 0;
				endColor.a = 1;
				text.color = Color.Lerp(startColor, endColor, i / (1f / _fadeInSpeed));
			}
			yield return null;
		}
		_panel.transform.localScale = i_initialScale * _endScale;
		displayedPanels.Add(_panel);
		currentCoroutines[_panel] = StartCoroutine(UpdatePanel_C(_panel, _duration, _fadeOutSpeed, _endScale, _callBack)) ;
	}
}
