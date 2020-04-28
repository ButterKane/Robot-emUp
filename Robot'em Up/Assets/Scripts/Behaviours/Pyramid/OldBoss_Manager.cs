using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class OldBoss_Manager : MonoBehaviour
{

    [HideInInspector] public static OldBoss_Manager i;

    public GameObject PyramidBehaviorGameObject;
    public ArenaDoor doorExit;
    public bool PyramidActivation;
    public float startDifficulty;
    public float divisorSpeedDifficulty;
    public float difficultyGainedWhenDestroyWeakPoint;
    public float minDifficulty;
    public float difficulty;
    public float showInversionMessage;
    public bool OnePlayerLeft;

    public List<OldBoss_WeakPoint> weakPoints;
    public Text winningMessage;
    public Text inversionMessage;
    public Slider pyramidHealth;

    public GameObject cameraZone;
    public Cinemachine.CinemachineVirtualCamera EndCamera;

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
    }


    public void ActivatePyramid()
    {
        PyramidActivation = true;
        PyramidBehaviorGameObject.SetActive(true);
        difficulty = startDifficulty;
    }


    public void DesactivatePyramid()
    {
        PyramidActivation = false;
        PyramidBehaviorGameObject.SetActive(false);
        doorExit.OpenDoor();
        cameraZone.SetActive(false);
        EndCamera.Priority = 13;
    }
    

    public void CheckIfWeakpointsAreAlive()
    {
        bool i_weakpointsAlive = true;
        foreach (var item in weakPoints)
        {
            if (item.life > 0)
            {
                i_weakpointsAlive = false;
            }
        }
        if (i_weakpointsAlive)
        {
            DesactivatePyramid();
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

        if (PyramidActivation)
        {
            int i_temporaryHealth = 0;
            showInversionMessage -= Time.deltaTime;
            if (showInversionMessage < 0)
            {
                inversionMessage.gameObject.SetActive(false);
            }
            foreach (var item in weakPoints)
            {
                if (item.life > 0)
                {
                    i_temporaryHealth++;
                }
            }
            if (i_temporaryHealth < 1)
            {
                pyramidHealth.gameObject.SetActive(false);
            }
            pyramidHealth.value = i_temporaryHealth;

            float i_totalHealth = GameManager.playerOne.GetHealth() + GameManager.playerTwo.GetHealth();
            float i_totalmaxHealth = GameManager.playerOne.maxHealth + GameManager.playerTwo.maxHealth;
            if (GameManager.playerOne.GetHealth() < 1 | GameManager.playerTwo.GetHealth() < 1)
            {
                OnePlayerLeft = true;
            }
            else
            {
                OnePlayerLeft = false;
            }

            if (i_totalHealth / i_totalmaxHealth > 1.2)
            {
                difficulty += Time.deltaTime / divisorSpeedDifficulty;
            }
            else if (i_totalHealth / i_totalmaxHealth > 0.6)
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
}
