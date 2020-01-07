using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class TurretGenerator : MonoBehaviour
{
    public float waitingTimeForNextEnemy;
    public float randomLenghtTime;
    public bool activeGenerator;

    public List<TurretBehaviour> listOfTurretPrefabToSpawn;

    public List<TurretBehaviour> spawnedTurret;

    [ReadOnly] public float currentTimer;

    // Start is called before the first frame update
    void Start()
    {
        currentTimer = waitingTimeForNextEnemy;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < spawnedTurret.Count; i++)
        {
            TurretBehaviour internal_item = (TurretBehaviour)spawnedTurret[i];
            if (internal_item.currentHealth < 0)
            {
                spawnedTurret.Remove(internal_item);
            }
        }

        if (spawnedTurret.Count == 0 & activeGenerator)
        {
            currentTimer -= Time.deltaTime;
        }
        if (currentTimer <= 0)
        {
            currentTimer = waitingTimeForNextEnemy + Random.Range(-randomLenghtTime, randomLenghtTime);
            var internal_newinst = Instantiate(listOfTurretPrefabToSpawn[Random.Range(0, listOfTurretPrefabToSpawn.Count)], transform.position, Quaternion.identity);
            internal_newinst.transform.position = new Vector3(internal_newinst.transform.position.x + Random.Range(-1f,1), internal_newinst.transform.position.y, internal_newinst.transform.position.z + Random.Range(-1f, 1));
            spawnedTurret.Add(internal_newinst);
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


