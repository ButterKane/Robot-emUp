using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;
using MyBox;
using TMPro;

public class Collectible : MonoBehaviour
{

    [Header("References")]
    public Renderer myRend;
    public SphereCollider myCollider;
    public TextMeshPro instructionText;
    public Renderer quadPlayer1Rend;
    public Renderer quadPlayer2Rend;

    [Header("Variables to tweak")]
    public float ratioBeforeTriggerOut;
    public float timeToPressA;

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
    float player1PressRatio;
    float player2PressRatio;

    void Awake()
    {
        playerIndex1 = PlayerIndex.One;
        playerIndex2 = PlayerIndex.Two;
        state1 = GamePad.GetState(playerIndex1);
        state2 = GamePad.GetState(playerIndex2);
    }

    void Update()
    {
        if (activated)
        {
            //UPDATE RATIO COMPLETION
            state1 = GamePad.GetState(playerIndex1);
            if (player1InRange && state1.Buttons.A == ButtonState.Pressed)
            {
                PlayerRatioUpdate(true, Mathf.Clamp01(player1PressRatio + Time.deltaTime / timeToPressA));
            }
            else if(player1PressRatio>0)
            {
                PlayerRatioUpdate(true, Mathf.Clamp01(player1PressRatio - Time.deltaTime / timeToPressA));
            }

            state2 = GamePad.GetState(playerIndex2);
            if (player2InRange && state2.Buttons.A == ButtonState.Pressed)
            {
                PlayerRatioUpdate(false, Mathf.Clamp01(player2PressRatio + Time.deltaTime / timeToPressA));
            }
            else if(player2PressRatio>0)
            {
                PlayerRatioUpdate(false, Mathf.Clamp01(player2PressRatio - Time.deltaTime / timeToPressA));
            }

            //CHECK IF FULLY COMPLETED
            if(player1PressRatio >= 1 && player2PressRatio >= 1)
            {
                CollectedEvent();
            }


        }
        else if(player1PressRatio > 0 || player2PressRatio > 0)
        {
            PlayerRatioUpdate(true, 0);
            PlayerRatioUpdate(false, 0);
        }
    }

    void PlayerRatioUpdate(bool _player1, float _value)
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

            if (!activated)
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

            if(!player1InRange && !player2InRange)
            {
                DeactivateCollectible();
            }
        }
    }

    void ActivateCollectible()
    {
        instructionText.gameObject.SetActive(true);
        activated = true;
    }
    void DeactivateCollectible()
    {
        instructionText.gameObject.SetActive(false);
        activated = false;
    }

    void CollectedEvent()
    {
        print("HEY YO");
        Destroy(gameObject);
    }
}
