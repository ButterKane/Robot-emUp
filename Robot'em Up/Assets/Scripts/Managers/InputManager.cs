using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using XInputDotNetPure;

public enum Inputs
{
    AButton,
    BButton,
    XButton,
    YButton,
    PadUp,
    PadDown,
    PadRight,
    PadLeft,
    LeftJoystick,
    RightJoystick,
    LB,
    RB,
    LT,
    RT,
    StartButton,
    BackButton
}

public class InputManager : MonoBehaviour
{
    public static InputManager i;

    [Separator("Settings")]
    public bool inputDisabled;
    public float triggerTreshold = 0.1f;
    GamePadState state;
    private Camera cam;
    public PlayerIndex playerIndex;
    [ReadOnly] public Vector3 leftJoystickInput;
    public float deadzone = 0.2f;
    protected Vector3 rightJoystickInput;
    public Dictionary<Inputs, UnityEvent> mappedInputsP1;
    public Dictionary<Inputs, UnityEvent> mappedInputsP2;
    private Dictionary<Inputs, UnityEvent> usedInputs;

    private bool rightTriggerWaitForRelease;
    //private bool leftTriggerWaitForRelease; //Uncomment if needed (Commented to avoid errors)
    private bool leftShouldWaitForRelease;
    private bool rightShouldWaitForRelease;

    [NonSerialized] public UnityEvent startButtonEvent;
    [NonSerialized] public UnityEvent backButtonEvent;
    [NonSerialized] public UnityEvent LeftJoystickEvent;
    [NonSerialized] public UnityEvent RightJoystickEvent;
    [NonSerialized] public UnityEvent RTEvent;
    [NonSerialized] public UnityEvent LTEvent;
    [NonSerialized] public UnityEvent LBEvent;
    [NonSerialized] public UnityEvent RBEvent;
    [NonSerialized] public UnityEvent AButtonEvent;
    [NonSerialized] public UnityEvent BButtonEvent;
    [NonSerialized] public UnityEvent YButtonEvent;
    [NonSerialized] public UnityEvent XButtonEvent;

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
        //ApplyInputChanges();
    }

    private void Update()
    {
        if (playerIndex == PlayerIndex.One)
        {
            usedInputs = mappedInputsP1;
        }
        if (playerIndex == PlayerIndex.Two)
        {
            usedInputs = mappedInputsP2;
        }
    }

    public void StartButtonAction()
    {
        usedInputs.TryGetValue(Inputs.StartButton, out startButtonEvent);
        startButtonEvent.Invoke();
    }

    public void BackButtonAction()
    {
        usedInputs.TryGetValue(Inputs.BackButton, out backButtonEvent);
        backButtonEvent.Invoke();
    }

    public void XButtonAction()
    {
        usedInputs.TryGetValue(Inputs.XButton, out XButtonEvent);
        XButtonEvent.Invoke();
    }

    public void YButtonAction()
    {
        usedInputs.TryGetValue(Inputs.YButton, out YButtonEvent);
        YButtonEvent.Invoke();
    }

    public void BButtonAction()
    {
        usedInputs.TryGetValue(Inputs.BButton, out BButtonEvent);
        BButtonEvent.Invoke();
    }

    public void AButtonAction()
    {
        usedInputs.TryGetValue(Inputs.AButton, out AButtonEvent);
        AButtonEvent.Invoke();
    }

    public void RBAction()
    {
        usedInputs.TryGetValue(Inputs.RB, out RBEvent);
        RBEvent.Invoke();
    }

    public void LBAction()
    {
        usedInputs.TryGetValue(Inputs.LB, out LBEvent);
        LBEvent.Invoke();
    }

    public void LTAction()
    {
        usedInputs.TryGetValue(Inputs.LT, out LTEvent);
        LTEvent.Invoke();
    }

    public void RTAction()
    {
        usedInputs.TryGetValue(Inputs.RT, out RTEvent);
        RTEvent.Invoke();
    }
}
