using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SettingsDescriptionManaging : MonoBehaviour
{
    public TextMeshProUGUI titleTextP1;
    public TextMeshProUGUI descriptionTextP1;
    public TextMeshProUGUI titleTextP2;
    public TextMeshProUGUI descriptionTextP2;

    public void UpdateTitle(int _playerIndex, string _titleText)
    {
        switch(_playerIndex)
        {
            case 1:
                if (!string.IsNullOrEmpty(_titleText))
                    titleTextP1.text = _titleText;
                break;
            case 2:
                if (!string.IsNullOrEmpty(_titleText))
                    titleTextP2.text = _titleText;
                break;
        }
    }

    public void UpdateDescription(int _playerIndex, string _descriptionText)
    {
        switch (_playerIndex)
        {
            case 1:
                if (!string.IsNullOrEmpty(_descriptionText))
                    descriptionTextP1.text = _descriptionText;
                break;
            case 2:
                if (!string.IsNullOrEmpty(_descriptionText))
                    descriptionTextP2.text = _descriptionText;
                break;
        }
    }

}
