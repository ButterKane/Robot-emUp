﻿using MyBox;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public enum InputType { None, Button, Axis };

[System.Serializable]
public class InputDisplay
{
    public string inputName;
    public InputType inputType = InputType.None;
    [ConditionalFieldAttribute(nameof(inputType), false, InputType.Button)] public ButtonAction buttonInfoPlayer1;
    [ConditionalFieldAttribute(nameof(inputType), false, InputType.Button)] public ButtonAction buttonInfoPlayer2;
    [ConditionalFieldAttribute(nameof(inputType), false, InputType.Axis)] public AxisAction axisInfoPlayer1;
    [ConditionalFieldAttribute(nameof(inputType), false, InputType.Axis)] public AxisAction axisInfoPlayer2;
}

[ExecuteInEditMode]
public class SettingsInputGroup : MonoBehaviour
{
    [Separator("References (don't touch)")]
    public TextMeshProUGUI nameTMP;
    public TextMeshProUGUI inputP1TMP;
    public TextMeshProUGUI inputP2TMP;

    [Separator("Input Managed")]
    [SerializeField] public InputDisplay actionAndInputs;

    private void Update()
    {
        UpdateTexts();
    }

    void UpdateTexts()
    {
        nameTMP.text = actionAndInputs.inputName;
        //inputP1TMP.text = actionAndInputs.inputP1.ToString();
        //inputP2TMP.text = actionAndInputs.inputP2.ToString();
    }
    
}
