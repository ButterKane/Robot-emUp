using MyBox;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBehaviour : MonoBehaviour
{
    public GameObject settingsArea;
    public TextMeshProUGUI boxTitle;
    private Image background;
    public Color selectedColor = new Color (0.5f, 0.5f, 0.5f, 0.7f);
    public Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.7f);
    [ReadOnly] public bool isSelected;
    public int titleFontSize = 20;
    public string settingsTitle;
    public int descriptionFontSize = 15;
    [TextArea(5,10)]
    public string settingsDescription;
    public Image changeIcon;
    

    private void Awake()
    {
        background = settingsArea.GetComponent<Image>();
        if (!settingsTitle.IsNullOrEmpty())
            boxTitle.text = settingsTitle;
    }

    public UIBehaviour SelectThisSetting()
    {
        isSelected = true;
        background.color = selectedColor;
        return this;
    }

    public UIBehaviour UnselectThisSetting()
    {
        isSelected = false;
        background.color = normalColor;
        return this;
    }

    public virtual void IncreaseValue()
    {

    }

    public virtual void DecreaseValue()
    {

    }

    public virtual void PressingA()
    {

    }

    public virtual void ResetValueToDefault()
    {

    }

    public void ToggleChangeIcon(bool _activate = true)
    {
        if (!_activate)
        {
            changeIcon.enabled = false;
        }
        else
        {
            changeIcon.enabled = true;
        }
    }
}
