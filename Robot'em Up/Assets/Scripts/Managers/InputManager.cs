using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using XInputDotNetPure;

public class InputManager : MonoBehaviour
{
    public static InputManager i;

    [Separator("References")]
    public SettingsInputGroup[] inputGroups;

    [Separator("Settings")]
    public bool inputDisabled;
    public float triggerTreshold = 0.1f;
    GamePadState state;
    private Camera cam;
    public PlayerIndex playerIndex;
    [ReadOnly] public Vector3 leftJoystickInput;
    public float deadzone = 0.2f;
    protected Vector3 rightJoystickInput;

    private bool rightTriggerWaitForRelease;
    //private bool leftTriggerWaitForRelease; //Uncomment if needed (Commented to avoid errors)
    private bool leftShouldWaitForRelease;
    private bool rightShouldWaitForRelease;

    private void Awake()
    {
        cam = GameManager.mainCamera;
        if (i == null)
        {
            i = this;
        }
        else
        {
            Debug.LogError("There is already an InputManager");
        }
    }

    private void Update()
    {
        
    }

    public void AssignSettingsInputIntoInputHandler()
    {
        // First, must check if every action has a valid input assigned for each player. If not, return.

        // Then must separate each column for player 1 & 2
        // Then, identify each action (dunk, throw etc.) and check what has been changed, so we only manage these.
        // Foreach "changed action", clear Buttons[] (or Axis[]) and create a new one according to the inputs in the settings
        // That should do it
    }

}
