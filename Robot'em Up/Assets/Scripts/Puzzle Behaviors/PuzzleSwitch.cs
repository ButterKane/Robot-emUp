using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;
using MyBox;

public class PuzzleSwitch : PuzzleActivator
{
    private MeshRenderer meshRenderer;
    private bool playerHere;
    public GameObject interactionHelper;
    private int totalPawnAmount = 0;


    [ReadOnly]
    public PlayerIndex playerIndex;
    [ReadOnly]
    public PlayerIndex playerIndex2;
    private GamePadState state;
    private GamePadState prevState;
    private GamePadState state2;
    private GamePadState prevState2;


    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        playerHere = false;
        UpdateMaterial();
        interactionHelper.SetActive(false);
        playerIndex = PlayerIndex.One;
        playerIndex2 = PlayerIndex.Two;
        state = GamePad.GetState(playerIndex);
        state2 = GamePad.GetState(playerIndex2);
        totalPawnAmount = 0;
    }
    
    void Update()
    {
        if (playerHere)
        {
            prevState = state;
            state = GamePad.GetState(playerIndex);

            // Detect if a button was pressed this frame
            if (state.Buttons.A == ButtonState.Released && prevState.Buttons.A == ButtonState.Pressed)
            {
                ActionOnSwitch();
            }
            prevState2 = state2;
            state2 = GamePad.GetState(playerIndex2);

            // Detect if a button was pressed this frame
            if (state2.Buttons.A == ButtonState.Released && prevState2.Buttons.A == ButtonState.Pressed)
            {
                ActionOnSwitch();
            }
        }

    }

    private void ActionOnSwitch()
    {
        isActivated = !isActivated;
        if (isActivated)
        {
            ActivateLinkedObjects();
        }
        else
        {
            DesactiveLinkedObjects();
        }
        UpdateMaterial();
    }


    private void UpdateMaterial()
    {
        UpdateLight();
        if (isActivated)
        {
            meshRenderer.material = puzzleData.m_switchActivate;
        }
        else
        {
            meshRenderer.material = puzzleData.m_switchDesactivate;
        }
    }


    private void OnTriggerEnter(Collider _other)
    {
        if (_other.GetComponent<PlayerController>())
        {
            totalPawnAmount++;
            playerHere = true;
            if (puzzleData.showTuto)
            {
                interactionHelper.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider _other)
    {
        if (_other.GetComponent<PlayerController>())
        {
            totalPawnAmount--;
            if (totalPawnAmount < 1)
            {
                playerHere = false;
                interactionHelper.SetActive(false);
            }
        }
    }
}
