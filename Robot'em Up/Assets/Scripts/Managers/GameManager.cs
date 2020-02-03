using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using XInputDotNetPure;
#pragma warning disable 0649

public enum DamageSource
{
	Ball,
	Dunk,
	DeathExplosion,
	ReviveExplosion,
	RedBarrelExplosion,
	PerfectReceptionExplosion,
    EnemyContact,
    Laser,
	SpawnImpact
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
    public static GameManager i;
    
    [Separator ("Public variables")] 
    public int ballDamage = 30;
    public GameObject SurrounderPrefab;
    public GameObject ballPrefab;

    [Separator("Private variables")]
    [SerializeField]
    GameObject dummyPrefab;

    // Auto-Assigned References
    [NonSerialized] public GameObject surrounderPlayerOne;
    [NonSerialized] public GameObject surrounderPlayerTwo;
    public static Canvas mainCanvas;
    public static List<PlayerController> deadPlayers;

    private static MainMenu menu;
    private static bool menuCalledOne = false;
    private static bool menuCalledTwo = false;
    private static bool deathPanelCalled = false;

    [NonSerialized] public LevelManager levelManager;
    [NonSerialized] public InputManager inputManager;
    [NonSerialized] public EnemyManager enemyManager;

    [NonSerialized] public GameObject mainCameraGO;
    [NonSerialized] public static PlayerController playerOne;
    [NonSerialized] public static PlayerController playerTwo;
    [NonSerialized] public static BallBehaviour ball;

    private void Awake()
    {
		AnalyticsManager.LoadDatas();
		Time.timeScale = 1f;
		i = this;
		deadPlayers = new List<PlayerController>();
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
		if (mainCameraGO == null) { mainCameraGO = Camera.main.gameObject; }
        if (enemyManager == null) { enemyManager = FindObjectOfType<EnemyManager>(); }
		if (ball == null) { ball = FindObjectOfType<BallBehaviour>(); }

		FindMainCanvas();
		LoadMomentumManager();
		LoadEnergyManager();

        if (surrounderPlayerOne!= null) { Destroy(surrounderPlayerOne); }
        surrounderPlayerOne = Instantiate(SurrounderPrefab, playerOne.transform.position, Quaternion.identity);
        surrounderPlayerOne.GetComponent<Surrounder>().playerTransform = playerOne.transform;

        if (surrounderPlayerTwo != null) { Destroy(surrounderPlayerTwo); }
        surrounderPlayerTwo = Instantiate(SurrounderPrefab, playerTwo.transform.position, Quaternion.identity);
        surrounderPlayerTwo.GetComponent<Surrounder>().playerTransform = playerTwo.transform;

		CameraBehaviour.allCameras = FindObjectsOfType<CameraBehaviour>();

        //if (playerOne && playerTwo) { AssignPlayers(); }
    }

	private void OnDisable ()
	{
		AnalyticsManager.SaveDatas();
	}

