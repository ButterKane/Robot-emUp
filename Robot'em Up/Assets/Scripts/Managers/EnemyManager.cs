using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("References")]
    public GameObject enemyPrefab;
    public List<Transform> enemySpawnPoints;
    public GameObject surrounderPrefab;

    [Space(2)]
    [Header("Auto-assigned References")]
    public GameObject playerOne;
    public GameObject playerTwo;
    public GameObject enemyCurrentlyAttacking = null;
    public List<EnemyBehaviour> enemies;

    public GameObject surrounderInstance;
    private bool isSurroundCooldownRunning;

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
    }

    public GameObject SpawnSurrounderInstance(Vector3 targetPosition)
    {
        surrounderInstance = Instantiate(surrounderPrefab, targetPosition, Quaternion.identity);
        return surrounderInstance;
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
