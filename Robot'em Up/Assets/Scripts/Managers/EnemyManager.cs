using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager i;
    
    private PlayerController playerOne;
    private PlayerController playerTwo;

    [ReadOnly] public List<EnemyBehaviour> enemies;
    [ReadOnly] public List<EnemyBehaviour> enemiesThatSurround;
    
    [NonSerialized] public List<EnemyBehaviour> enemyGroupOne;
    [NonSerialized] public List<EnemyBehaviour> enemyGroupTwo;
    [NonSerialized] public Vector3 groupOneMiddlePoint;
    [NonSerialized] public Vector3 groupTwoMiddlePoint;

    [Separator("Enemies agressivity")]
    [Space(10)]
    public float gentleAnticipationModifier = 1;
    public float gentleRestTimeModifier = 1.5f;
    public float gentleSpeedModifier = 0.7f;
    [Space(10)]
    public float normalAnticipationModifier = 1;
    public float normalRestTimeModifier = 1;
    public float normalSpeedModifier = 1f;
    [Space(10)]
    public float aggressiveAnticipationModifier = 0.7f;
    public float aggressiveRestTimeModifier = 0.1f;
    public float aggressiveSpeedModifier = 1.5f;


    private void Awake()
    {
        if (i != null) { Destroy(i); }
        i = this;
    }

    private void Start()
    {
        playerOne = GameManager.playerOne;
        playerTwo = GameManager.playerTwo;
        ChangeEnemiesAgressivity(PlayerPrefs.GetInt("REU_Enemies Agressivity", 1));
    }

    private void Update()
    {
        GetMiddleOfEnemies();
    }

    #region Public functions
    /// <summary>
    /// Checks if some enemies should be dead and aren't, and kills them
    /// </summary>
    public void KillDeadEnemies()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] != null && enemies[i].GetHealth() <= 0)
            {
                enemies[i].Kill();
                i--; // because it removes this enemy from the lists, so the counting would be wronged
            }
        }
    }

    public void RemoveEnemy(EnemyBehaviour _enemyToForget)
    {
        enemies.Remove(_enemyToForget);
        enemiesThatSurround.Remove(_enemyToForget);
    }
    #endregion

    #region Private functions
    /// <summary>
    /// Get the median point of each enemy group
    /// </summary>
    private void GetMiddleOfEnemies()
    {
        List<EnemyBehaviour> i_groupOne;
        List<EnemyBehaviour> i_groupTwo;

        groupOneMiddlePoint = new Vector3();
        groupTwoMiddlePoint = new Vector3();

        SetGroupsOfEnemies(out i_groupOne, out i_groupTwo);

        enemyGroupOne = i_groupOne;   // list of enemies targeting player one, sorted from closest to farthest to player
        enemyGroupTwo = i_groupTwo;   // same, but with the player two

        for (int i = 0; i < i_groupOne.Count; i++)
        {
            groupOneMiddlePoint += i_groupOne[i].transform.position;
        }

        for (int i = 0; i < i_groupTwo.Count; i++)
        {
            groupTwoMiddlePoint += i_groupTwo[i].transform.position;
        }

        groupOneMiddlePoint = new Vector3(groupOneMiddlePoint.x / i_groupOne.Count, groupOneMiddlePoint.y / i_groupOne.Count, groupOneMiddlePoint.z / i_groupOne.Count);
        groupTwoMiddlePoint = new Vector3(groupTwoMiddlePoint.x / i_groupTwo.Count, groupTwoMiddlePoint.y / i_groupTwo.Count, groupTwoMiddlePoint.z / i_groupTwo.Count);
    }

    /// <summary>
    /// Sets which enemies are in group 1 and group 2, to surround corresponding player
    /// </summary>
    /// <param name="i_groupOne"></param>
    /// <param name="i_groupTwo"></param>
    private void SetGroupsOfEnemies(out List<EnemyBehaviour> i_groupOne, out List<EnemyBehaviour> i_groupTwo)
    {
        i_groupOne = new List<EnemyBehaviour>();
        i_groupTwo = new List<EnemyBehaviour>();

        // Foreach enemy, check if he's focused on player1 or player 2, and make groups with that
        foreach (var enemy in enemiesThatSurround)
        {
            if (enemy.focusedPawnController != null)
            {
                if (enemy.focusedPawnController.gameObject == playerOne.gameObject)
                {
                    i_groupOne.Add(enemy);
                }
                else if (enemy.focusedPawnController.gameObject == playerTwo.gameObject)
                {
                    i_groupTwo.Add(enemy);
                }
                else { Debug.Log("The enemy " + enemy.name + " doesn't have a target"); }
            }
        }

        // Determine the closest enemies in the made groups
        i_groupOne = GetClosestEnemies(i_groupOne);
        i_groupTwo = GetClosestEnemies(i_groupTwo);
    }

    /// <summary>
    /// Creates a list of the closest enemies in the group
    /// </summary>
    /// <param name="_enemies"></param>
    /// <returns></returns>
    private List<EnemyBehaviour> GetClosestEnemies(List<EnemyBehaviour> _enemies)
    {
        List<EnemyBehaviour> i_closeEnemies = new List<EnemyBehaviour>();

        if (_enemies.Count < 0)
        {
            Debug.LogWarning("The group of enemy is empty");
            return null;
        }

        // Sort the group
        _enemies.Sort((a, b) =>
        {
            var i_target = a.focusedPawnController;
            var i_dstToA = (i_target.transform.position - a.transform.position).magnitude;
            var i_dstToB = (i_target.transform.position - b.transform.position).magnitude;

            if (i_dstToA > i_dstToB) return 1;
            else if (i_dstToA < i_dstToB) return -1;
            else return 0;
        });

        for (int i = 0; i < Mathf.Min(GameManager.i.numberOfSurroundSpots, _enemies.Count); i++)
        {
            if (_enemies[i] != null)
            {
                i_closeEnemies.Add(_enemies[i]);   // Logiquement donc rangés par ordre du plus près au plus loin
            }
        }

        return i_closeEnemies;
    }

    public void ChangeEnemiesAgressivity(int _newAgressivity)
    {
        float i_newWaitModifier = 1;
        float i_newAnticipationModifier = 1;
        float i_newSpeedMoveModifier = 1;
        switch (_newAgressivity)
        {
            case 0:
                i_newWaitModifier = gentleRestTimeModifier;
                i_newAnticipationModifier= gentleAnticipationModifier;
                i_newSpeedMoveModifier = gentleSpeedModifier;
                break;
            case 1:
                i_newWaitModifier = normalRestTimeModifier;
                i_newAnticipationModifier =normalAnticipationModifier;
                i_newSpeedMoveModifier = normalSpeedModifier;
                break;
            case 2:
                i_newWaitModifier = aggressiveRestTimeModifier;
                i_newAnticipationModifier= aggressiveAnticipationModifier;
                i_newSpeedMoveModifier = aggressiveSpeedModifier;
                break;
            default:
                break;
        }

        foreach(var enemy in enemies)
        {
            enemy.attackAnticipationSettingMod = i_newAnticipationModifier;
            enemy.attackPauseSettingMod = i_newWaitModifier;
            enemy.speedSettingsMod = i_newSpeedMoveModifier;
        }
    }

    public void ChangeAimAssistanceForAllEnemies(float _assistanceRatio)
    {
        foreach(var enemy in enemies)
        {
            enemy.ChangeAimAssistance(Mathf.Max((PlayerPrefs.GetFloat("REU_Assisting Aim", 50)/50), 0.4f));
        }
    }
    #endregion

}
