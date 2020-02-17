using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Choice
{
    public Image choiceIcon;
    public string choiceText;
}

public class MultichoiceUI : UIBehaviour
{
    public Choice[] choices;
    [ReadOnly] public int selectedChoiceIndex;
    public Color selectedChoiceColor = new Color(0.5f, 0.5f, 1f, 1);
    public Color normalChoiceColor = new Color(0.1f, 0.1f, 0.1f, 1);
    public Text displayText;

    void Start()
    {
        ChangeChoice(0, 0); // Set choice[0] as selected choice
    }


    public void ChangeChoice(int _plusOrMinus, int? _overrideIndex)
    {
        foreach(var choice in choices)
        {
            choice.choiceIcon.color = normalChoiceColor;
        }

        if (_overrideIndex != null)
        {
            selectedChoiceIndex = (int)_overrideIndex;
        }
        else
        {
            selectedChoiceIndex = Mathf.Clamp(selectedChoiceIndex + _plusOrMinus, 0, choices.Length - 1);
        }

        choices[selectedChoiceIndex].choiceIcon.color = selectedChoiceColor;
        displayText.text = choices[selectedChoiceIndex].choiceText;
    }
}
