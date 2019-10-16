using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject enemyPrefab;

    public List<EnemyBehaviour> enemies;

    public GameObject playerOne;

   

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            foreach (var enemy in enemies)
            {
                StartCoroutine(enemy.SurroundPlayer(playerOne));
            }
        }
    }
}
