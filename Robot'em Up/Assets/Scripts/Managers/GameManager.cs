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

public class DamageModifier
{
    public DamageModifier(float _multiplyCoef, float _duration, DamageModifierSource _source)
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

    [Separator("Public variables")]
    public int ballDamage = 30;
    public GameObject SurrounderPrefab;
    [NonSerialized] public int numberOfSurroundSpots = 0;
    public GameObject ballPrefab;

    // Auto-Assigned References
    [NonSerialized] public GameObject surrounderPlayerOne;
    [NonSerialized] public GameObject surrounderPlayerTwo;
    public static Canvas mainCanvas;
    public static List<PlayerController> deadPlayers;
    public static List<PlayerController> alivePlayers;

    [NonSerialized] public LevelManager levelManager;
    [NonSerialized] public InputManager inputManager;
    [NonSerialized] public EnemyManager enemyManager;

    [NonSerialized] public static PlayerController playerOne;
    [NonSerialized] public static PlayerController playerTwo;
    [NonSerialized] public static BallBehaviour ball;
    [NonSerialized] public static Camera mainCamera;

    [NonSerialized] public static CameraGlobalSettings cameraGlobalSettings;

    public static float timeInZone;

    // UI stuff
    private static GameObject restartPanel;
    private static MainMenu mainMenu;
    private static bool menuCalledOne = false;
    private static bool menuCalledTwo = false;
    private static bool deathPanelCalled = false;
    public static List<GameObject> DDOL;
    public static string currentZoneName;

    private void Awake()
    {
        deathPanelCalled = false;
        DDOL = new List<GameObject>();
        Time.timeScale = 1f;
        i = this;
        deadPlayers = new List<PlayerController>();
        alivePlayers = new List<PlayerController>();
        //Auto assign players
        foreach (PlayerController pc in FindObjectsOfType<PlayerController>())
        {
            if (pc.playerIndex == XInputDotNetPure.PlayerIndex.One)
            {
                playerOne = pc;
            }
            if (pc.playerIndex == XInputDotNetPure.PlayerIndex.Two)
            {
                playerTwo = pc;
            }
        }
        if (levelManager == null) { levelManager = FindObjectOfType<LevelManager>(); }
        if (inputManager == null) { inputManager = FindObjectOfType<InputManager>(); }
        if (enemyManager == null) { enemyManager = FindObjectOfType<EnemyManager>(); }
        if (ball == null) { ball = FindObjectOfType<BallBehaviour>(); }

        FindMainCanvas();
        LoadMomentumManager();
        LoadEnergyManager();

        numberOfSurroundSpots = SurrounderPrefab.GetComponent<Surrounder>().points.Count;

        CreateSurroundersForPlayers();

        InstantiateMenus();

        CameraBehaviour.allCameras = FindObjectsOfType<CameraBehaviour>();
        cameraGlobalSettings = Resources.Load<CameraGlobalSettings>("CameraGlobalDatas");
        mainCamera = Camera.main;
    }

    private void Update()
    {
        timeInZone += Time.deltaTime;
        if (deadPlayers.Count >= 2 && !deathPanelCalled)
        {
            deathPanelCalled = true;
            restartPanel.SetActive(true);
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
                LoadSceneByIndex(GetSceneIndexFromName(GetCurrentZoneName()) + 1);
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (SceneManager.GetActiveScene().buildIndex > 0)
            {
                LoadSceneByIndex(GetSceneIndexFromName(GetCurrentZoneName()) - 1);
            }
        }
        UpdateSceneLoader();
    }


    #region Public functions
    public static string GetCurrentZoneName()
    {
        return currentZoneName;
    }

    public static void ChangeCurrentZone(string _newZoneName)
    {
        currentZoneName = _newZoneName;
        timeInZone = 0;
    }

    public static float GetTimeInZone()
    {
        return timeInZone;
    }

    public static void LoadSceneByIndex(int index)
    {
        DestroyDDOL();
        SceneManager.LoadScene(index);
        GamePad.SetVibration(PlayerIndex.One, 0, 0);
        GamePad.SetVibration(PlayerIndex.Two, 0, 0);
        Time.timeScale = 1f;
    }

    public static void LoadNextScene()
    {
        DestroyDDOL();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        GamePad.SetVibration(PlayerIndex.One, 0, 0);
        GamePad.SetVibration(PlayerIndex.Two, 0, 0);
        Time.timeScale = 1f;
    }

