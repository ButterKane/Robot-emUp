using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderUI : UIBehaviour
{
    public Slider slider;
    public Text valueText;
    private float defaultTimeBetweenChangeValue;
    private float timeBetweenValueChange;
    private float currentTimeProgressionBeforeValueChange;

    // Start is called before the first frame update
    void Start()
    {
        timeBetweenValueChange = defaultTimeBetweenChangeValue;
        currentTimeProgressionBeforeValueChange = 0;
        UpdateSliderText();
    }


    public void UpdateSliderText()
    {
        valueText.text = slider.value.ToString() ;
    }

    public void UpdateSliderValue(int _valueToAdd)
    {
        if (currentTimeProgressionBeforeValueChange <= 0)
        {
            slider.value += (float)_valueToAdd / 100;
            currentTimeProgressionBeforeValueChange = timeBetweenValueChange;
            UpdateSliderText();
        }

        currentTimeProgressionBeforeValueChange -= Time.deltaTime;

    }
}
