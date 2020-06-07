using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyFocusValues", menuName = "GlobalDatas/Enemy/EnemyFocusValues", order = 1)]
public class EnemyFocusValues : ScriptableObject
{
    public float focusDistance = 3;
    public float maxHeightOfDetection = 3;
    public float unfocusDistance = 20;
    [HideInInspector] public float timeBetweenCheck = 0;
    public float distanceBeforeChangingPriority = 3;
    public float maxTimeBetweenCheck = 0.25f;
    public float closestDistanceToplayer = 2; // The closest a following enemy can go to a player without touching it
}