    public static string GetSceneNameFromIndex(int _buildIndex)
    {
        string i_path = SceneUtility.GetScenePathByBuildIndex(_buildIndex);
        int i_slash = i_path.LastIndexOf('/');
        string i_name = i_path.Substring(i_slash + 1);
        int i_dot = i_name.LastIndexOf('.');
        return i_name.Substring(0, i_dot);
    }

    public static int GetSceneIndexFromName(string _sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string i_testedScreen = GetSceneNameFromIndex(i);
            if (i_testedScreen == _sceneName)
                return i;
        }
        return -1;
    }

    public static void OpenLevelMenu()
    {
        Time.timeScale = 0f;
        mainMenu.gameObject.SetActive(true);
    }

    public static void CloseLevelMenu()
    {
        Time.timeScale = 1f;
        mainMenu.gameObject.SetActive(false);
    }

    public void FindMainCanvas()
    {
        foreach (Canvas canvas in FindObjectsOfType<Canvas>())
        {
            if (canvas.renderMode != RenderMode.WorldSpace)
            {
                mainCanvas = canvas;
                DontDestroyOnLoad(mainCanvas.gameObject);
                DDOL.Add(mainCanvas.gameObject);
            }
        }
    }

    public static void ResetScene()
    {
        DestroyDDOL();
        SceneManager.LoadScene(GetCurrentZoneName());
    }

    public static void ResetBall()
    {
        if (ball != null)
        {
            ball.transform.position = playerOne.transform.position + new Vector3(0, 2, 0);
        }
        else
        {
            foreach (BallBehaviour ball in FindObjectsOfType<BallBehaviour>())
            {
                Destroy(ball.gameObject);
            }
            GameObject i_newBall = Instantiate(i.ballPrefab, null);
            DontDestroyOnLoad(i_newBall);
            BallBehaviour.instance = i_newBall.GetComponent<BallBehaviour>();
            ball = i_newBall.GetComponent<BallBehaviour>();
            ball.transform.position = playerOne.transform.position + new Vector3(0, 2, 0);
        }
    }
    #endregion

    #region Private functions
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
                if (mainMenu != null)
                {
                    CloseLevelMenu();
                    if (i == 0) { menuCalledOne = true; }
                    if (i == 1) { menuCalledTwo = true; }
                }
                else
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

    void LoadMomentumManager()
    {
        MomentumManager i_mm = gameObject.AddComponent<MomentumManager>();
        MomentumManager.datas = (MomentumData)Resources.Load("MomentumData");
    }

    void LoadEnergyManager()
    {
        EnergyManager i_em = gameObject.AddComponent<EnergyManager>();
        EnergyManager.datas = (EnergyData)Resources.Load("EnergyData");
    }

    private void CreateSurroundersForPlayers()
    {
        if (surrounderPlayerOne != null) { Destroy(surrounderPlayerOne); }
        surrounderPlayerOne = Instantiate(SurrounderPrefab, playerOne.transform.position, Quaternion.identity);
        surrounderPlayerOne.GetComponent<Surrounder>().playerTransform = playerOne.transform;

        if (surrounderPlayerTwo != null) { Destroy(surrounderPlayerTwo); }
        surrounderPlayerTwo = Instantiate(SurrounderPrefab, playerTwo.transform.position, Quaternion.identity);
        surrounderPlayerTwo.GetComponent<Surrounder>().playerTransform = playerTwo.transform;
    }

    private void OnApplicationQuit()
    {
        GamePad.SetVibration(PlayerIndex.One, 0, 0);
        GamePad.SetVibration(PlayerIndex.Two, 0, 0);
    }

    private void InstantiateMenus()
    {
        // the restart panel, for when you die
        if (restartPanel != null) { Destroy(restartPanel); }
        restartPanel = Instantiate(Resources.Load<GameObject>("Menu/RestartPanel"));
        restartPanel.SetActive(false);

        // the main Menu of the game
        if (mainMenu != null) { Destroy(mainMenu); }
        mainMenu = Instantiate(Resources.Load<GameObject>("Menu/LevelMenu"), null).GetComponent<MainMenu>();
        mainMenu.gameObject.SetActive(false);

    }

    private static void DestroyDDOL()
    {
        if (DDOL == null) { return; }
        foreach (GameObject obj in DDOL)
        {
            Destroy(obj.gameObject);
        }
        GameManager.i = null;
    }
    #endregion
}
