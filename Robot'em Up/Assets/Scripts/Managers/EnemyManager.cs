using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject enemyPrefab;

    public List<EnemyBehaviour> enemies;

    public GameObject playerOne;

    public GameObject surrounderPrefab;

    private GameObject surrounderInstance;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (surrounderInstance)
            {
                Destroy(surrounderInstance);
            }
            surrounderInstance = Instantiate(surrounderPrefab, playerOne.transform.position, Quaternion.identity);
            foreach (var enemy in enemies)
            {
                enemy.surrounder = surrounderInstance;
                StartCoroutine(enemy.SurroundPlayer(playerOne));
            }
        }
    }
}
