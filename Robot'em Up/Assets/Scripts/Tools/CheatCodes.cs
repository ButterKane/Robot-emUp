using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class CheatCodes : MonoBehaviour
{
    public bool ActivateCheat;
    [ConditionalField(nameof(ActivateCheat))] public GameObject BallPrefab;
    [ConditionalField(nameof(ActivateCheat))] public PlayerController PlayerOne;
    [ConditionalField(nameof(ActivateCheat))] public PlayerController PlayerTwo;
    [ConditionalField(nameof(ActivateCheat))] public bool playersInvicible;
    private bool isInvincibilityToggled = false;


    private void Start()
    {
        BallPrefab = GameManager.i.ballPrefab;
        PlayerOne = GameManager.playerOne;
        PlayerTwo = GameManager.playerTwo;
        playersInvicible = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Quote)) // "²"
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
        Color normalColor = GUI.color;

        if (ActivateCheat)
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

            GUI.backgroundColor = normalColor;

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
            if (GUI.Button(new Rect(120, 150, 100, 25), "Go Next"))
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

        Instantiate(BallPrefab, PlayerOne.transform.position, Quaternion.identity);
    }

    public void ToggleInvicibility()
    {
        isInvincibilityToggled = !isInvincibilityToggled;
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

    public IEnumerator KillEnemies()
    {
        List<EnemyBehaviour> enemies = EnemyManager.i.enemies;
        int count = enemies.Count;
        for (int i = 0; i < count; i++)
        {
            Debug.Log("destroying " + enemies[0]);
            if (enemies[0].transform.GetComponent<EnemyRedBarrel>())
            {
                EnemyBehaviour temp = enemies[0];
                EnemyManager.i.enemies.Remove(temp);
                Destroy(temp.gameObject);
            }
            else
            {
                enemies[0].Die(); 
            }
            yield return null;
        }

        TurretBehaviour[] turrets = FindObjectsOfType<TurretBehaviour>();
        foreach(var turret in turrets)
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
            GameManager.i.LoadSceneByIndex(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void GoPrevious()
    {
        if (SceneManager.GetActiveScene().buildIndex > 0)
            GameManager.i.LoadSceneByIndex(SceneManager.GetActiveScene().buildIndex - 1);
    }
}
