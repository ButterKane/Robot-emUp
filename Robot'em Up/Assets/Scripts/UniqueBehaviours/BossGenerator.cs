using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class BossGenerator : MonoBehaviour
{
    public float WaitingTimeForNextEnemy;
    public float RandomLenghtTime;


    public List<GameObject> listOfEnemiesPrefabToSpawn;

    [ReadOnly] public float currentTimer;

    // Start is called before the first frame update
    void Start()
    {
        currentTimer = WaitingTimeForNextEnemy;
    }

    // Update is called once per frame
    void Update()
    {
        currentTimer -= Time.deltaTime;
        if (currentTimer <= 0)
        {
            currentTimer = WaitingTimeForNextEnemy + Random.Range(-RandomLenghtTime, RandomLenghtTime);

            Instantiate(listOfEnemiesPrefabToSpawn[Random.Range(0, listOfEnemiesPrefabToSpawn.Count)], transform.position, Quaternion.identity);

        }
    }
}