	private void Update ()
	{
		if (deadPlayers.Count >= 2 && !deathPanelCalled)
		{
			deathPanelCalled = true;
			Instantiate(Resources.Load<GameObject>("Menu/RestartPanel"));
		}
		if (mainCanvas == null)
		{
			FindMainCanvas();
		}
		if (Input.GetKeyDown(KeyCode.B))
		{
			ResetBall();
		}
		if (Input.GetKeyDown(KeyCode.R))
		{
			ResetScene();
		}
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			if (SceneManager.GetActiveScene().buildIndex < SceneManager.sceneCountInBuildSettings - 1)
			{
				GameManager.LoadSceneByIndex(SceneManager.GetActiveScene().buildIndex + 1);
			}
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			if (SceneManager.GetActiveScene().buildIndex > 0)
			{
				GameManager.LoadSceneByIndex(SceneManager.GetActiveScene().buildIndex - 1);
			}
		}
		UpdateSceneLoader();
	}

	private void OnApplicationQuit ()
	{
		AnalyticsManager.SaveDatas();
		AnalyticsManager.SendDatas();
		GamePad.SetVibration(PlayerIndex.One, 0, 0);
		GamePad.SetVibration(PlayerIndex.Two, 0, 0);
	}

	public static void LoadSceneByIndex(int index)
	{
		SceneManager.LoadScene(index);
		GamePad.SetVibration(PlayerIndex.One, 0, 0);
		GamePad.SetVibration(PlayerIndex.Two, 0, 0);
		Time.timeScale = 1f;
	}
	public static void LoadNextScene ()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
		GamePad.SetVibration(PlayerIndex.One, 0, 0);
		GamePad.SetVibration(PlayerIndex.Two, 0, 0);
		Time.timeScale = 1f;
	}

	public static void OpenLevelMenu()
	{
		Time.timeScale = 0f;
		menu = Instantiate(Resources.Load<GameObject>("Menu/LevelMenu"), null).GetComponent<MainMenu>();
	}

	public static void CloseLevelMenu()
	{
		Time.timeScale = 1f;
		Destroy(menu.gameObject);
	}
	void UpdateSceneLoader()
	{
		GamePadState state = GamePad.GetState(PlayerIndex.One);
		for (int i = 0; i < 2; i++)
		{
			if (i == 0) { state = GamePad.GetState(PlayerIndex.One); }
			if (i == 1) { state = GamePad.GetState(PlayerIndex.Two); }
			if (state.Buttons.Start == ButtonState.Pressed)
			{
				if (i == 0) { if (menuCalledOne) { return; } }
				if (i == 1) { if (menuCalledTwo) { return; } }
				if (menu != null)
				{
					CloseLevelMenu();
					if (i == 0) { menuCalledOne = true; }
					if (i == 1) { menuCalledTwo = true; }
				} else
				{
					OpenLevelMenu();
					if (i == 0) { menuCalledOne = true; }
					if (i == 1) { menuCalledTwo = true; }
				}
			}
			if (state.Buttons.Start == ButtonState.Released)
			{
				if (i == 0) { menuCalledOne = false; }
				if (i == 1) { menuCalledTwo = false; }
			}
		}

		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			LoadSceneByIndex(0);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			LoadSceneByIndex(1);
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			LoadSceneByIndex(2);
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			LoadSceneByIndex(3);
		}
		if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			LoadSceneByIndex(4);
		}
		if (Input.GetKeyDown(KeyCode.Alpha6))
		{
			LoadSceneByIndex(5);
		}
		if (Input.GetKeyDown(KeyCode.Alpha7))
		{
			LoadSceneByIndex(6);
		}
		if (Input.GetKeyDown(KeyCode.Alpha8))
		{
			LoadSceneByIndex(7);
		}
		if (Input.GetKeyDown(KeyCode.Alpha9))
		{
			LoadSceneByIndex(8);
		}
		if (Input.GetKeyDown(KeyCode.Alpha0))
		{
			LoadSceneByIndex(9);
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

    void SpawnDummy()
    {
        Instantiate(dummyPrefab, playerOne.transform.position + (playerOne.transform.forward * 7f), Quaternion.identity);
        //if (playerOne && playerTwo) { AssignPlayers(); }
    }

	public void FindMainCanvas()
	{
		foreach (Canvas canvas in FindObjectsOfType<Canvas>())
		{
			if (canvas.renderMode != RenderMode.WorldSpace)
			{
				mainCanvas = canvas;
			}
		}
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
		GameObject i_newBall = Instantiate(i.ballPrefab, null);
		DontDestroyOnLoad(i_newBall);
		BallBehaviour.instance = i_newBall.GetComponent<BallBehaviour>();
        ball = i_newBall.GetComponent<BallBehaviour>();
		i_newBall.transform.position = playerOne.transform.position + new Vector3(0, 2, 0);
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
