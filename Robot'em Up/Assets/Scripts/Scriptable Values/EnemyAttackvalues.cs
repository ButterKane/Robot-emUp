using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAttackValues", menuName = "GlobalDatas/Enemy/EnemyAttackValues", order = 1)]
public class EnemyAttackvalues : ScriptableObject
{
    public float maxAnticipationTime = 0.5f;
    public float maxTimePauseAfterAttack = 1;
    public Vector3 hitBoxOffset;
    [Range(0, 1)] public float rotationSpeedPreparingAttack = 0.2f;
    public float distanceToAttack = 5;
    [Range(0, 1)] public float portionOfAnticipationWithRotation = 0.3f;
    public float cooldownAfterAttackTime = 1f;
}
