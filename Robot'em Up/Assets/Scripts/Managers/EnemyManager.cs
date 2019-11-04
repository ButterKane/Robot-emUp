using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager i;

    [Header("References")]
    public GameObject enemyPrefab;
    public List<Transform> enemySpawnPoints;

    [Space(2)]
    [Header("Auto-assigned References")]
    public GameObject playerOne;
    public GameObject playerTwo;
    public GameObject enemyCurrentlyAttacking = null;
    public List<EnemyBehaviour> enemies;

    public List<EnemyBehaviour> enemyGroupOne;
    public List<EnemyBehaviour> enemyGroupTwo;
    public Vector3 groupOneMiddlePoint;
    public Vector3 groupTwoMiddlePoint;

    private void Awake()
    {
        if (i != null) { Destroy(i); }
        i = this;
    }
    public void Start()
    {
        playerOne = GameManager.i.playerOne;
        playerTwo = GameManager.i.playerTwo;
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
        List<EnemyBehaviour> groupOne;
        List<EnemyBehaviour> groupTwo;

        groupOneMiddlePoint = new Vector3();
        groupTwoMiddlePoint = new Vector3();

        SetGroupsOfEnemies(out groupOne, out groupTwo);

        enemyGroupOne = groupOne;   // list of enemies targeting player one, sorted from closest to farthest to player
        enemyGroupTwo = groupTwo;   // same, but with the player two

        for (int i = 0; i < groupOne.Count; i++)
        {
            groupOneMiddlePoint += groupOne[i].transform.position;
        }

        for (int i = 0; i < groupTwo.Count; i++)
        {
            groupTwoMiddlePoint += groupTwo[i].transform.position;
        }

        groupOneMiddlePoint = new Vector3(groupOneMiddlePoint.x / groupOne.Count, groupOneMiddlePoint.y / groupOne.Count, groupOneMiddlePoint.z / groupOne.Count);
        groupTwoMiddlePoint = new Vector3(groupTwoMiddlePoint.x / groupTwo.Count, groupTwoMiddlePoint.y / groupTwo.Count, groupTwoMiddlePoint.z / groupTwo.Count);
    }

    public void SetGroupsOfEnemies(out List<EnemyBehaviour> groupOne, out List<EnemyBehaviour> groupTwo)
    {
        groupOne = new List<EnemyBehaviour>();
        groupTwo = new List<EnemyBehaviour>();

        foreach (var enemy in enemies)
        {
            if (enemy.Target.gameObject == playerOne)
            {
                groupOne.Add(enemy);
            }
            else if (enemy.Target.gameObject == playerTwo)
            {
                groupTwo.Add(enemy);
            }
            else { Debug.Log("The enemy " + enemy.name + " doesn't have a target"); }
        }
        
        groupOne = GetClosestEnemies(groupOne);
        groupTwo = GetClosestEnemies(groupTwo);
    }

    public List<EnemyBehaviour> GetClosestEnemies(List<EnemyBehaviour> enemies)
    {
        List<EnemyBehaviour> closeEnemies = new List<EnemyBehaviour>();

        if (enemies.Count < 0)
        {
            Debug.LogWarning("The groupe of enemy is empty");
            return null;
        }

        enemies.Sort((a, b) => {
            var target = a.Target;
            var dstToA = (target.transform.position - a.transform.position).magnitude;
            var dstToB = (target.transform.position - b.transform.position).magnitude;

            if (dstToA > dstToB) return 1;
            else if (dstToA < dstToB) return -1;
            else return 0;
        });

        for (int i = 0; i < Mathf.Min(GameManager.i.SurrounderPrefab.transform.childCount, enemies.Count); i++)
        {
            if (enemies[i] != null)
            {
                Debug.DrawRay(enemies[i].transform.position, Vector3.up * 3, Color.yellow);
                closeEnemies.Add(enemies[i]);   // Logiquement donc rangés par ordre du plus près au plus loin
            }
        }
        
        return closeEnemies;
    }

    public void SpawnEnemies()
    {
        for (int i = 0; i < 3; i++)
        {
            var newEnemy = Instantiate(enemyPrefab, enemySpawnPoints[i].position, Quaternion.identity);
            enemies.Add(newEnemy.GetComponent<EnemyBehaviour>());
        }
    }
}
