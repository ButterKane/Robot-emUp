using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextRenderQfterFight : MonoBehaviour
{
    bool waitToBeActivated;
    float timeBetweenCheck;
    public Renderer myRend;
    GameObject[] enemyArray;

    // Update is called once per frame
    void Update()
    {
        timeBetweenCheck -= Time.deltaTime;
        if (waitToBeActivated && timeBetweenCheck < 0f)
        {
            timeBetweenCheck = 0.5f;
            bool isAllEnemyDead = true;
            for (int i = 0; i < enemyArray.Length; i++)
            {
                if(enemyArray[i] != null)
                {
                    isAllEnemyDead = false;
                }
            }
            if (isAllEnemyDead)
            {
                myRend.enabled = true;
                waitToBeActivated = false;
            }
        }
    }

    public void PleaseActivate()
    {
        waitToBeActivated = true;
        enemyArray = GameObject.FindGameObjectsWithTag("Enemy");
    }
}
