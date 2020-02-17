using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleUI : UIBehaviour
{
    public bool isYes;
    public Image yesButton;
    public Image noButton;
    public Color selectedButtonColor = new Color(0.5f, 0.5f, 1f, 1);
    public Color normalButtonColor = new Color(0.1f, 0.1f, 0.1f, 1);

    private void Start()
    {
        ColorizeButton();
    }

    public void ColorizeButton()
    {
        switch(isYes)
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
}
