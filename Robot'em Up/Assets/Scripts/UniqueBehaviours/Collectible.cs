using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using XInputDotNetPure;
using MyBox;
using TMPro;

public class Collectible : MonoBehaviour
{

    [Header("References")]
    public Renderer myRend;
    public SphereCollider myCollider;
    public GameObject collectibleUI;
    public Renderer quadPlayer1Rend;
    public Renderer quadPlayer2Rend;
    public Animator myAnim;

    [Header("Variables to tweak")]
    public float ratioBeforeTriggerOut;
    public float timeToPressA;
    //CompletionCircle !
    public float normalEmissiveMultiplier;
    public float maxEmissiveMultiplier;
    public float curveTime;
    public AnimationCurve emissiveCurve;
    public UnityEvent collectedEvent;

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

    void Awake()
    {
        playerIndex1 = PlayerIndex.One;
        playerIndex2 = PlayerIndex.Two;
        state1 = GamePad.GetState(playerIndex1);
        state2 = GamePad.GetState(playerIndex2);
    }

    void Update()
    {
        CheckPlayerCompletion();
        CompletionEmissiveUpdate();
    }

    void CheckPlayerCompletion()
    {
        if (activated)
        {
            //UPDATE RATIO COMPLETION
            state1 = GamePad.GetState(playerIndex1);
            if (player1InRange && state1.Buttons.A == ButtonState.Pressed)
            {
                PlayerRatioUpdate(true, Mathf.Clamp01(player1PressRatio + Time.deltaTime / timeToPressA));
            }
            else if (player1PressRatio > 0)
            {
                PlayerRatioUpdate(true, Mathf.Clamp01(player1PressRatio - Time.deltaTime / timeToPressA));
            }

            state2 = GamePad.GetState(playerIndex2);
            if (player2InRange && state2.Buttons.A == ButtonState.Pressed)
            {
                PlayerRatioUpdate(false, Mathf.Clamp01(player2PressRatio + Time.deltaTime / timeToPressA));
            }
            else if (player2PressRatio > 0)
            {
                PlayerRatioUpdate(false, Mathf.Clamp01(player2PressRatio - Time.deltaTime / timeToPressA));
            }

            //CHECK IF FULLY COMPLETED
            if (player1PressRatio >= 1 && player2PressRatio >= 1)
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
        collectedEvent.Invoke();
    }

    void DestroyObject()
    {
        Destroy(gameObject);
    }
}
