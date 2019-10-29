using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;
using MyBox;

public class PuzzleSwitch : PuzzleActivator
{
    private MeshRenderer meshRenderer;
    private BoxCollider boxCollider;
    private bool PlayerHere;
    public GameObject InteractionHelper;
    public Light light;


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
        PlayerHere = false;
        UpdateMaterial();
        InteractionHelper.SetActive(false);
        playerIndex = PlayerIndex.One;
        playerIndex2 = PlayerIndex.Two;
    }
    
    void Update()
    {
        if (PlayerHere)
        {
            Debug.Log("Player Here");

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
        Debug.Log("Action on switch");
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
        if (isActivated)
        {
            light.gameObject.SetActive(true);
            meshRenderer.material = puzzleData.M_SwitchActivate;
        }
        else
        {
            light.gameObject.SetActive(false);
            meshRenderer.material = puzzleData.M_SwitchDesactivate;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            PlayerHere = true;
            InteractionHelper.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            PlayerHere = false;
            InteractionHelper.SetActive(false);
        }
    }
}
