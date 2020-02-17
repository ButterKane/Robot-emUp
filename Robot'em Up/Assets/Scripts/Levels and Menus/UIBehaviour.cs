using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBehaviour : MonoBehaviour
{
    public GameObject settingsArea;
    private Image background;
    public Color selectedColor = new Color (0.5f, 0.5f, 0.5f, 0.7f);
    public Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.7f);
    [ReadOnly] public bool isSelected;

    private void Awake()
    {
        background = settingsArea.GetComponent<Image>();
    }

    public void SelectSettingsArea()
    {
        isSelected = true;
    }
}
