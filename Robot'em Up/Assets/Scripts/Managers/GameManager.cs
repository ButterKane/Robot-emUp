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
    public static List<PlayerController> players;

    [NonSerialized] public InputManager inputManager;

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
    public static List<PlayerController> disabledInputs;


    // Settings variables
    [Tooltip("gameSpeed: 100 = TimeScale: 1")]
    public float gameSpeed = 100; // as in 100% of normal speed
    [ReadOnly] public float damageTakenSettingsMod = 1;
    [ReadOnly] public float aimAssistanceSettingsMod = 0; // Get PlayerPrefs.GetFloat("REU_Assisting Aim", aimAssistanceSettingsMod);
    [ReadOnly] public int enemiesAgressivity = 1;
    [ReadOnly] public bool isDifficultyAdaptative = true;
    [ReadOnly] public int currentDifficultySetting = 0;

    private void Awake()
    {
        i = this;

        deathPanelCalled = false;
        DDOL = new List<GameObject>();
        Time.timeScale = (PlayerPrefs.GetFloat("REU_GameSpeed", gameSpeed)/100); 
        ChangeDifficulty(null, null); // initialisation, with base  
        deadPlayers = new List<PlayerController>();
        alivePlayers = new List<PlayerController>();
        players = new List<PlayerController>();
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
            players.Add(pc);
        }
        if (inputManager == null) { inputManager = FindObjectOfType<InputManager>(); }
        if (ball == null) { ball = FindObjectOfType<BallBehaviour>(); }

        FindMainCanvas();
        LoadMomentumManager();
        LoadEnergyManager();

        numberOfSurroundSpots = SurrounderPrefab.GetComponent<Surrounder>().points.Count;

        CreateSurroundersForPlayers();
        InstantiateMenus();

        cameraGlobalSettings = Resources.Load<CameraGlobalSettings>("CameraGlobalDatas");
        mainCamera = Camera.main;
        disabledInputs = new List<PlayerController>();
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
        VibrationManager.CancelAllVibrations();
        Time.timeScale = PlayerPrefs.GetFloat("REU_GameSpeed", i.gameSpeed)/100 ;
    }

    public static void LoadNextScene()
    {
        DestroyDDOL();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        VibrationManager.CancelAllVibrations();
        Time.timeScale = PlayerPrefs.GetFloat("REU_GameSpeed", i.gameSpeed)/100 ;
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
        foreach (PlayerController p in GameManager.players)
        {
            if (!p.IsInputDisabled())
            {
                p.DisableInput();
                disabledInputs.Add(p);
            }
        }
        Time.timeScale = 0f;
        VibrationManager.CancelAllVibrations();
        mainMenu.mainMenuCanvas.enabled = true;
    }

    public static void CloseLevelMenu()
    {
        foreach (PlayerController p in disabledInputs)
        {
            p.EnableInput();
        }
        Time.timeScale = PlayerPrefs.GetFloat("REU_GameSpeed", i.gameSpeed)/100;
        mainMenu.mainMenuCanvas.enabled = false;
    }

    public void FindMainCanvas()
    {
        foreach (Canvas canvas in FindObjectsOfType<Canvas>())
        {
            if (canvas.renderMode != RenderMode.WorldSpace)
            {
                mainCanvas = canvas;
                DontDestroyOnLoad(mainCanvas.rootCanvas.transform);
                DDOL.Add(mainCanvas.gameObject);
            }
        }
    }

    public static void ResetScene()
    {
        DestroyDDOL();
        SceneManager.LoadScene(GetCurrentZoneName());
        VibrationManager.CancelAllVibrations();
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
            DDOL.Add(i_newBall);
            BallBehaviour.instance = i_newBall.GetComponent<BallBehaviour>();
            ball = i_newBall.GetComponent<BallBehaviour>();
            ball.transform.position = playerOne.transform.position + new Vector3(0, 2, 0);
        }
    }

    public static void ChangeDifficulty(bool? _newAdaptative, int? _newDifficulty)
    {
        if (_newDifficulty == null)
        {
            i.currentDifficultySetting = PlayerPrefs.GetInt("REU_Overall Difficulty", i.currentDifficultySetting);
            if (i.currentDifficultySetting == 0) { i.isDifficultyAdaptative = true; }
            else { i.isDifficultyAdaptative = false; }
            return;
        }

        if (_newDifficulty == 0)
        {
            i.currentDifficultySetting = 0;
            i.isDifficultyAdaptative = true;
        }
        else
        {
            i.isDifficultyAdaptative = false;
            switch (_newDifficulty)
            {
                // Something with damage dealt/taken? Or with the amount of energy you get? Or the number of enemies?
                case 1:
                    i.currentDifficultySetting = 1;
                    break;
                case 2:
                    i.currentDifficultySetting = 2;
                    break;
                case 3:
                    i.currentDifficultySetting = 3;
                    break;
            }
        }
    }

    public static void PickedUpAnUpgrade(ConcernedAbility _concernedAbility, Upgrade _newAbilityLevel)
    {
        AbilityManager.UpgradeAbility(_concernedAbility, _newAbilityLevel);
        OpenLevelMenu();
        mainMenu.OpenAbilitiesMenuAtSpecificOne(_concernedAbility, _newAbilityLevel);
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
                if (mainMenu != null && !mainMenu.mainMenuCanvas.enabled)
                {
                    OpenLevelMenu();
                    if (i == 0) { menuCalledOne = true; }
                    if (i == 1) { menuCalledTwo = true; }
                }
                else if (mainMenu != null)
                {
                    CloseLevelMenu();
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
        gameObject.AddComponent<MomentumManager>();
        MomentumManager.datas = (MomentumData)Resources.Load("MomentumData");
    }

    void LoadEnergyManager()
    {
        gameObject.AddComponent<EnergyManager>();
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
        VibrationManager.CancelAllVibrations();
    }

    private void InstantiateMenus()
    {
        // the restart panel, for when you die
        if (restartPanel != null) { Destroy(restartPanel); }
        restartPanel = Instantiate(Resources.Load<GameObject>("Menu/RestartPanel"));
        restartPanel.SetActive(false);
        
        // the main Menu of the game
        if (mainMenu != null) { Destroy(mainMenu); }
        mainMenu = mainCanvas.gameObject.GetComponentInChildren<MainMenu>();
        mainMenu.InitiateSubMenus();
    }

    private static void DestroyDDOL()
    {
        if (DDOL == null) { return; }
        foreach (GameObject obj in DDOL)
        {
            if (DDOL != null)
            {
                Destroy(obj.gameObject);
            }
        }
        //GameManager.i = null;
    }
    #endregion
}
