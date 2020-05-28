using MyBox;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderUI : UIBehaviour
{
    [Separator("References (don't touch)")]
    public Slider slider;
    public TextMeshProUGUI valueText;
    public TextMeshProUGUI minValueText;
    public TextMeshProUGUI maxValueText;

    [Separator ("Tweakable variables")]
    public int minValue = 0;
    public int maxValue = 100;
    public int defaultValue = 50;
    [ReadOnly] public int currentValue;        // This is the actual value, not the 0-1 of the slider
    public float defaultTimeBetweenChangeValue;

    private float currentTimeProgressionBeforeValueChange;


    // Start is called before the first frame update
    void Start()
    {
        slider.value = defaultValue;
        currentValue = (int)slider.value;
        currentTimeProgressionBeforeValueChange = 0;
        UpdateSliderText();
    }

    private void Update()
    {
        if (currentValue != defaultValue)
        {
            ToggleChangeIcon(true);
        }
        else
        {
            ToggleChangeIcon(false);
        }
    }


    public void UpdateSliderText()
    {
        valueText.text = Mathf.RoundToInt(slider.value).ToString() ;
    }

    public void UpdateSliderValue(int _valueToAdd, bool _valueIsAdded)
    {
        if (_valueIsAdded)
        {
            if (currentTimeProgressionBeforeValueChange <= 0) // from max to min
            {
                slider.value += _valueToAdd;
                slider.value = Mathf.RoundToInt(slider.value);
                currentValue = (int)slider.value;
                currentTimeProgressionBeforeValueChange = defaultTimeBetweenChangeValue;
            }
            currentTimeProgressionBeforeValueChange -= Time.unscaledDeltaTime;
        }
        else
        {
            slider.value = _valueToAdd;
            currentValue = (int)slider.value;
            currentTimeProgressionBeforeValueChange = 0;
        }
        UpdateSliderText();
    }

    public override void IncreaseValue()
    {
        UpdateSliderValue(1, true);
    }

    public override void DecreaseValue()
    {
        UpdateSliderValue(-1, true);
    }

    public override void ResetValueToDefault()
    {
        slider.value = defaultValue;
        currentTimeProgressionBeforeValueChange = 0;
        UpdateSliderText();
    }

    public void ForceModifyValue(int _value)
    {
        UpdateSliderValue(_value, false);
    }

    /// <summary>
    /// Attribute the texts AND the slider min and max values
    /// </summary>
    public void AttributeMinMaxValues()
    {
        minValueText.text = minValue.ToString();
        maxValueText.text = maxValue.ToString();

        slider.minValue = minValue;
        slider.maxValue = maxValue;

        slider.value = defaultValue;
        currentValue = (int)slider.value;

        valueText.text = currentValue.ToString();
    }
}
