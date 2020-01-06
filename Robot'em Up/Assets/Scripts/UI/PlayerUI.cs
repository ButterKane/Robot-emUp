using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum HealthAnimationType { Loss, Gain }
public class PlayerUI : MonoBehaviour
{

	//Global settings
	public bool displayHealth;
	public bool displayDashes;

	public float panelWidth;
	public float panelHeight;

	public float panelDistanceToPlayer;

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
		float internal_healthLerpSpeed = healthLossLerpSpeed;
		if (currentHealth > displayedHealth)
		{
			internal_healthLerpSpeed = healthGainLerpSpeed;
		}
		displayedHealth = Mathf.Lerp(displayedHealth, currentHealth, Time.deltaTime * internal_healthLerpSpeed);
		displayedHealth = Mathf.Clamp(displayedHealth, 0f, 1f);
		healthText.text = "" + Mathf.RoundToInt((displayedHealth * 100f)).ToString() + "%";

		float internal_evaluateTime = 1f - Mathf.Clamp(displayedHealth - (displayedHealth % healthGradientInterpolationRate), 0f, 1f);
		healthText.color = healthColorGradient.Evaluate(internal_evaluateTime);

		float internal_HealthNormalized = (float)pawnController.GetHealth() / (float)pawnController.GetMaxHealth();
		if (internal_HealthNormalized < healthAlwaysDisplayedTreshold && internal_HealthNormalized > 0)
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
		float internal_totalFillAmount = dashController.GetCurrentStackAmount() + (dashController.GetCurrentStackCooldown() / dashController.stackCooldown);
		for (int i = 0; i < dashStacks.Count; i++)
		{
			float fillAmount = Mathf.Clamp(internal_totalFillAmount, 0f, 1f);
			dashStacks[i].fillAmount = fillAmount;
			internal_totalFillAmount -= fillAmount;
		}
	}

	void GenerateCanvas()
	{
		playerCanvas = new GameObject().AddComponent<Canvas>();
		playerCanvas.transform.SetParent(this.transform);
		playerCanvas.renderMode = RenderMode.WorldSpace;
		playerCanvas.name = "PlayerCanvas";
		RectTransform internal_canvasRT = playerCanvas.GetComponent<RectTransform>();
		internal_canvasRT.sizeDelta = new Vector2(panelWidth, panelHeight);
		internal_canvasRT.pivot = new Vector2(0.5f, 0f);
		playerCanvas.transform.localPosition = new Vector3(0, pawnController.GetHeight() + panelDistanceToPlayer, 0);
		playerCanvas.gameObject.AddComponent<Billboard>();
		VerticalLayoutGroup internal_vlg = playerCanvas.gameObject.AddComponent<VerticalLayoutGroup>();
		internal_vlg.childAlignment = TextAnchor.LowerCenter;
		internal_vlg.childForceExpandHeight = false;
		internal_vlg.childForceExpandWidth = false;
		internal_vlg.childScaleHeight = true;
		internal_vlg.childControlHeight = false;
		internal_vlg.childControlWidth = false;
	}

	void GenerateHealthPanel()
	{
		healthPanel = new GameObject();
		healthPanel.name = "HealthPanel";
		RectTransform internal_healthRT = healthPanel.AddComponent<RectTransform>();
		healthText = healthPanel.AddComponent<TextMeshProUGUI>();
		healthPanel.transform.SetParent(playerCanvas.transform);
		healthPanel.transform.localRotation = Quaternion.identity;
		healthPanel.transform.localPosition = Vector3.zero;
		internal_healthRT.sizeDelta = new Vector2(panelWidth, panelWidth / 2f);
		internal_healthRT.localScale = new Vector3(1f, 1f, 1f) * healthFontSize;
		healthText.fontSize = (int)healthFontSize;
		healthText.font = healthFont;
		healthText.alignment = TMPro.TextAlignmentOptions.Center;
		healthPanel.SetActive(false);
		internal_healthRT.pivot = new Vector2(0.5f, 0f);
	}

	void GenerateDashBars()
	{
		dashPanel = new GameObject();
		dashPanel.name = "DashUI";
		RectTransform internal_dashRT = dashPanel.AddComponent<RectTransform>();
		dashPanel.transform.SetParent(playerCanvas.transform);
		dashPanel.transform.localRotation = Quaternion.identity;
		dashPanel.transform.localPosition = Vector3.zero;
		internal_dashRT.sizeDelta = new Vector2(panelWidth, 0.4f);
		HorizontalLayoutGroup internal_hlg = dashPanel.AddComponent<HorizontalLayoutGroup>();
		internal_hlg.childAlignment = TextAnchor.MiddleCenter;
		internal_hlg.childForceExpandHeight = false;
		internal_hlg.childForceExpandWidth = false;
		internal_hlg.childScaleHeight = false;
		internal_hlg.childScaleWidth = false;
		internal_hlg.childControlHeight = true;
		internal_hlg.childControlWidth = true;
		internal_hlg.spacing = 0.1f;

		for (int i = 0; i < dashController.maxStackAmount; i++)
		{
			GameObject internal_dashStackBackground = new GameObject();
			internal_dashStackBackground.transform.SetParent(dashPanel.transform);
			internal_dashStackBackground.transform.localPosition = Vector3.zero;
			internal_dashStackBackground.name = "Dash bar BG [" + i + "]";

			Image internal_dashStackBackgroundImage = internal_dashStackBackground.AddComponent<Image>();
			internal_dashStackBackgroundImage.sprite = dashBarSpriteBackground;

			GameObject internal_dashStackFill = new GameObject();
			internal_dashStackFill.transform.SetParent(internal_dashStackBackground.transform);
			internal_dashStackFill.transform.localPosition = Vector3.zero;
			internal_dashStackFill.name = "Dash bar fill [" + i + "]";

			Image internal_dashStackFillImage = internal_dashStackFill.AddComponent<Image>();
			internal_dashStackFillImage.sprite = dashBarSpriteFill;
			internal_dashStackFillImage.color = dashBarColor;
			internal_dashStackFillImage.type = Image.Type.Filled;
			internal_dashStackFillImage.fillMethod = Image.FillMethod.Horizontal;
			dashStacks.Add(internal_dashStackFillImage);
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
					StopCoroutine(currentCoroutines[healthPanel]);
					currentCoroutines[healthPanel] = StartCoroutine(UpdatePanel_C(healthPanel, healthGainShowDuration, healthGainFadeOutSpeed, healthGainEndScale));
				}
				else if (!currentCoroutines.ContainsKey(healthPanel))
				{
					currentCoroutines.Add(healthPanel, StartCoroutine(DisplayPanel_C(healthPanel, healthGainShowDuration, healthGainFadeInSpeed, healthGainFadeOutSpeed, healthGainStartScale, healthGainEndScale, healthGainAnimationCurve)));
				}
				break;
			case HealthAnimationType.Loss:
				if (displayedPanels.Contains(healthPanel))
				{
					StopCoroutine(currentCoroutines[healthPanel]);
					currentCoroutines[healthPanel] = StartCoroutine(UpdatePanel_C(healthPanel, healthLossShowDuration, healthLossFadeOutSpeed, healthLossEndScale));
				} else if (!currentCoroutines.ContainsKey(healthPanel))
				{
					currentCoroutines.Add(healthPanel, StartCoroutine(DisplayPanel_C(healthPanel, healthLossShowDuration, healthLossFadeInSpeed, healthLossFadeOutSpeed, healthLossStartScale, healthLossEndScale, healthLossAnimationCurve)));
				}
				break;
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

	IEnumerator UpdatePanel_C(GameObject _panel, float _duration, float _fadeOutSpeed, float _endScale)
	{
		_panel.SetActive(true);
		Canvas.ForceUpdateCanvases();
		Image[] internal_images = _panel.GetComponentsInChildren<Image>();
		TextMeshProUGUI[] internal_texts = _panel.GetComponentsInChildren<TextMeshProUGUI>();

		foreach (Image image in internal_images)
		{
			Color startColor = image.color;
			Color endColor = image.color;
			startColor.a = 1;
			endColor.a = 0;
			image.color = startColor;
		}
		foreach (TextMeshProUGUI text in internal_texts)
		{
			Color startColor = text.color;
			Color endColor = text.color;
			startColor.a = 1;
			endColor.a = 0;
			text.color = startColor;
		}
		Vector3 internal_initialScale = _panel.transform.localScale / _endScale;
		yield return new WaitForSeconds(_duration);
		if (panelShowedPermanently.Contains(_panel))
		{
			StopCoroutine(currentCoroutines[_panel]);
			currentCoroutines[_panel] = StartCoroutine(UpdatePanel_C(_panel, _duration, _fadeOutSpeed, _endScale));
			yield return null;
		}
		for (float i = 0; i < 1f / _fadeOutSpeed; i += Time.deltaTime)
		{
			foreach (Image image in internal_images)
			{
				Color startColor = image.color;
				Color endColor = image.color;
				startColor.a = 1;
				endColor.a = 0;
				image.color = Color.Lerp(startColor, endColor, i / (1f / _fadeOutSpeed));
			}
			foreach (TextMeshProUGUI text in internal_texts)
			{
				Color startColor = text.color;
				Color endColor = text.color;
				startColor.a = 1;
				endColor.a = 0;
				text.color = Color.Lerp(startColor, endColor, i / (1f / _fadeOutSpeed));
			}
			yield return null;
		}
		_panel.transform.localScale = internal_initialScale;
		displayedPanels.Remove(_panel);
		currentCoroutines.Remove(_panel);
		_panel.SetActive(false);
	}

	IEnumerator DisplayPanel_C(GameObject _panel, float _duration, float _fadeInSpeed, float _fadeOutSpeed, float _startScale, float _endScale, AnimationCurve _scaleCurve)
	{
		_panel.SetActive(true);
		Image[] internal_images = _panel.GetComponentsInChildren<Image>();
		TextMeshProUGUI[] internal_texts = _panel.GetComponentsInChildren<TextMeshProUGUI>();
		Vector3 internal_initialScale = _panel.transform.localScale;

		_panel.transform.localScale = internal_initialScale * _startScale;
		for (float i = 0; i < 1f/_fadeInSpeed; i+=Time.deltaTime)
		{
			_panel.transform.localScale = Vector3.Lerp(internal_initialScale * _startScale, internal_initialScale * _endScale, _scaleCurve.Evaluate(i / (1f / _fadeInSpeed)));
			foreach (Image image in internal_images)
			{
				Color startColor = image.color;
				Color endColor = image.color;
				startColor.a = 0;
				endColor.a = 1;
				image.color = Color.Lerp(startColor, endColor, i / (1f/_fadeInSpeed));
			}
			foreach (TextMeshProUGUI text in internal_texts)
			{
				Color startColor = text.color;
				Color endColor = text.color;
				startColor.a = 0;
				endColor.a = 1;
				text.color = Color.Lerp(startColor, endColor, i / (1f / _fadeInSpeed));
			}
			yield return null;
		}
		_panel.transform.localScale = internal_initialScale * _endScale;
		displayedPanels.Add(_panel);
		currentCoroutines[_panel] = StartCoroutine(UpdatePanel_C(_panel, _duration, _fadeOutSpeed, _endScale));
	}
}
