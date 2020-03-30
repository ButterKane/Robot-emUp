using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyMoveValues", menuName = "GlobalDatas/Enemy/EnemyMoveValues", order = 1)]
public class EnemyMoveValues : ScriptableObject
{
    public float randomSpeedMod;
    public float speedMultiplierFromPassHit;
    public float timeToRecoverSlowFromPass;
    public float speedMultiplierFromDunkHit;
    public float timeToRecoverSlowFromDunk;
}
