using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class CheatCodes : MonoBehaviour
{
    public bool activateCheat;
    [ConditionalField(nameof(activateCheat))] public GameObject ballPrefab;
    [ConditionalField(nameof(activateCheat))] public PlayerController playerOne;
    [ConditionalField(nameof(activateCheat))] public PlayerController playerTwo;
    [ConditionalField(nameof(activateCheat))] public bool playersInvicible;
    private bool isInvincibilityToggled = false;


    private void Start()
    {
        ballPrefab = GameManager.i.ballPrefab;
        playerOne = GameManager.playerOne;
        playerTwo = GameManager.playerTwo;
        playersInvicible = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Quote)) // "²"
        {
            activateCheat = !activateCheat;
        }
    }

    void OnGUI()
    {
        Color i_normalColor = GUI.color;

        if (activateCheat)
        {
            if (GUI.Button(new Rect(10, 10, 60, 25), "TP Ball"))
            {
                TPBallOnPlayer();
            }

            if (isInvincibilityToggled)
            {
                GUI.backgroundColor = Color.green;
            }

            if (GUI.Button(new Rect(10, 40, 120, 25), "Toggle invicibility"))
            {
                ToggleInvicibility();
            }

            GUI.backgroundColor = i_normalColor;

            if (GUI.Button(new Rect(10, 70, 100, 25), "Charge Energy"))
            {
                ChargeEnergy();
            }
            if (GUI.Button(new Rect(10, 100, 100, 25), "Kill Enemies"))
            {
                StartCoroutine(KillEnemies());
            }
            if (GUI.Button(new Rect(10, 130, 100, 25), "Restart"))
            {
                Restart();
            }
            if (GUI.Button(new Rect(10, 160, 100, 25), "Go Previous"))
            {
                GoPrevious();
            }
            if (GUI.Button(new Rect(120, 160, 100, 25), "Go Next"))
            {
                GoNext();
            }
        }
    }

    public void TPBallOnPlayer()
    {
        if (GameManager.ball != null)
        {
            Destroy(GameManager.ball);
        }

        Instantiate(ballPrefab, playerOne.transform.position, Quaternion.identity);
    }

    public void ToggleInvicibility()
    {
        isInvincibilityToggled = !isInvincibilityToggled;
        playersInvicible = !playersInvicible;
        playerOne.SetInvincible(playersInvicible);
        playerTwo.SetInvincible(playersInvicible);
    }

    public void ChargeEnergy()
    {
        EnergyManager.IncreaseEnergy(1);
    }

    public IEnumerator KillEnemies()
    {
        List<EnemyBehaviour> i_enemies = EnemyManager.i.enemies;
        int count = i_enemies.Count;
        for (int i = 0; i < count; i++)
        {
            if (i_enemies[0].transform.GetComponent<EnemyRedBarrel>()) 
            {
                EnemyBehaviour temp = i_enemies[0];
                EnemyManager.i.enemies.Remove(temp);
                Destroy(temp.gameObject);
            }
            else
            {
                i_enemies[0].Kill(); 
            }
            yield return null;
        }

        TurretBehaviour[] i_turrets = FindObjectsOfType<TurretBehaviour>();
        foreach(var turret in i_turrets)
        {
            turret.Die();
            yield return null;
        }
    }

    public void Restart()
    {
        GameManager.ResetScene();
    }

    public void GoNext()
    {
        if(SceneManager.GetActiveScene().buildIndex < SceneManager.sceneCountInBuildSettings - 1)
            GameManager.LoadSceneByIndex(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void GoPrevious()
    {
        if (SceneManager.GetActiveScene().buildIndex > 0)
            GameManager.LoadSceneByIndex(SceneManager.GetActiveScene().buildIndex - 1);
    }
}
