using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleUI : UIBehaviour
{
    public bool isYes;
    private bool selectionIsYes;
    public Image yesButton;
    public Image noButton;
    public Image currentSelectionHighlight;
    public Color selectedButtonColor = new Color(0.5f, 0.5f, 1f, 1);
    public Color normalButtonColor = new Color(0.1f, 0.1f, 0.1f, 1);

    private void Start()
    {
        ColorizeButton();
    }

    private void Update()
    {
        if (isSelected)
        {
            UpdateSelectionVisual();
        }
        else if (isSelected == false && currentSelectionHighlight.enabled == true)
        {
            currentSelectionHighlight.enabled = false;
        }
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

    public void UpdateSelectionVisual()
    {
        switch (selectionIsYes)
        {
            case true:
                currentSelectionHighlight.transform.position = yesButton.transform.position;
                break;
            case false:
                currentSelectionHighlight.transform.position = noButton.transform.position;
                break;
        }
    }

    public void ConfirmSelection()
    {
        isYes = selectionIsYes;
        ColorizeButton();
    }
}
