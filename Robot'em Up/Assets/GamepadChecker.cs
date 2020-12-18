using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class GamepadChecker : MonoBehaviour
{
    public static GamepadChecker instance;

    [Header("Settings")]
    [SerializeField] private float animationDuration = 1f;
    [SerializeField] private Ease animationEase = default;

    [Header("References")]
    [SerializeField] private Image background = default;
    [SerializeField] private TextMeshProUGUI[] texts = default;


    private float savedTimescale;


    private bool shown = false;

    private void Awake ()
    {
        instance = this;
        DontDestroyOnLoad(transform);
    }

    public void Show ()
    {
        if (shown) return;
        shown = true;
        background.DOFade(1f, animationDuration).SetEase(animationEase).SetUpdate(true);
        foreach (TextMeshProUGUI t in texts)
        {
            t.DOFade(1f, animationDuration).SetEase(animationEase).SetUpdate(true);
        }
    }

    public void Hide()
    {
        if (!shown) return;
        shown = false;
        background.DOFade(0f, animationDuration).SetEase(animationEase).SetUpdate(true);
        foreach (TextMeshProUGUI t in texts)
        {
            t.DOFade(0f, animationDuration).SetEase(animationEase).SetUpdate(true);
        }
    }
    public int GetConnectedGamepadAmount ()
    {
        int amount = 0;
        string[] i_names = Input.GetJoystickNames();
        for (int i = 0; i < i_names.Length; i++)
        {
            if (i_names[i].Length > 0)
            {
                amount++;
            }
        }
        return amount;
    }

    private void Update()
    {
        int gamepadAmount = GetConnectedGamepadAmount();
        if (!shown)
        {
            if (gamepadAmount < 1)
            {
                Show();
                savedTimescale = Time.timeScale;
                Time.timeScale = 0f;
            }
        }
        if (shown)
        {
            if (gamepadAmount >= 1)
            {
                Hide();
                Time.timeScale = savedTimescale;
            }
        }
    }
}
