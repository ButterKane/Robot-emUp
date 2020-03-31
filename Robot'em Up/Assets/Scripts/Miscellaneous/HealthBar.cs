using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private RectTransform fillRect;
	public float heightOffset;
    public PawnController target;
    public Gradient healthBarGradient;
	public float fadeInSpeed = 0.5f;
	public float fadeOutSpeed = 0.1f;
    private float _initialWidth;
    private Rect _rect;
    private Camera _mainCamera;
    private RectTransform _self;
    private RawImage barBackground;
    private RawImage barFill;
	private bool activated;

	private float initialBarFillAlpha;
	private float initialBarBackgroundAlpha;


    public bool customHealthBar;
    public float customValueToCheck;
    public float customDeltaPosition = 0f;

    // Start is called before the first frame update
    void Awake()
    {
        _self = GetComponent<RectTransform>();
        _mainCamera = Camera.main;
        if (fillRect == null)
        {
            fillRect = GetComponentInChildren<RectTransform>();
        }
        _rect = fillRect.rect;
        _initialWidth = _rect.width;

        barBackground = GetComponent<RawImage>();
        barFill = fillRect.GetComponent<RawImage>();
		activated = true;
		initialBarFillAlpha = barFill.color.a;
		initialBarBackgroundAlpha = barBackground.color.a;
		ToggleHealthBar(false);
	}

    // Update is called once per frame
    void Update()
    {
        if(target != null)
        {
            if (!customHealthBar)
            {
                fillRect.sizeDelta = new Vector2(((float)target.GetHealth() / target.GetMaxHealth()) * _initialWidth, _rect.height);
            }
            else
            {
                fillRect.sizeDelta = new Vector2((customValueToCheck) * _initialWidth, _rect.height);
            }

			Color newColor = healthBarGradient.Evaluate(fillRect.sizeDelta.magnitude / _initialWidth);
			newColor.a = barFill.color.a;
			barFill.color = newColor;

			_self.position = _mainCamera.WorldToScreenPoint(target.GetHeadPosition() + new Vector3(0f, heightOffset + customDeltaPosition, 0f));
            if (target.GetHealth() <= 0)
            {
				ToggleHealthBar(false);
			}
        }   
    }

    public void ToggleHealthBar(bool _value)
    {
		if (activated == _value) { return; }
		if (_value == true)
		{
			StartCoroutine(FadeIn_C(fadeInSpeed));
		} else
		{
			StartCoroutine(FadeOut_C(fadeOutSpeed));
		}
		activated = _value;
    }

	IEnumerator FadeIn_C(float _duration)
	{
		barBackground.enabled = true;
		barFill.enabled = true;
		Color i_barColor = barFill.color;
		Color i_backgroundColor = barBackground.color;
		for (float i = 0; i < _duration; i+= Time.deltaTime)
		{
			i_barColor.a = Mathf.Lerp(0f, initialBarFillAlpha, i / _duration);
			barFill.color = i_barColor;
			i_backgroundColor.a = Mathf.Lerp(0f, initialBarBackgroundAlpha, i / _duration);
			barBackground.color = i_backgroundColor;
			yield return null;
		}
		i_barColor.a = initialBarFillAlpha;
		barFill.color = i_barColor;
		i_backgroundColor.a = initialBarBackgroundAlpha;
		barBackground.color = i_backgroundColor;
	}

	IEnumerator FadeOut_C ( float _duration )
	{
		Color i_barColor = barFill.color;
		Color i_backgroundColor = barBackground.color;
		for (float i = 0; i < _duration; i += Time.deltaTime)
		{
			i_barColor.a = Mathf.Lerp(initialBarFillAlpha, 0f, i / _duration);
			barFill.color = i_barColor;
			i_backgroundColor.a = Mathf.Lerp(initialBarBackgroundAlpha, 0f, i / _duration);
			barBackground.color = i_backgroundColor;
			yield return null;
		}
		i_barColor.a = 0f;
		barFill.color = i_barColor;
		i_backgroundColor.a = 0f;
		barBackground.color = i_backgroundColor;
		barBackground.enabled = false;
		barFill.enabled = false;
	}
}
