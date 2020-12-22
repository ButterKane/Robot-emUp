using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using XInputDotNetPure;
using MyBox;
using TMPro;

public enum ConcernedAbility
{
    Pass,
    Dash,
    Dunk,
    PerfectReception,
    Revive
}

public enum Upgrade
{
    Base,
    Upgrade1,
    Upgrade2,
    Upgrade3
}

public class Collectible : MonoBehaviour
{
    [Header("References")]
    public Renderer myRend;
    public SphereCollider myCollider;
    public GameObject collectibleUI;
    public Renderer quadPlayer1Rend;
    public Renderer quadPlayer2Rend;
    public Animator myAnim;
    public GameObject aButtonPlayer1;
    public GameObject aButtonPlayer2;
    public TextMeshPro tmp_field;

    public Sprite gamepadInputSprite;
    public Sprite keyboardInputSprite;

    [Header("Variables to tweak")]
    public float ratioBeforeTriggerOut;
    public float timeToPressA;
    public float delayBeforeOpeningMenu;
    //CompletionCircle !
    public float normalEmissiveMultiplier;
    public float maxEmissiveMultiplier;
    public float curveTime;
    public AnimationCurve emissiveCurve;
    //appearance of indicator
    public float indicatorCurveTime;
    public AnimationCurve indicatorCurve;
    //UNITY EVENT
    public UnityEvent collectedEvent;
    [TextArea(2, 5)]
    public string displayedText;
    public ConcernedAbility concernedAbility;
    public Upgrade newAbilityLevel;
    
    [Header("Read Only")]
    [ReadOnly] public PlayerIndex playerIndex1;
    [ReadOnly] public PlayerIndex playerIndex2;
    [ReadOnly] public bool player1InRange;
    [ReadOnly] public bool player2InRange;

    //PRIVATE
    private GamePadState state1;
    private GamePadState state2;
    Transform middlePointRefInRange;
    bool activated;
    bool collected;
    float player1PressRatio;
    float player2PressRatio;
    float emissiveCurveTimerPlayer1;
    float emissiveCurveTimerPlayer2;
    float indicatorCurveTimer;
    private SpriteRenderer player1SR;
    private SpriteRenderer player2SR;

    void Awake()
    {
        playerIndex1 = PlayerIndex.One;
        playerIndex2 = PlayerIndex.Two;
        state1 = GamePad.GetState(playerIndex1);
        state2 = GamePad.GetState(playerIndex2);
        tmp_field.text = displayedText;
        player1SR = aButtonPlayer1.GetComponent<SpriteRenderer>();
        player2SR = aButtonPlayer2.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        CheckPlayerCompletion();
        CompletionEmissiveUpdate();
        IndicatorAppearance();
        UpdateIndicatorController();
    }

    private void UpdateIndicatorController()
    {
        switch (GameManager.playerOne.controllerType)
        {
            case PlayerController.ControllerType.Gamepad:
                player1SR.sprite = gamepadInputSprite;
                break;
            case PlayerController.ControllerType.Keyboard:
                player1SR.sprite = keyboardInputSprite;
                break;
        }
        switch (GameManager.playerTwo.controllerType)
        {
            case PlayerController.ControllerType.Gamepad:
                player2SR.sprite = gamepadInputSprite;
                break;
            case PlayerController.ControllerType.Keyboard:
                player2SR.sprite = keyboardInputSprite;
                break;
        }
    }

    void IndicatorAppearance()
    {
        if (!player1InRange && activated)
        {
            indicatorCurveTimer += Time.deltaTime/indicatorCurveTime;
            if(indicatorCurve.Evaluate(indicatorCurveTimer%1) < 0.5f)
            {
                aButtonPlayer1.SetActive(false);
            }
            else
            {
                aButtonPlayer1.SetActive(true);
            }
        }
        else if (!aButtonPlayer1.activeSelf)
        {
            aButtonPlayer1.SetActive(true);
        }

        if (!player2InRange && activated)
        {
            indicatorCurveTimer += Time.deltaTime / indicatorCurveTime;
            if (indicatorCurve.Evaluate(indicatorCurveTimer % 1) < 0.5f)
            {
                aButtonPlayer2.SetActive(false);
            }
            else
            {
                aButtonPlayer2.SetActive(true);
            }
        }
        else if (!aButtonPlayer2.activeSelf)
        {
            aButtonPlayer2.SetActive(true);
        }

    }

