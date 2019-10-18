using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VibrationForce
{
	Heavy,
	Medium,
	Light
}
public enum DamageSource
{
	Ball,
	Dunk,
}
public class GameManager : MonoBehaviour
{
    //Singleton du gameManager
    [HideInInspector] public static GameManager i;
    [HideInInspector] public LevelManager levelManager;
    [HideInInspector] public InputManager inputManager;
    [HideInInspector] public EventManager eventManager;
    [HideInInspector] public EnemyManager enemyManager;

    public GameObject mainCameraGO;
    public GameObject playerOne;
    public GameObject playerTwo;
    public GameObject ball;
    public BallBehaviour ballBehaviour;
    public int ballDamage = 30;
    public List<GameObject> enemies; 

    [SerializeField]
    GameObject dummyPrefab;


    private void Awake()
    {
        i = this;
        if (levelManager == null){ levelManager = FindObjectOfType<LevelManager>();}
        if (inputManager == null) { inputManager = FindObjectOfType<InputManager>(); }
        if (eventManager == null) { eventManager = FindObjectOfType<EventManager>(); }
		if (mainCameraGO == null) { mainCameraGO = Camera.main.gameObject; }

        if (playerOne && playerTwo) { AssignPlayers(); }

    }

    private void AssignPlayers()
    {
        if (playerTwo != null)
        {
            playerOne.GetComponent<PlayerControllerAlex>().otherPlayer = playerOne.GetComponent<PlayerControllerAlex>().ballTarget = playerTwo.transform;
        }
        if (playerOne != null)
        {
            playerTwo.GetComponent<PlayerControllerAlex>().otherPlayer = playerTwo.GetComponent<PlayerControllerAlex>().ballTarget = playerOne.transform;
        }
    }

    private void OnEnable()
    {
        EventManager.DummySpawn += SpawnDummy;
    }

    void SpawnDummy()
    {
        playerTwo = Instantiate(dummyPrefab, playerOne.transform.position + (playerOne.transform.forward * 7f), Quaternion.identity);
        if (playerOne && playerTwo) { AssignPlayers(); }
    }
}
