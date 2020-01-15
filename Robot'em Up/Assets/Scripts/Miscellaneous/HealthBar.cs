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
    private float _initialWidth;
    private Rect _rect;
    private Camera _mainCamera;
    private RectTransform _self;
    private RawImage BarBackground;
    private RawImage BarFill;
	private bool activated;

    // Start is called before the first frame update
    void Start()
    {
        _self = GetComponent<RectTransform>();
        _mainCamera = Camera.main;
        if (fillRect == null)
        {
            fillRect = GetComponentInChildren<RectTransform>();
        }
        _rect = fillRect.rect;
        _initialWidth = _rect.width;

        BarBackground = GetComponent<RawImage>();
        BarFill = fillRect.GetComponent<RawImage>();
		activated = true;
		ToggleHealthBar(false);
	}

    // Update is called once per frame
    void Update()
    {
        if(target != null)
        {
            fillRect.sizeDelta = new Vector2(((float)target.currentHealth / target.maxHealth) * _initialWidth, _rect.height);
            BarFill.color = healthBarGradient.Evaluate(fillRect.sizeDelta.magnitude / _initialWidth);
            _self.position = _mainCamera.WorldToScreenPoint(target.GetHeadPosition() + new Vector3(0f, heightOffset,0f));
            if (target.currentHealth <= 0)
            {
				ToggleHealthBar(false);
			}
        }   
    }

    public void ToggleHealthBar(bool _value)
    {
		if (activated == _value) { return; }
        BarBackground.enabled = _value;
        BarFill.enabled = _value;
		activated = _value;
    }
}
