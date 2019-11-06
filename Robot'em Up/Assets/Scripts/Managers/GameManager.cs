using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable 0649

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

public enum DamageModifierSource
{
	PerfectReception
}

public class DamageModifier {

	public DamageModifier ( float _multiplyCoef, float _duration, DamageModifierSource _source)
	{
		multiplyCoef = _multiplyCoef;
		duration = _duration;
		source = _source;
	}
	public float multiplyCoef;
	public float duration;
	public DamageModifierSource source;
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


    [HideInInspector] public GameObject surrounderPlayerOne;
    [HideInInspector] public GameObject surrounderPlayerTwo;
    public GameObject SurrounderPrefab;

    [SerializeField]
    GameObject dummyPrefab;
    [SerializeField]
    GameObject ballPrefab;


    private void Awake()
    {
        i = this;
        if (levelManager == null){ levelManager = FindObjectOfType<LevelManager>();}
        if (inputManager == null) { inputManager = FindObjectOfType<InputManager>(); }
        if (eventManager == null) { eventManager = FindObjectOfType<EventManager>(); }
		if (mainCameraGO == null) { mainCameraGO = Camera.main.gameObject; }
        if (enemyManager == null) { enemyManager = FindObjectOfType<EnemyManager>(); }

        if (surrounderPlayerOne!= null) { Destroy(surrounderPlayerOne); }
        surrounderPlayerOne = Instantiate(SurrounderPrefab, playerOne.transform.position, Quaternion.identity);
        surrounderPlayerOne.GetComponent<Surrounder>().playerTransform = playerOne.transform;

        if (surrounderPlayerTwo != null) { Destroy(surrounderPlayerTwo); }
        surrounderPlayerTwo = Instantiate(SurrounderPrefab, playerTwo.transform.position, Quaternion.identity);
        surrounderPlayerTwo.GetComponent<Surrounder>().playerTransform = playerTwo.transform;

        //if (playerOne && playerTwo) { AssignPlayers(); }
    }

	private void Update ()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			ResetBall();
		}
	}

	//private void AssignPlayers()
	//{
	//    if (playerTwo != null)
	//    {
	//        playerOne.GetComponent<PlayerControllerAlex>().otherPlayer = playerOne.GetComponent<PlayerControllerAlex>().ballTarget = playerTwo.transform;
	//    }
	//    if (playerOne != null)
	//    {
	//        playerTwo.GetComponent<PlayerControllerAlex>().otherPlayer = playerTwo.GetComponent<PlayerControllerAlex>().ballTarget = playerOne.transform;
	//    }
	//}

	private void OnEnable()
    {
        EventManager.DummySpawn += SpawnDummy;
    }

    void SpawnDummy()
    {
        playerTwo = Instantiate(dummyPrefab, playerOne.transform.position + (playerOne.transform.forward * 7f), Quaternion.identity);
        //if (playerOne && playerTwo) { AssignPlayers(); }
    }

	public static void ResetBall()
	{
		foreach (BallBehaviour ball in FindObjectsOfType<BallBehaviour>())
		{
			Destroy(ball.gameObject);
		}
		GameObject newBall = Instantiate(i.ballPrefab, null);
        i.ball = newBall;
		newBall.transform.position = new Vector3(0, 1f, 0);
	}
}
