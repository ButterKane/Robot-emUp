using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class TurretGenerator : MonoBehaviour
{
    public float WaitingTimeForNextEnemy;
    public float RandomLenghtTime;
    public bool activeGenerator;


    public List<TurretBehaviour> listOfTurretPrefabToSpawn;


    public List<TurretBehaviour> SpawnedTurret;

    [ReadOnly] public float currentTimer;

    // Start is called before the first frame update
    void Start()
    {
        currentTimer = WaitingTimeForNextEnemy;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < SpawnedTurret.Count; i++)
        {
            TurretBehaviour item = (TurretBehaviour)SpawnedTurret[i];
            if (item.health < 0)
            {
                SpawnedTurret.Remove(item);
            }
        }


        if (SpawnedTurret.Count == 0 & activeGenerator)
        {
            currentTimer -= Time.deltaTime;
        }
        if (currentTimer <= 0)
        {
            currentTimer = WaitingTimeForNextEnemy + Random.Range(-RandomLenghtTime, RandomLenghtTime);
            var newinst = Instantiate(listOfTurretPrefabToSpawn[Random.Range(0, listOfTurretPrefabToSpawn.Count)], transform.position, Quaternion.identity);
            newinst.transform.position = new Vector3(newinst.transform.position.x + Random.Range(-1f,1), newinst.transform.position.y, newinst.transform.position.z + Random.Range(-1f, 1));
            SpawnedTurret.Add(newinst);


            }
    }

    public void StopGenerator()
    {
        activeGenerator = false;
    }


    public void ActiveGenerator()
    {
        activeGenerator = true;
    }
}


