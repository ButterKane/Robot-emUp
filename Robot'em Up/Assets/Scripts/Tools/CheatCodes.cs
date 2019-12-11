﻿using MyBox;
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
        playersInvicible = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Quote))
        {
            ActivateCheat = !ActivateCheat;
        }
        if (playersInvicible && ActivateCheat)
        {
            PlayerOne.IsInvincible = playersInvicible;
            PlayerTwo.IsInvincible = playersInvicible;
        }
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
            if (GUI.Button(new Rect(10, 70, 100, 25), "Charge Energy"))
            {
                ChargeEnergy();
            }
            if (GUI.Button(new Rect(10, 100, 100, 25), "Kill Enemies"))
            {
                StartCoroutine(KillEnemies());
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
        if (!playersInvicible)
        {
            PlayerOne.IsInvincible = playersInvicible;
            PlayerTwo.IsInvincible = playersInvicible;
        }
    }

    public void ChargeEnergy()
    {
        EnergyManager.IncreaseEnergy(1);
    }

    //public void KillEnemies()
    //{
    //    List<EnemyBehaviour> enemies = EnemyManager.i.enemies;
    //    for (int i = 0; i < enemies.Count; i++)
    //    {
    //        enemies[0].Die(); // It doesn't destroy every enemy... :/
    //        enemies.Remove(enemies[0]);
    //    }
    //}

    public IEnumerator KillEnemies()
    {
        List<EnemyBehaviour> enemies = EnemyManager.i.enemies;
        int count = enemies.Count;
        for (int i = 0; i < count; i++)
        {
            enemies[0].Die(); // It doesn't destroy every enemy... :/*
            yield return null;
        }
    }
}
