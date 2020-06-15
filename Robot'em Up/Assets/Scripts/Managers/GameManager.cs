using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
    [NonSerialized] public static Cinemachine.CinemachineBrain cameraBrain;

    [NonSerialized] public static CameraGlobalSettings cameraGlobalSettings;

    public static float timeInZone;

    // UI stuff
    private static GameObject restartPanel;
    private static MainMenu mainMenu;
    private static bool menuCalledOne = false;
    private static bool menuCalledTwo = false;
    [NonSerialized] public bool waitForStartReset;
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

        cameraGlobalSettings = Resources.Load<CameraGlobalSettings>("CameraGlobalDatas");
        mainCamera = Camera.main;
        cameraBrain = Camera.main.GetComponent<Cinemachine.CinemachineBrain>();
        disabledInputs = new List<PlayerController>();
    }

    private void Start()
    {
        InstantiateMenus();
    }

    private void Update()
    {
        if (LoadingScreen.loading) { return; }
        timeInZone += Time.deltaTime;
        if (deadPlayers.Count >= 2 && !deathPanelCalled)
        {
            cameraBrain.enabled = false;
            Time.timeScale = 0.5f;
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
        //Reset button
        GamePadState state1 = GamePad.GetState(PlayerIndex.One);
        GamePadState state2 = GamePad.GetState(PlayerIndex.Two);
        if (state1.Buttons.Back == ButtonState.Pressed || state2.Buttons.Back == ButtonState.Pressed)
        {
            ResetScene();
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
        UnityAction newAction = new UnityAction(() => SceneManager.LoadScene(index));
        newAction += DestroyDDOL; 
        LoadingScreen.StartLoadingScreen(newAction);
        VibrationManager.CancelAllVibrations();
        Time.timeScale = PlayerPrefs.GetFloat("REU_GameSpeed", i.gameSpeed)/100 ;
        timeInZone = 0;
    }

    public static void LoadNextScene()
    {
        UnityAction newAction = new UnityAction(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1));
        newAction += DestroyDDOL;
        LoadingScreen.StartLoadingScreen(newAction);
        VibrationManager.CancelAllVibrations();
        Time.timeScale = PlayerPrefs.GetFloat("REU_GameSpeed", i.gameSpeed)/100 ;
        timeInZone = 0;
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
        mainMenu.waitForStartReset = true;
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
        mainMenu.isMainMenuActive = true;
        timeInZone = 0;
    }

    public static void CloseLevelMenu()
    {
        foreach (PlayerController p in disabledInputs)
        {
            p.EnableInput();
        }
        Time.timeScale = PlayerPrefs.GetFloat("REU_GameSpeed", i.gameSpeed)/100;
        mainMenu.mainMenuCanvas.enabled = false;
        mainMenu.isMainMenuActive = false;
    }

    public void FindMainCanvas()
    {
        foreach (Canvas canvas in FindObjectsOfType<Canvas>())
        {
            if (canvas.renderMode != RenderMode.WorldSpace && canvas.name != "LoadingScreenCanvas")
            {
                mainCanvas = canvas;
                DontDestroyOnLoad(mainCanvas.rootCanvas.transform);
                DDOL.Add(mainCanvas.gameObject);
            }
        }
    }

    public static void ResetScene()
    {
        UnityAction newAction = new UnityAction(() => SceneManager.LoadScene(GetCurrentZoneName()));
        newAction += DestroyDDOL;
        LoadingScreen.StartLoadingScreen(newAction);
        VibrationManager.CancelAllVibrations();
        timeInZone = 0;
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
        if(mainMenu == null) { mainMenu = mainCanvas.gameObject.GetComponentInChildren<MainMenu>(); }
        if (!mainMenu.DoesAbilityMenuExist()) { mainMenu.InitiateSubMenus(); }
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
                if (waitForStartReset) { return; }
                if (mainMenu != null && !mainMenu.mainMenuCanvas.enabled)
                {
                    OpenLevelMenu();
                    if (i == 0) { menuCalledOne = true; }
                    if (i == 1) { menuCalledTwo = true; }
                }
                //else if (mainMenu != null)
                //{
                //    CloseLevelMenu();
                //    if (i == 0) { menuCalledOne = true; }
                //    if (i == 1) { menuCalledTwo = true; }
                //}
            }
            if (state.Buttons.Start == ButtonState.Released)
            {
                if (i == 0) { menuCalledOne = false; }
                if (i == 1) { menuCalledTwo = false; }
                waitForStartReset = false;
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
        DDOL.Add(restartPanel);
        DontDestroyOnLoad(restartPanel);
        
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
