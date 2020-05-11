using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputLinkUIGame : MonoBehaviour
{
    public PlayerController player1;
    public PlayerController player2;

    private void Start()
    {
        player1 = GameManager.playerOne;
        player2 = GameManager.playerTwo;
    }

    public void Dash(int _playerIndex)
    {
        if (_playerIndex == 1) 
        {
            
        }
    }

    public void Dunk()
    {

    }

    public void ThrowBall()
    {

    }

    public void Grapple()
    {

    }

    public void LocateBall()
    {

    }
}
