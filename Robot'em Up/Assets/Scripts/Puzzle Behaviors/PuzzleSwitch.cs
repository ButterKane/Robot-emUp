using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;
using MyBox;

public class PuzzleSwitch : PuzzleActivator
{
    private MeshRenderer meshRenderer;
    private BoxCollider boxCollider;
    private bool playerHere;
    public GameObject interactionHelper;
    private List<PawnController> listPawnsHere;


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
        boxCollider = GetComponent<BoxCollider>();
        playerHere = false;
        UpdateMaterial();
        interactionHelper.SetActive(false);
        playerIndex = PlayerIndex.One;
        playerIndex2 = PlayerIndex.Two;
        state = GamePad.GetState(playerIndex);
        state2 = GamePad.GetState(playerIndex2);
        listPawnsHere = new List<PawnController>();
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
                //Debug.Log("ActionOnSwitch 1");
                ActionOnSwitch();
            }
            prevState2 = state2;
            state2 = GamePad.GetState(playerIndex2);

            // Detect if a button was pressed this frame
            if (state2.Buttons.A == ButtonState.Released && prevState2.Buttons.A == ButtonState.Pressed)
            {
                //Debug.Log("ActionOnSwitch 2");
                ActionOnSwitch();
            }
        }

    }

    private void ActionOnSwitch()
    {
      //  Debug.Log("Action on switch fct");
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
            meshRenderer.material = puzzleData.M_SwitchActivate;
        }
        else
        {
            meshRenderer.material = puzzleData.M_SwitchDesactivate;
        }
    }


    private void OnTriggerEnter(Collider _other)
    {
        if (_other.GetComponent<PlayerController>())
        {
            PawnController pawn = _other.gameObject.GetComponent<PawnController>();
            listPawnsHere.Add(pawn);
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
            PawnController pawn = _other.gameObject.GetComponent<PawnController>();
            listPawnsHere.Remove(pawn);
            if (listPawnsHere.Count < 1)
            {
                playerHere = false;
                interactionHelper.SetActive(false);
            }
        }
    }
}
