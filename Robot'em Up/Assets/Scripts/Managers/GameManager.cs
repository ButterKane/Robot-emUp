using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
	DeathExplosion,
	ReviveExplosion,
	RedBarrelExplosion
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

	[HideInInspector] public GameObject mainCameraGO;
	[HideInInspector] public PlayerController playerOne;
	[HideInInspector] public PlayerController playerTwo;
	[HideInInspector] public BallBehaviour ball;
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
		//Auto assign players
		foreach (PlayerController pc in FindObjectsOfType<PlayerController>())
		{
			if (pc.playerIndex == XInputDotNetPure.PlayerIndex.One) {
				playerOne = pc;
			}
			if (pc.playerIndex == XInputDotNetPure.PlayerIndex.Two)
			{
				playerTwo = pc;
			}
		}
        if (levelManager == null){ levelManager = FindObjectOfType<LevelManager>();}
        if (inputManager == null) { inputManager = FindObjectOfType<InputManager>(); }
        if (eventManager == null) { eventManager = FindObjectOfType<EventManager>(); }
		if (mainCameraGO == null) { mainCameraGO = Camera.main.gameObject; }
        if (enemyManager == null) { enemyManager = FindObjectOfType<EnemyManager>(); }
		if (ball == null) { ball = FindObjectOfType<BallBehaviour>(); }

		LoadMomentumManager();
		LoadEnergyManager();

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
		if (Input.GetKeyDown(KeyCode.B))
		{
			ResetBall();
		}
		if (Input.GetKeyDown(KeyCode.R))
		{
			ResetScene();
		}
		UpdateSceneLoader();
	}

	void LoadSceneByIndex(int index)
	{
		SceneLoader sceneLoader = Resources.Load<SceneLoader>("SceneLoader");
		if (sceneLoader == null) { Debug.LogWarning("SceneLoader not found, can't load scene"); return; }
		string sceneFound = sceneLoader.GetSceneByIndex(index);
		if (sceneFound == "") { Debug.LogWarning("Scene with that ID doesn't exit"); return; }
		SceneManager.LoadScene(sceneFound);
	}
	void UpdateSceneLoader()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			LoadSceneByIndex(1);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			LoadSceneByIndex(2);
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			LoadSceneByIndex(3);
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			LoadSceneByIndex(4);
		}
		if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			LoadSceneByIndex(5);
		}
		if (Input.GetKeyDown(KeyCode.Alpha6))
		{
			LoadSceneByIndex(6);
		}
		if (Input.GetKeyDown(KeyCode.Alpha7))
		{
			LoadSceneByIndex(7);
		}
		if (Input.GetKeyDown(KeyCode.Alpha8))
		{
			LoadSceneByIndex(8);
		}
		if (Input.GetKeyDown(KeyCode.Alpha9))
		{
			LoadSceneByIndex(9);
		}
		if (Input.GetKeyDown(KeyCode.Alpha0))
		{
			LoadSceneByIndex(10);
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
        Instantiate(dummyPrefab, playerOne.transform.position + (playerOne.transform.forward * 7f), Quaternion.identity);
        //if (playerOne && playerTwo) { AssignPlayers(); }
    }

	public static void ResetScene()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public static void ResetBall()
	{
		foreach (BallBehaviour ball in FindObjectsOfType<BallBehaviour>())
		{
			Destroy(ball.gameObject);
		}
		GameObject newBall = Instantiate(i.ballPrefab, null);
		BallBehaviour.instance = newBall.GetComponent<BallBehaviour>();
        i.ball = newBall.GetComponent<BallBehaviour>();
		newBall.transform.position = new Vector3(0, 1f, 0);
	}

	void LoadMomentumManager()
	{
		MomentumManager mm = gameObject.AddComponent<MomentumManager>();
		MomentumManager.datas = (MomentumData)Resources.Load("MomentumData");
	}

	void LoadEnergyManager()
	{
		EnergyManager em = gameObject.AddComponent<EnergyManager>();
		EnergyManager.datas = (EnergyData)Resources.Load("EnergyData");
	}
}
