﻿using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleUI : UIBehaviour
{
    [Separator("References (don't touch)")]
    public Image yesButton;
    public Image noButton;

    [Separator("Tweakable Variables")]
    public bool defaultValueIsYes = true;
    [ReadOnly] public bool buttonIsYes = true;
    public Color selectedButtonColor = new Color(0.5f, 0.5f, 1f, 1);
    public Color normalButtonColor = new Color(0.1f, 0.1f, 0.1f, 1);

    private void Start()
    {
        buttonIsYes = defaultValueIsYes;
        ColorizeButton();
    }

    private void Update()
    {
        if (isSelected)
        {
            ColorizeButton();
        }

        if (buttonIsYes != defaultValueIsYes)
        {
            ToggleChangeIcon(true);
        }
        else
        {
            ToggleChangeIcon(false);
        }
    }

    public void ColorizeButton()
    {
        switch(buttonIsYes)
        {
            case true:
                yesButton.color = selectedButtonColor;
                noButton.color = normalButtonColor;
                break;
            case false:
                yesButton.color = normalButtonColor;
                noButton.color = selectedButtonColor;
                break;
                
        }
    }


    public override void IncreaseValue()
    {
        buttonIsYes = !buttonIsYes;
    }

    public override void DecreaseValue()
    {
        buttonIsYes = !buttonIsYes;
    }

    public override void ResetValueToDefault()
    {
        buttonIsYes = defaultValueIsYes;
        ColorizeButton();
    }

    public void ForceModifyValue(bool _value)
    {
        buttonIsYes = _value;
        ColorizeButton();
    }
}
