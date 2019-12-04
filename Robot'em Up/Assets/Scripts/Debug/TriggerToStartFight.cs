using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TriggerToStartFight : MonoBehaviour
{

    public EnemyBehaviour[] enemyArray;
    public TurretBehaviour[] turretArray;
    public Transform wallBehindPlayer;
    public CinemachineVirtualCamera myVC;
    public Vector3 pos1;
    public Vector3 pos2;
    public float timeForWallToCome;
    float timePassed;
    bool wallShouldMove;

    private void Update()
    {
        if (wallShouldMove)
        {
            timePassed += Time.deltaTime;
            wallBehindPlayer.position = Vector3.Lerp(pos1, pos2, timePassed / timeForWallToCome);
            if (timePassed / timeForWallToCome >= 1)
            {
                wallBehindPlayer.position = pos2;
                /*
                for (int i = 0; i < enemyArray.Length; i++)
                {
                    enemyArray[i].ChangingState(EnemyState.Idle);
                }
                for (int i = 0; i < turretArray.Length; i++)
                {
                    turretArray[i].ChangingState(TurretState.Idle);
                }
                */
                myVC.m_Priority = 0;
                gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !wallShouldMove)
        {
            wallShouldMove = true;
        }
    }
}
