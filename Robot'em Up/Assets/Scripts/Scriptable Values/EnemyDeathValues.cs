using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "EnemyDeathValues", menuName = "GlobalDatas/Enemy/EnemyDeathValues", order = 1)]
public class EnemyDeathValues : ScriptableObject
{
    public float coreDropChances = 1;
    public Vector2 minMaxDropForce;
    public Vector2 minMaxCoreHealthValue = new Vector2(1, 3);
    public float waitTimeBeforeDisappear = 1;
}