    void CheckPlayerCompletion()
    {
        if (activated)
        {
            //UPDATE RATIO COMPLETION
            state1 = GamePad.GetState(playerIndex1);
            if (player1InRange && GameManager.playerOne.IsPickingAbility())
            {
                PlayerRatioUpdate(true, Mathf.Clamp01(player1PressRatio + Time.deltaTime / timeToPressA));
            }
            else if (player1PressRatio > 0)
            {
                PlayerRatioUpdate(true, Mathf.Clamp01(player1PressRatio - Time.deltaTime / timeToPressA));
            }

            state2 = GamePad.GetState(playerIndex2);
            if (player2InRange && GameManager.playerTwo.IsPickingAbility())
            {
                PlayerRatioUpdate(false, Mathf.Clamp01(player2PressRatio + Time.deltaTime / timeToPressA));
            }
            else if (player2PressRatio > 0)
            {
                PlayerRatioUpdate(false, Mathf.Clamp01(player2PressRatio - Time.deltaTime / timeToPressA));
            }

            //CHECK IF FULLY COMPLETED
            if (player1PressRatio >= 1 && player2PressRatio >= 1 && collected == false)
            {
                CollectedEvent();
            }


        }
        else if (player1PressRatio > 0 || player2PressRatio > 0)
        {
            PlayerRatioUpdate(true, 0);
            PlayerRatioUpdate(false, 0);
        }
    }

    void CompletionEmissiveUpdate()
    {
        //PLAYER1
        if (player1PressRatio >= 1)
        {
            emissiveCurveTimerPlayer1 += Time.deltaTime/curveTime;
            quadPlayer1Rend.material.SetFloat("_EmissiveMultiplier", Mathf.Lerp(normalEmissiveMultiplier, maxEmissiveMultiplier, emissiveCurve.Evaluate(emissiveCurveTimerPlayer1 % 1)));
        }
        else if (quadPlayer1Rend.material.GetFloat("_EmissiveMultiplier") > normalEmissiveMultiplier)
        {
            quadPlayer1Rend.material.SetFloat("_EmissiveMultiplier", Mathf.Lerp(quadPlayer1Rend.material.GetFloat("_EmissiveMultiplier"), normalEmissiveMultiplier, 0.2f));
            emissiveCurveTimerPlayer1 = 0;
        }

        //PLAYER2
        if (player2PressRatio >= 1)
        {
            emissiveCurveTimerPlayer2 += Time.deltaTime / curveTime;
            quadPlayer2Rend.material.SetFloat("_EmissiveMultiplier", Mathf.Lerp(normalEmissiveMultiplier, maxEmissiveMultiplier, emissiveCurve.Evaluate(emissiveCurveTimerPlayer2 % 1)));
        }
        else if (quadPlayer2Rend.material.GetFloat("_EmissiveMultiplier") > normalEmissiveMultiplier)
        {
            quadPlayer2Rend.material.SetFloat("_EmissiveMultiplier", Mathf.Lerp(quadPlayer2Rend.material.GetFloat("_EmissiveMultiplier"), normalEmissiveMultiplier, 0.2f));
            emissiveCurveTimerPlayer2 = 0;
        }
    }

    void PlayerRatioUpdate(bool _player1, float _value)
    {
        if (!collected)
        {
            if (_player1)
            {
                player1PressRatio = _value;
                quadPlayer1Rend.material.SetFloat("_AddToCompleteCircle", player1PressRatio);
            }
            else
            {
                player2PressRatio = _value;
                quadPlayer2Rend.material.SetFloat("_AddToCompleteCircle", player2PressRatio);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if(other.name == "Player1")
            {
                player1InRange = true;
            }
            else if(other.name == "Player2")
            {
                player2InRange = true;
            }

            if (!activated && !collected)
            {
                ActivateCollectible();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if (other.name == "Player1")
            {
                player1InRange = false;
            }
            else if (other.name == "Player2")
            {
                player2InRange = false;
            }

            if(!player1InRange && !player2InRange && !collected)
            {
                DeactivateCollectible();
            }
        }
    }

    void ActivateCollectible()
    {
        collectibleUI.SetActive(true);
        activated = true;
    }
    void DeactivateCollectible()
    {
        collectibleUI.SetActive(false);
        activated = false;
    }

    void CollectedEvent()
    {
        myAnim.enabled = true;
        myAnim.SetTrigger("CollectedTrigger");
        collected = true;
        Invoke("EndEvents", delayBeforeOpeningMenu);
    }

    void EndEvents()
    {
        GameManager.PickedUpAnUpgrade(concernedAbility, newAbilityLevel);
        collectedEvent.Invoke();
        Destroy(gameObject);
    }
}
