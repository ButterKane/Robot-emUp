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
    public float showInversionMessage;
    public bool OnePlayerLeft;

    public List<Boss_WeakPoint> weakPoints;
    public Text winningMessage;
    public Text inversionMessage;
    public Slider pyramidHealth;

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
        bool internal_weakpointsAlive = true;
        foreach (var item in weakPoints)
        {
            if (item.life > 0)
            {
                internal_weakpointsAlive = false;
            }
        }
        if (internal_weakpointsAlive)
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
        int internal_temporaryHealth = 0;
        showInversionMessage -= Time.deltaTime;
        if (showInversionMessage < 0)
        {
            inversionMessage.gameObject.SetActive(false);
        }
        foreach (var item in weakPoints)
        {
            if (item.life > 0)
            {
                internal_temporaryHealth++;
            }
        }
        if (internal_temporaryHealth < 1)
        {
            pyramidHealth.gameObject.SetActive(false);
        }
        pyramidHealth.value = internal_temporaryHealth;

        int internal_totalHealth = GameManager.playerOne.currentHealth + GameManager.playerTwo.currentHealth;
        int internal_totalmaxHealth = GameManager.playerOne.maxHealth + GameManager.playerTwo.maxHealth;
        if (GameManager.playerOne.currentHealth < 1 | GameManager.playerTwo.currentHealth < 1)
        {
            OnePlayerLeft = true;
        }
        else
        {
            OnePlayerLeft = false;
        }

        if (internal_totalHealth / internal_totalmaxHealth > 1.2)
        {
            difficulty += Time.deltaTime / divisorSpeedDifficulty;
        }
        else if (internal_totalHealth / internal_totalmaxHealth > 0.6)
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
