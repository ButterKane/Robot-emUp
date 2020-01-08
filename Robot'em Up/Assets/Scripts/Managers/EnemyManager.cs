using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager i;

    [Separator("References")]
    public GameObject enemyPrefab;
    public List<Transform> enemySpawnPoints;

    // Auto-Assigned References
    [NonSerialized] public PlayerController playerOne;
    [NonSerialized] public PlayerController playerTwo;

    [ReadOnly] public List<EnemyBehaviour> enemies;
    [NonSerialized] public List<EnemyBehaviour> enemiesThatSurround;
    

    [NonSerialized] public List<EnemyBehaviour> enemyGroupOne;
    [NonSerialized] public List<EnemyBehaviour> enemyGroupTwo;
    [NonSerialized] public Vector3 groupOneMiddlePoint;
    [NonSerialized] public Vector3 groupTwoMiddlePoint;

    private void Awake()
    {
        if (i != null) { Destroy(i); }
        i = this;
    }
    public void Start()
    {
        playerOne = GameManager.playerOne;
        playerTwo = GameManager.playerTwo;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            SpawnEnemies();
        }

        GetMiddleOfEnemies();
    }

    public void GetMiddleOfEnemies()
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

    public void SetGroupsOfEnemies(out List<EnemyBehaviour> i_groupOne, out List<EnemyBehaviour> i_groupTwo)
    {
        i_groupOne = new List<EnemyBehaviour>();
        i_groupTwo = new List<EnemyBehaviour>();

        foreach (var enemy in enemiesThatSurround)
        {
            if (enemy.focusedPlayer != null)
            {
                if (enemy.focusedPlayer.gameObject == playerOne.gameObject)
                {
                    i_groupOne.Add(enemy);
                }
                else if (enemy.focusedPlayer.gameObject == playerTwo.gameObject)
                {
                    i_groupTwo.Add(enemy);
                }
                else { Debug.Log("The enemy " + enemy.name + " doesn't have a target"); }
            }
        }

        i_groupOne = GetClosestEnemies(i_groupOne);
        i_groupTwo = GetClosestEnemies(i_groupTwo);
    }

    public List<EnemyBehaviour> GetClosestEnemies(List<EnemyBehaviour> _enemies)
    {
        List<EnemyBehaviour> i_closeEnemies = new List<EnemyBehaviour>();

        if (_enemies.Count < 0)
        {
            Debug.LogWarning("The groupe of enemy is empty");
            return null;
        }

        _enemies.Sort((a, b) =>
        {
            var i_target = a.focusedPlayer;
            var i_dstToA = (i_target.transform.position - a.transform.position).magnitude;
            var i_dstToB = (i_target.transform.position - b.transform.position).magnitude;

            if (i_dstToA > i_dstToB) return 1;
            else if (i_dstToA < i_dstToB) return -1;
            else return 0;
        });

        for (int i = 0; i < Mathf.Min(GameManager.i.SurrounderPrefab.transform.childCount, _enemies.Count); i++)
        {
            if (_enemies[i] != null)
            {
                Debug.DrawRay(_enemies[i].transform.position, Vector3.up * 3, Color.yellow);
                i_closeEnemies.Add(_enemies[i]);   // Logiquement donc rangés par ordre du plus près au plus loin
            }
        }

        return i_closeEnemies;
    }

    public void SpawnEnemies()
    {
        for (int i = 0; i < 3; i++)
        {
            var i_newEnemy = Instantiate(enemyPrefab, enemySpawnPoints[i].position, Quaternion.identity);
            enemiesThatSurround.Add(i_newEnemy.GetComponent<EnemyBehaviour>());
        }
    }
}
