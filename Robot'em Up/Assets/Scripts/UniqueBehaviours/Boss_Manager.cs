using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boss_Manager : MonoBehaviour
{

    [HideInInspector] public static Boss_Manager i;

    public float startDifficulty;
    public float divisorSpeedDifficulty;
    public float difficultyGainedWhenDestroyWeakPoint;
    public float minDifficulty;
    public float difficulty;
    public bool OnePlayerLeft;

    public List<Boss_WeakPoint> weakPoints;
    public Text winningMessage;

    private void Awake()
    {
        if (i == null)
        {
            i = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        difficulty = startDifficulty;
    }

    public void CheckIfWeakpointsAreAlive()
    {
        bool temp = true;
        foreach (var item in weakPoints)
        {
            if (item.life > 0)
            {
                temp = false;
            }
        }
        if (temp)
        {
            difficulty = 0;
            winningMessage.gameObject.SetActive(true);
        }
    }

    public void DestroyAWeakPoint()
    {
        CheckIfWeakpointsAreAlive();
        difficulty =+ difficultyGainedWhenDestroyWeakPoint;
    }

    
    void Update()
    {
        int totalHealth = GameManager.playerOne.currentHealth + GameManager.playerTwo.currentHealth;
        int totalmaxHealth = GameManager.playerOne.maxHealth + GameManager.playerTwo.maxHealth;
        if (GameManager.playerOne.currentHealth < 1 | GameManager.playerTwo.currentHealth < 1)
        {
            OnePlayerLeft = true;
        }
        else
        {
            OnePlayerLeft = false;
        }

        if (totalHealth / totalmaxHealth > 1.2)
        {
            difficulty += Time.deltaTime / divisorSpeedDifficulty;
        }
        else if (totalHealth / totalmaxHealth > 0.6)
            {
            difficulty -= Time.deltaTime / divisorSpeedDifficulty;

        }
        else
        {
            difficulty -= Time.deltaTime / divisorSpeedDifficulty / 1.5f;
        }
        if (difficulty < minDifficulty)
        {
            difficulty = minDifficulty;
        }
    }
}
