using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderUI : UIBehaviour
{
    public Slider slider;
    public int sliderValue;
    public Text valueText;

    // Start is called before the first frame update
    void Start()
    {
        slider.value = sliderValue;
        UpdateSliderText();
    }


    public void UpdateSliderText()
    {
        valueText.text = sliderValue.ToString() ;
    }
}
