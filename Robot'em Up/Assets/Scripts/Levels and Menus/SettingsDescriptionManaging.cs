using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SettingsDescriptionManaging : MonoBehaviour
{
    public TextMeshProUGUI titleTextP1;
    public TextMeshProUGUI descriptionTextP1;

    public void UpdateTitle(string _titleText)
    {
        if (!string.IsNullOrEmpty(_titleText))
            titleTextP1.text = _titleText;
    }

    public void UpdateDescription(string _descriptionText)
    {
        if (!string.IsNullOrEmpty(_descriptionText))
            descriptionTextP1.text = _descriptionText;
    }

}
