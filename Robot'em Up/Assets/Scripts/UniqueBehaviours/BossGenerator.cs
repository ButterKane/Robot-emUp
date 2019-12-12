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
        currentTimer -= Time.deltaTime * Boss_Manager.i.difficulty;
        if (currentTimer <= 0)
        {
            currentTimer = WaitingTimeForNextEnemy + Random.Range(-RandomLenghtTime, RandomLenghtTime);

            if (Boss_Manager.i.OnePlayerLeft)
            {
            }
            else
            {
                var newinst = Instantiate(listOfEnemiesPrefabToSpawn[Random.Range(0, listOfEnemiesPrefabToSpawn.Count)], transform.position, Quaternion.identity);
            newinst.transform.position = new Vector3(newinst.transform.position.x + Random.Range(-0.5f, 0.5f), newinst.transform.position.y, newinst.transform.position.z + Random.Range(-0.5f, 0.5f));
            }
        }
    }
}
