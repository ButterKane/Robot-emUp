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

    [NonSerialized] public List<EnemyBehaviour> enemies;

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
        List<EnemyBehaviour> internal_groupOne;
        List<EnemyBehaviour> internal_groupTwo;

        groupOneMiddlePoint = new Vector3();
        groupTwoMiddlePoint = new Vector3();

        SetGroupsOfEnemies(out internal_groupOne, out internal_groupTwo);

        enemyGroupOne = internal_groupOne;   // list of enemies targeting player one, sorted from closest to farthest to player
        enemyGroupTwo = internal_groupTwo;   // same, but with the player two

        for (int i = 0; i < internal_groupOne.Count; i++)
        {
            groupOneMiddlePoint += internal_groupOne[i].transform.position;
        }

        for (int i = 0; i < internal_groupTwo.Count; i++)
        {
            groupTwoMiddlePoint += internal_groupTwo[i].transform.position;
        }

        groupOneMiddlePoint = new Vector3(groupOneMiddlePoint.x / internal_groupOne.Count, groupOneMiddlePoint.y / internal_groupOne.Count, groupOneMiddlePoint.z / internal_groupOne.Count);
        groupTwoMiddlePoint = new Vector3(groupTwoMiddlePoint.x / internal_groupTwo.Count, groupTwoMiddlePoint.y / internal_groupTwo.Count, groupTwoMiddlePoint.z / internal_groupTwo.Count);
    }

    public void SetGroupsOfEnemies(out List<EnemyBehaviour> internal_groupOne, out List<EnemyBehaviour> internal_groupTwo)
    {
        internal_groupOne = new List<EnemyBehaviour>();
        internal_groupTwo = new List<EnemyBehaviour>();

        foreach (var enemy in enemies)
        {
            if (enemy.focusedPlayer != null)
            {
                if (enemy.focusedPlayer.gameObject == playerOne.gameObject)
                {
                    internal_groupOne.Add(enemy);
                }
                else if (enemy.focusedPlayer.gameObject == playerTwo.gameObject)
                {
                    internal_groupTwo.Add(enemy);
                }
                else { Debug.Log("The enemy " + enemy.name + " doesn't have a target"); }
            }
        }

        internal_groupOne = GetClosestEnemies(internal_groupOne);
        internal_groupTwo = GetClosestEnemies(internal_groupTwo);
    }

    public List<EnemyBehaviour> GetClosestEnemies(List<EnemyBehaviour> _enemies)
    {
        List<EnemyBehaviour> internal_closeEnemies = new List<EnemyBehaviour>();

        if (_enemies.Count < 0)
        {
            Debug.LogWarning("The groupe of enemy is empty");
            return null;
        }

        _enemies.Sort((a, b) =>
        {
            var internal_target = a.focusedPlayer;
            var internal_dstToA = (internal_target.transform.position - a.transform.position).magnitude;
            var internal_dstToB = (internal_target.transform.position - b.transform.position).magnitude;

            if (internal_dstToA > internal_dstToB) return 1;
            else if (internal_dstToA < internal_dstToB) return -1;
            else return 0;
        });

        for (int i = 0; i < Mathf.Min(GameManager.i.SurrounderPrefab.transform.childCount, _enemies.Count); i++)
        {
            if (_enemies[i] != null)
            {
                Debug.DrawRay(_enemies[i].transform.position, Vector3.up * 3, Color.yellow);
                internal_closeEnemies.Add(_enemies[i]);   // Logiquement donc rangés par ordre du plus près au plus loin
            }
        }

        return internal_closeEnemies;
    }

    public void SpawnEnemies()
    {
        for (int i = 0; i < 3; i++)
        {
            var internal_newEnemy = Instantiate(enemyPrefab, enemySpawnPoints[i].position, Quaternion.identity);
            enemies.Add(internal_newEnemy.GetComponent<EnemyBehaviour>());
        }
    }
}
