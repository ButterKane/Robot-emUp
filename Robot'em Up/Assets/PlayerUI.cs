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
		float healthLerpSpeed = healthLossLerpSpeed;
		if (currentHealth > displayedHealth)
		{
			healthLerpSpeed = healthGainLerpSpeed;
		}
		displayedHealth = Mathf.Lerp(displayedHealth, currentHealth, Time.deltaTime * healthLerpSpeed);
		displayedHealth = Mathf.Clamp(displayedHealth, 0f, 1f);
		healthText.text = "" + Mathf.RoundToInt((displayedHealth * 100f)).ToString() + "%";
		float evaluateTime = 1f - Mathf.Clamp(displayedHealth - (displayedHealth % healthGradientInterpolationRate), 0f, 1f);
		healthText.color = healthColorGradient.Evaluate(evaluateTime);
		float healthNormalized = (float)pawnController.GetHealth() / (float)pawnController.GetMaxHealth();
		if (healthNormalized < healthAlwaysDisplayedTreshold && healthNormalized > 0)
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
		float totalFillAmount = dashController.GetCurrentStackAmount() + (dashController.GetCurrentStackCooldown() / dashController.stackCooldown);
		for (int i = 0; i < dashStacks.Count; i++)
		{
			float fillAmount = Mathf.Clamp(totalFillAmount, 0f, 1f);
			dashStacks[i].fillAmount = fillAmount;
			totalFillAmount -= fillAmount;
		}
	}

	void GenerateCanvas()
	{
		playerCanvas = new GameObject().AddComponent<Canvas>();
		playerCanvas.transform.SetParent(this.transform);
		playerCanvas.renderMode = RenderMode.WorldSpace;
		playerCanvas.name = "PlayerCanvas";
		RectTransform canvasRT = playerCanvas.GetComponent<RectTransform>();
		canvasRT.sizeDelta = new Vector2(panelWidth, panelHeight);
		canvasRT.pivot = new Vector2(0.5f, 0f);
		playerCanvas.transform.localPosition = new Vector3(0, pawnController.GetHeight() + panelDistanceToPlayer, 0);
		playerCanvas.gameObject.AddComponent<Billboard>();
		VerticalLayoutGroup vlg = playerCanvas.gameObject.AddComponent<VerticalLayoutGroup>();
		vlg.childAlignment = TextAnchor.LowerCenter;
		vlg.childForceExpandHeight = false;
		vlg.childForceExpandWidth = false;
		vlg.childScaleHeight = true;
		vlg.childControlHeight = false;
		vlg.childControlWidth = false;
	}

	void GenerateHealthPanel()
	{
		healthPanel = new GameObject();
		healthPanel.name = "HealthPanel";
		RectTransform healthRT = healthPanel.AddComponent<RectTransform>();
		healthText = healthPanel.AddComponent<TextMeshProUGUI>();
		healthPanel.transform.SetParent(playerCanvas.transform);
		healthPanel.transform.localRotation = Quaternion.identity;
		healthPanel.transform.localPosition = Vector3.zero;
		healthRT.sizeDelta = new Vector2(panelWidth, panelWidth / 2f);
		healthRT.localScale = new Vector3(1f, 1f, 1f) * healthFontSize;
		healthText.fontSize = (int)healthFontSize;
		healthText.font = healthFont;
		healthText.alignment = TMPro.TextAlignmentOptions.Center;
		healthPanel.SetActive(false);
		healthRT.pivot = new Vector2(0.5f, 0f);
	}

	void GenerateDashBars()
	{
		dashPanel = new GameObject();
		dashPanel.name = "DashUI";
		RectTransform dashRT = dashPanel.AddComponent<RectTransform>();
		dashPanel.transform.SetParent(playerCanvas.transform);
		dashPanel.transform.localRotation = Quaternion.identity;
		dashPanel.transform.localPosition = Vector3.zero;
		dashRT.sizeDelta = new Vector2(panelWidth, 0.4f);
		HorizontalLayoutGroup hlg = dashPanel.AddComponent<HorizontalLayoutGroup>();
		hlg.childAlignment = TextAnchor.MiddleCenter;
		hlg.childForceExpandHeight = false;
		hlg.childForceExpandWidth = false;
		hlg.childScaleHeight = false;
		hlg.childScaleWidth = false;
		hlg.childControlHeight = true;
		hlg.childControlWidth = true;
		hlg.spacing = 0.1f;

		for (int i = 0; i < dashController.maxStackAmount; i++)
		{
			GameObject dashStackBackground = new GameObject();
			dashStackBackground.transform.SetParent(dashPanel.transform);
			dashStackBackground.transform.localPosition = Vector3.zero;
			dashStackBackground.name = "Dash bar BG [" + i + "]";
			Image dashStackBackgroundImage = dashStackBackground.AddComponent<Image>();
			dashStackBackgroundImage.sprite = dashBarSpriteBackground;

			GameObject dashStackFill = new GameObject();
			dashStackFill.transform.SetParent(dashStackBackground.transform);
			dashStackFill.transform.localPosition = Vector3.zero;
			dashStackFill.name = "Dash bar fill [" + i + "]";
			Image dashStackFillImage = dashStackFill.AddComponent<Image>();
			dashStackFillImage.sprite = dashBarSpriteFill;
			dashStackFillImage.color = dashBarColor;
			dashStackFillImage.type = Image.Type.Filled;
			dashStackFillImage.fillMethod = Image.FillMethod.Horizontal;
			dashStacks.Add(dashStackFillImage);
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
				} else
				{
					currentCoroutines.Add(healthPanel, StartCoroutine(DisplayPanel_C(healthPanel, healthGainShowDuration, healthGainFadeInSpeed, healthGainFadeOutSpeed, healthGainStartScale, healthGainEndScale, healthGainAnimationCurve)));
				}
				break;
			case HealthAnimationType.Loss:
				if (displayedPanels.Contains(healthPanel))
				{
					StopCoroutine(currentCoroutines[healthPanel]);
					currentCoroutines[healthPanel] = StartCoroutine(UpdatePanel_C(healthPanel, healthLossShowDuration, healthLossFadeOutSpeed, healthLossEndScale));
				} else
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
			currentCoroutines.Add(dashPanel, StartCoroutine(DisplayPanel_C(dashPanel, dashShowDuration, dashFadeInSpeed, dashFadeOutSpeed, dashStartScale, dashEndScale, dashAnimationCurve)));
		}
	}

	IEnumerator UpdatePanel_C(GameObject _panel, float _duration, float _fadeOutSpeed, float _endScale)
	{
		_panel.SetActive(true);
		Canvas.ForceUpdateCanvases();
		Image[] images = _panel.GetComponentsInChildren<Image>();
		TextMeshProUGUI[] texts = _panel.GetComponentsInChildren<TextMeshProUGUI>();
		foreach (Image image in images)
		{
			Color startColor = image.color;
			Color endColor = image.color;
			startColor.a = 1;
			endColor.a = 0;
			image.color = startColor;
		}
		foreach (TextMeshProUGUI text in texts)
		{
			Color startColor = text.color;
			Color endColor = text.color;
			startColor.a = 1;
			endColor.a = 0;
			text.color = startColor;
		}
		yield return new WaitForSeconds(_duration);
		if (panelShowedPermanently.Contains(_panel))
		{
			Debug.Log("Looping coroutine");
			StopCoroutine(currentCoroutines[_panel]);
			currentCoroutines[_panel] = StartCoroutine(UpdatePanel_C(_panel, _duration, _fadeOutSpeed, _endScale));
			yield return null;
		}
		Vector3 initialScale = _panel.transform.localScale / _endScale;
		for (float i = 0; i < 1f / _fadeOutSpeed; i += Time.deltaTime)
		{
			foreach (Image image in images)
			{
				Color startColor = image.color;
				Color endColor = image.color;
				startColor.a = 1;
				endColor.a = 0;
				image.color = Color.Lerp(startColor, endColor, i / (1f / _fadeOutSpeed));
			}
			foreach (TextMeshProUGUI text in texts)
			{
				Color startColor = text.color;
				Color endColor = text.color;
				startColor.a = 1;
				endColor.a = 0;
				text.color = Color.Lerp(startColor, endColor, i / (1f / _fadeOutSpeed));
			}
			yield return null;
		}
		_panel.transform.localScale = initialScale;
		displayedPanels.Remove(_panel);
		currentCoroutines.Remove(_panel);
		_panel.SetActive(false);
	}

	IEnumerator DisplayPanel_C(GameObject _panel, float _duration, float _fadeInSpeed, float _fadeOutSpeed, float _startScale, float _endScale, AnimationCurve _scaleCurve)
	{
		_panel.SetActive(true);
		Image[] images = _panel.GetComponentsInChildren<Image>();
		TextMeshProUGUI[] texts = _panel.GetComponentsInChildren<TextMeshProUGUI>();
		Vector3 initialScale = _panel.transform.localScale;
		_panel.transform.localScale = initialScale * _startScale;
		for (float i = 0; i < 1f/_fadeInSpeed; i+=Time.deltaTime)
		{
			_panel.transform.localScale = Vector3.Lerp(initialScale * _startScale, initialScale * _endScale, _scaleCurve.Evaluate(i / (1f / _fadeInSpeed)));
			foreach (Image image in images)
			{
				Color startColor = image.color;
				Color endColor = image.color;
				startColor.a = 0;
				endColor.a = 1;
				image.color = Color.Lerp(startColor, endColor, i / (1f/_fadeInSpeed));
			}
			foreach (TextMeshProUGUI text in texts)
			{
				Color startColor = text.color;
				Color endColor = text.color;
				startColor.a = 0;
				endColor.a = 1;
				text.color = Color.Lerp(startColor, endColor, i / (1f / _fadeInSpeed));
			}
			yield return null;
		}
		_panel.transform.localScale = initialScale * _endScale;
		displayedPanels.Add(_panel);
		currentCoroutines[_panel] = StartCoroutine(UpdatePanel_C(_panel, _duration, _fadeOutSpeed, _endScale));
	}
}
