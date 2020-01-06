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
        if (playersInvicible && activateCheat)
        {
            playerOne.isInvincible_access = playersInvicible;
            playerTwo.isInvincible_access = playersInvicible;
        }
    }

    void OnGUI()
    {
        Color internal_normalColor = GUI.color;

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

            GUI.backgroundColor = internal_normalColor;

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
        if (GameManager.i.ball != null)
        {
            Destroy(GameManager.i.ball);
        }

        Instantiate(ballPrefab, playerOne.transform.position, Quaternion.identity);
    }

    public void ToggleInvicibility()
    {
        isInvincibilityToggled = !isInvincibilityToggled;
        playersInvicible = !playersInvicible;
        if (!playersInvicible)
        {
            playerOne.isInvincible_access = playersInvicible;
            playerTwo.isInvincible_access = playersInvicible;
        }
    }

    public void ChargeEnergy()
    {
        EnergyManager.IncreaseEnergy(1);
    }

    public IEnumerator KillEnemies()
    {
        List<EnemyBehaviour> internal_enemies = EnemyManager.i.enemies;
        int count = internal_enemies.Count;
        for (int i = 0; i < count; i++)
        {
            Debug.Log("destroying " + internal_enemies[0]);
            if (internal_enemies[0].transform.GetComponent<EnemyRedBarrel>())
            {
                EnemyBehaviour temp = internal_enemies[0];
                EnemyManager.i.enemies.Remove(temp);
                Destroy(temp.gameObject);
            }
            else
            {
                internal_enemies[0].Die(); 
            }
            yield return null;
        }

        TurretBehaviour[] internal_turrets = FindObjectsOfType<TurretBehaviour>();
        foreach(var turret in internal_turrets)
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
