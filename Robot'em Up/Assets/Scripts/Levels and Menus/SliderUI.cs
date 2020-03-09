using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderUI : UIBehaviour
{
    public Slider slider;
    [Range(0, 100)] public int defaultValue = 50;
    public int currentValue;
    public TextMeshProUGUI valueText;
    private float defaultTimeBetweenChangeValue;
    private float timeBetweenValueChange;
    private float currentTimeProgressionBeforeValueChange;

    // Start is called before the first frame update
    void Start()
    {
        slider.value = (float)defaultValue / 100;
        currentValue = (int) slider.value * 100;
        timeBetweenValueChange = defaultTimeBetweenChangeValue;
        currentTimeProgressionBeforeValueChange = 0;
        UpdateSliderText();
    }

    private void Update()
    {
        if (System.Math.Round(slider.value, 2) != System.Math.Round((float)defaultValue / 100, 2))
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
        valueText.text = Mathf.RoundToInt(slider.value *100).ToString() ;
    }

    public void UpdateSliderValue(int _valueToAdd, bool _valueIsAdded)
    {
        if (_valueIsAdded)
        {
            if (currentTimeProgressionBeforeValueChange <= 0)
            {
                slider.value += (float)_valueToAdd / 100;
                currentValue = (int)slider.value * 100;
                currentTimeProgressionBeforeValueChange = timeBetweenValueChange;
                UpdateSliderText();
            }
            currentTimeProgressionBeforeValueChange -= Time.unscaledDeltaTime;
        }
        else
        {
            slider.value = (float)_valueToAdd / 100;
            currentValue = (int)slider.value * 100;
            currentTimeProgressionBeforeValueChange = timeBetweenValueChange;
        }
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
        slider.value = (float)defaultValue / 100;
        timeBetweenValueChange = defaultTimeBetweenChangeValue;
        currentTimeProgressionBeforeValueChange = 0;
        UpdateSliderText();
    }

    public void ForceModifyValue(int _value)
    {
        UpdateSliderValue(_value, false);
    }
}
