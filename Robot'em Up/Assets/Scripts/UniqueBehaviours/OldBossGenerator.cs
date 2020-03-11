using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using UnityEngine.Events;

public class OldBossGenerator : MonoBehaviour
{
    public float waitingTimeForNextEnemy;
    public float randomLenghtTime;
    public List<GameObject> listOfEnemiesPrefabToSpawn;
    public UnityEvent SpawnerToTrigger;

    [ReadOnly] public float currentTimer;

    // Start is called before the first frame update
    void Start()
    {
        currentTimer = waitingTimeForNextEnemy;
    }

    // Update is called once per frame
    void Update()
    {
        currentTimer -= Time.deltaTime * OldBoss_Manager.i.difficulty;
        if (currentTimer <= 0)
        {
            currentTimer = waitingTimeForNextEnemy + Random.Range(-randomLenghtTime, randomLenghtTime);

            if (OldBoss_Manager.i.OnePlayerLeft)
            {
            }
            else
            {
                SpawnerToTrigger.Invoke();
               // var newInst = Instantiate(listOfEnemiesPrefabToSpawn[Random.Range(0, listOfEnemiesPrefabToSpawn.Count)], transform.position, Quaternion.identity);
               // newInst.transform.position = new Vector3(newInst.transform.position.x + Random.Range(-0.5f, 0.5f), newInst.transform.position.y, newInst.transform.position.z + Random.Range(-0.5f, 0.5f));
            }
        }
    }
}
