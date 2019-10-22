using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject enemyPrefab;

    public GameObject enemyCurrentlyAttacking = null;

    public List<EnemyBehaviour> enemies;

    public List<Transform> enemySpawnPoints;

    public GameObject playerOne;
    public GameObject playerTwo;

    public GameObject surrounderPrefab;

    private GameObject surrounderInstance;

    private bool isSurroundCooldownRunning;

    public void Start()
    {
        playerOne = GameManager.i.playerOne;
        playerTwo = GameManager.i.playerTwo;
        StartCoroutine(TimeBetweenSurrounding());
    }

    // Update is called once per frame
    void Update()
    {
        if (!isSurroundCooldownRunning)
        {
            Debug.Log("bidouille");
            StartCoroutine(TimeBetweenSurrounding());
            List<EnemyBehaviour> enemiesReadyToActOnOne = new List<EnemyBehaviour>();
            List<EnemyBehaviour> enemiesReadyToActOnTwo = new List<EnemyBehaviour>();

            CheckIfMultipleCloseEnemies(out enemiesReadyToActOnOne, out enemiesReadyToActOnTwo);

            LaunchSurrounding(enemiesReadyToActOnOne);
            LaunchSurrounding(enemiesReadyToActOnTwo);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            SpawnEnemies();
        }
    }

    private IEnumerator TimeBetweenSurrounding()
    {
        isSurroundCooldownRunning = true;
        yield return new WaitForSeconds(Random.Range(5, 10));
        isSurroundCooldownRunning = false;
    }

    private void CheckIfMultipleCloseEnemies(out List<EnemyBehaviour> enemiesReadyToActOnOne, out List<EnemyBehaviour> enemiesReadyToActOnTwo)
    {
        List<EnemyBehaviour> enemiesOnPlayerOne =  new List<EnemyBehaviour>();
        List<EnemyBehaviour> enemiesOnPlayerTwo = new List<EnemyBehaviour>();

        var p1 = playerOne.transform.position;
        var p2 = playerTwo.transform.position;
        
        foreach (var enemy in enemies)
        {
            float distanceToOne = (p1 - enemy.gameObject.transform.position).magnitude;
            float distanceToTwo = (p2 - enemy.gameObject.transform.position).magnitude;

            if (distanceToOne < distanceToTwo && distanceToTwo < enemy.attackDistance * 2.5f)
            {
                enemiesOnPlayerTwo.Add(enemy);
            }
            else if (distanceToTwo < distanceToOne && distanceToOne < enemy.attackDistance * 2.5f)
            {
                enemiesOnPlayerOne.Add(enemy);
            }
        }

        enemiesReadyToActOnOne = CheckIfEnemiesAreBusy(enemiesOnPlayerOne);
        enemiesReadyToActOnTwo = CheckIfEnemiesAreBusy(enemiesOnPlayerTwo);
    }

    private List<EnemyBehaviour> CheckIfEnemiesAreBusy(List<EnemyBehaviour> list)
    {
        List<EnemyBehaviour> availableEnemies = new List<EnemyBehaviour>();
        foreach(var enemy in list)
        {
            if (enemy.state != EnemyState.Attacking)
            {
                availableEnemies.Add(enemy);
            }
        }
        return availableEnemies;
    }

    /// <summary>
    /// Will order all enemies in the list to surround the player, if possible
    /// </summary>
    /// <param name="list"></param>
    private void LaunchSurrounding(List<EnemyBehaviour> list) 
    {
        Debug.Log("radouille");
        if (list.Count > 0)
        {
            Transform target = list[0].target;
            Debug.Log("target is " + target.gameObject.name);
            surrounderInstance = Instantiate(surrounderPrefab, target.position, Quaternion.identity);
            foreach (var enemy in list)
            {
                enemy.surrounder = surrounderInstance;
                StartCoroutine(enemy.SurroundPlayer(target.gameObject));
            }
        }
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
