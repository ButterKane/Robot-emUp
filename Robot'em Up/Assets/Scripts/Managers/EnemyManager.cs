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

        enemyGroupOne = groupOne;
        enemyGroupTwo = groupTwo;

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

        Debug.Assert(enemies.Count > 0, "The groupe of enemy is empty");

        enemies.Sort(SortByDistance);

        for (int i = 0; i < Mathf.Min(GameManager.i.SurrounderPrefab.transform.childCount, enemies.Count); i++)
        {
            if (enemies[i] != null)
                closeEnemies.Add(enemies[i]);
        }

        Debug.Assert(closeEnemies.Count > 0, "There isn't any ennemy recognized as close");

        return closeEnemies;
    }

    public int SortByDistance(EnemyBehaviour a, EnemyBehaviour b)
    {
        var target = a.Target;  // Get the concerned player

        var dstToA = (target.transform.position - a.transform.position).magnitude;
        var dstToB = (target.transform.position - b.transform.position).magnitude;

        return dstToA.CompareTo(dstToB);
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
