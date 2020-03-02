using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleUI : UIBehaviour
{
    public bool defaultValueIsYes = true;
    [ReadOnly] public bool buttonIsYes;
    private bool selectionIsYes;
    public Image yesButton;
    public Image noButton;
    public Image currentSelectionHighlight;
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
            UpdateSelectionVisual();
        }
        else if (isSelected == false && currentSelectionHighlight.enabled == true)
        {
            currentSelectionHighlight.enabled = false;
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

    public void UpdateSelectionVisual()
    {
        currentSelectionHighlight.enabled = true;

        switch (selectionIsYes)
        {
            case true:
                currentSelectionHighlight.transform.localPosition = yesButton.transform.localPosition;
                break;
            case false:
                currentSelectionHighlight.transform.localPosition = noButton.transform.localPosition;
                break;
        }
    }

    public void ConfirmSelection()
    {
        buttonIsYes = selectionIsYes;
        ColorizeButton();
    }

    public override void IncreaseValue()
    {
        selectionIsYes = !selectionIsYes;
    }

    public override void DecreaseValue()
    {
        selectionIsYes = !selectionIsYes;
    }

    public override void PressingA()
    {
        ConfirmSelection();
    }

    public override void ResetValueToDefault()
    {
        buttonIsYes = defaultValueIsYes;
        ColorizeButton();
    }
}
