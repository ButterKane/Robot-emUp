using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatCodes : MonoBehaviour
{
    public bool ActivateCheat;
    [ConditionalField(nameof(ActivateCheat))] public GameObject BallPrefab;
    [ConditionalField(nameof(ActivateCheat))] public PlayerController PlayerOne;
    [ConditionalField(nameof(ActivateCheat))] public PlayerController PlayerTwo;
    [ConditionalField(nameof(ActivateCheat))] public bool playersInvicible;

    private void Start()
    {
        BallPrefab = GameManager.i.ballPrefab;
        PlayerOne = GameManager.playerOne;
        PlayerTwo = GameManager.playerTwo;
    }

    void OnGUI()
    {
        if (ActivateCheat)
        {
            if (GUI.Button(new Rect(10, 10, 60, 25), "TP Ball"))
            {
                TPBallOnPlayer();
            }
            if (GUI.Button(new Rect(10, 40, 120, 25), "Toggle invicibility"))
            {
                ToggleInvicibility();
            }
        }
    }


    public void TPBallOnPlayer()
    {
        if (GameManager.i.ball != null)
        {
            Destroy(GameManager.i.ball);
        }

        Instantiate(BallPrefab, PlayerOne.transform.position, Quaternion.identity);
    }

    public void ToggleInvicibility()
    {
        playersInvicible = !playersInvicible;
        PlayerOne.IsInvincible = playersInvicible;
        PlayerTwo.IsInvincible = playersInvicible;
    }
}
